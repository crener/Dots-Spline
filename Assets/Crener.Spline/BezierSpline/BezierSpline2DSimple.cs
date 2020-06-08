using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Crener.Spline.BaseSpline;
using Crener.Spline.Common;
using Crener.Spline.Common.DataStructs;
using Crener.Spline.Common.Interfaces;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace Crener.Spline.BezierSpline
{
    /// <summary>
    /// Standard 2D spline along the XY axis
    /// </summary>
    public class BezierSpline2DSimple : BaseSpline2D, IArkableSpline
    {
        protected const int c_floatsPerControlPoint = 3;

        [SerializeField]
        protected List<SplineEditMode> PointEdit = new List<SplineEditMode>();
        [SerializeField, Tooltip("Ensures constant length between points in spline"), FormerlySerializedAs("arkParameterization")]
        private bool arkParameterization = false;
        [SerializeField, FormerlySerializedAs("arkLength")]
        private float arkLength = 0.1f;

        /// <inheritdoc cref="BaseSpline2D"/>
        public override int ControlPointCount => Points.Count == 0 ? 0 : (int) math.ceil(Points.Count / 3f);

        public bool ArkParameterization
        {
            get => arkParameterization;
            set
            {
                if(arkParameterization != value)
                {
                    arkParameterization = value;

                    ClearData();
                }
            }
        }

        public float ArkLength
        {
            get => arkLength;
            set
            {
                if(arkLength != value)
                {
                    arkLength = value;
                    ClearData();
                }
            }
        }

        /// <summary>
        /// Adds a point to the end of the spline
        /// </summary>
        public override void AddControlPoint(float2 point)
        {
            if(Points.Count == 0)
            {
                Points.Add(point);
            }
            else
            {
                float2 lastPoint = Points[Points.Count - 1];
                float2 delta = lastPoint - point;
                float angle = math.atan2(delta.y, delta.x) - (math.PI / 2);

                float2 lastPost = new float2(
                    lastPoint.x + math.sin(angle),
                    lastPoint.y + math.cos(angle));
                float2 newPre = new float2(
                    point.x + math.sin(-angle),
                    point.y + math.cos(-angle));

                Points.Add(lastPost);
                Points.Add(newPre);
                Points.Add(point);
            }

            PointEdit.Add(SplineEditMode.Standard);
            RecalculateLengthBias();
        }

        /// <summary>
        /// Update an existing control points data
        /// </summary>
        /// <param name="index">control point index</param>
        /// <param name="point">location of the point</param>
        /// <param name="mode">type of point to update</param>
        public override void UpdateControlPoint(int index, float2 point, SplinePoint mode)
        {
            Assert.IsTrue(index <= ControlPointCount);

            int i = IndexMode(index, mode);
            Points[i] = point;

            RecalculateLengthBias();
        }

        /// <summary>
        /// inserts a point before the specified segment index
        /// </summary>
        /// <param name="index">segment index</param>
        /// <param name="point">location to insert</param>
        public override void InsertControlPoint(int index, float2 point)
        {
            if(Points.Count <= 1 || index >= ControlPointCount)
            {
                // add as there aren't enough points to insert between
                AddControlPoint(point);
                return;
            }

            if(index == 0)
            {
                // replace the first node
                float2 pre = math.lerp(point, Points[0], 0.3f);
                float2 post = math.lerp(Points[0], point, 0.3f);

                Points.Insert(0, point);
                Points.Insert(1, pre);
                Points.Insert(2, post);
            }
            else
            {
                int i = IndexMode(index, SplinePoint.Point);

                float2 lastPoint = Points[IndexMode(index - 1, SplinePoint.Point)];
                float2 delta = point - lastPoint;
                float angle = math.atan2(delta.x, delta.y);

                float2 nextPoint = Points[IndexMode(index, SplinePoint.Point)];
                delta = point - nextPoint;
                float angle2 = math.atan2(delta.x, delta.y);
                float superAngle = (angle + angle2 + math.PI) / 2f;

                float2 pre = new float2(
                    point.x - math.sin(superAngle),
                    point.y - math.cos(superAngle));
                float2 post = new float2(
                    point.x + math.sin(superAngle),
                    point.y + math.cos(superAngle));

                Points.Insert(i - 1, pre);
                Points.Insert(i, point);
                Points.Insert(i + 1, post);
            }

            PointEdit.Insert(index, SplineEditMode.Standard);
            RecalculateLengthBias();
        }

        /// <summary>
        /// Remove existing control points data
        /// </summary>
        /// <param name="index">control point index</param>
        public override void RemoveControlPoint(int index)
        {
            if(ControlPointCount == 0 || index < 0) return;
            if(ControlPointCount == 1)
            {
                Points.RemoveAt(0);
                PointEdit.RemoveAt(0);
                return;
            }

            if(index == 0)
            {
                Points.RemoveRange(0, c_floatsPerControlPoint);
            }
            else if(index >= ControlPointCount - 1)
            {
                int startIndex = math.max(0, IndexMode(ControlPointCount - 1, SplinePoint.Pre) - 1);
                Points.RemoveRange(startIndex, c_floatsPerControlPoint);

                // fixes the index for later if it is greater than the amount of control points
                index = ControlPointCount - 1;
            }
            else
            {
                int startIndex = math.max(0, IndexMode(index, SplinePoint.Pre));
                int endIndex = math.max(0, IndexMode(index, SplinePoint.Post));
                Points.RemoveRange(startIndex, (endIndex - startIndex) + 1);
            }

            PointEdit.RemoveAt(index);
            RecalculateLengthBias();
        }


        /// <summary>
        /// Get the edit mode for a control point 
        /// </summary>
        /// <param name="index"> control point index</param>
        /// <returns>edit mode for the control point</returns>
        public override SplineEditMode GetEditMode(int index)
        {
            return PointEdit[index];
        }

        /// <summary>
        /// Change the edit mode of a control point
        /// </summary>
        /// <param name="index">control point index</param>
        /// <param name="mode">new control point edit mode</param>
        public override void ChangeEditMode(int index, SplineEditMode mode)
        {
            PointEdit[index] = mode;
        }

        /// <summary>
        /// Gets the given point from a point segment
        /// </summary>
        /// <param name="i">index of the segment</param>
        /// <param name="point">type of point to get</param>
        /// <returns>World Space position for the point</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float2 GetControlPoint(int i, SplinePoint point)
        {
            int index = IndexMode(i, point);

#if UNITY_EDITOR
            Assert.IsTrue(index >= 0 && index < Points.Count,
                $"Index out of range! index: {index}, point Count: {Points.Count}");
#endif

            return Points[index];
        }

        /// <summary>
        /// Gets the given point from a point segment
        /// </summary>
        /// <param name="i">index of the segment</param>
        /// <returns>World Space position for the point</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override float2 GetControlPoint(int i)
        {
            return GetControlPoint(i, SplinePoint.Point);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int IndexMode(int i, SplinePoint point)
        {
            switch (point)
            {
                case SplinePoint.Point:
                    return (i * c_floatsPerControlPoint);
                case SplinePoint.Post:
                    return (i * c_floatsPerControlPoint) + 1;
                case SplinePoint.Pre:
                    return (i * c_floatsPerControlPoint) - 1;
            }

            throw new ArgumentException("Unexpected enum value", nameof(point));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override float2 SplineInterpolation(float t, int a, int b)
        {
            float2 p0 = GetControlPoint(a, SplinePoint.Point);
            float2 p1 = GetControlPoint(a, SplinePoint.Post);
            float2 p2 = GetControlPoint(b, SplinePoint.Pre);
            float2 p3 = GetControlPoint(b, SplinePoint.Point);

            return BezierMath.CubicBezierPoint(t, p0, p1, p2, p3);
        }

        public override SplineType SplineDataType => ArkParameterization ? SplineType.PointToPoint : SplineType.Bezier;

        public override void Convert(Unity.Entities.Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<Spline2DData>(entity);
            Spline2DData splineData = ConvertData();
            SplineEntityData = splineData;
            dstManager.SetSharedComponentData(entity, splineData);
        }

        protected override Spline2DData ConvertData()
        {
            ClearData();

            if(ArkParameterization)
            {
                Assert.IsFalse(hasSplineEntityData);
                SplineEntityData = SplineArkConversion();
            }
            else
            {
                NativeArray<float2> points = new NativeArray<float2>(Points.ToArray(), Allocator.Persistent);
                NativeArray<float> time = new NativeArray<float>(SegmentLength.ToArray(), Allocator.Persistent);

                Assert.IsFalse(hasSplineEntityData);
                SplineEntityData = new Spline2DData
                {
                    Length = Length(),
                    Points = points,
                    Time = time
                };
            }

            return SplineEntityData.Value;
        }

        /// <summary>
        /// c
        /// </summary>
        /// <returns></returns>
        private Spline2DData SplineArkConversion()
        {
            float previousTime = 0;
            float normalizedArkLength = math.max(0.001f, ArkLength);
            float splineLength = Length();
            double splineCompleted = 0f;
            List<float2> points = new List<float2>((int) (splineLength / normalizedArkLength * 1.3f));
            List<float> times = new List<float>(points.Count);

            const int perPointIterationAttempts = 100; // lower values = fast, high values = high precision but longer generation times

            for (int i = 0; i < ControlPointCount - 1; i++)
            {
                float currentTime = SegmentLength[i];
                float sectionLength = Length() * currentTime;
                float pointCount = ((currentTime - previousTime) * sectionLength) / normalizedArkLength;
                previousTime = currentTime;

                double previousProgress = 0f;
                float2 previous = GetPoint((float) previousProgress, i);
                points.Add(previous); // directly add control point

                if(i > 0)
                {
                    times.Add((float) splineCompleted / splineLength);
                }

                double sectionLengthDone = 0f;

                for (int j = 0; j < pointCount - 1; j++)
                {
                    // binary search to get best case distance from (expected) previous point
                    double currentProgress = 0.5f;
                    double currentSize = 0.5f;

                    float distance = normalizedArkLength;
                    float targetDistance = normalizedArkLength * (j + 1);

                    float2 point = previous;
                    int attempts = -1;
                    while (++attempts < perPointIterationAttempts)
                    {
                        point = GetPoint((float) currentProgress, i);

                        distance = math.distance(previous, point);
                        if(math.abs((sectionLengthDone + distance) - targetDistance) < 0.0000005f)
                            break;

                        currentSize /= 2;
                        if(distance > normalizedArkLength)
                        {
                            if(currentProgress <= previousProgress) currentProgress = previousProgress;
                            else currentProgress -= currentSize;
                        }
                        else currentProgress += currentSize;
                    }

                    sectionLengthDone += distance;
                    splineCompleted += distance;
                    points.Add(point);
                    times.Add((float) splineCompleted / splineLength);

                    previous = point;
                    previousProgress = currentProgress;
                }
            }

            if(ControlPointCount >= 2)
            {
                // add final control point
                points.Add(Points[Points.Count - 1]);
            }

            times.Add(1f);

            return new Spline2DData
            {
                Length = Length(),
                Points = new NativeArray<float2>(points.ToArray(), Allocator.Persistent),
                Time = new NativeArray<float>(times.ToArray(), Allocator.Persistent)
            };
        }
    }
}