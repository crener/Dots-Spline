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
    [AddComponentMenu("Spline/3D/Bezier Spline")]
    public class BezierSpline3DSimple : BaseSpline3D, IArkableSpline
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
        public override void AddControlPoint(float3 point)
        {
            if(Points.Count == 0)
            {
                Points.Add(point);
            }
            else
            {
                float3 lastPoint = Points[Points.Count - 1];
                float3 delta = lastPoint - point;
                
                float3 up = math.cross(Vector3.right, math.cross(delta, new float3(1f, 0f, 0f)));
                Quaternion rotation = Quaternion.LookRotation(delta, up);
                float3 dir = (rotation * Vector3.forward);

                float3 lastPost = new float3(lastPoint - dir);
                float3 newPre = new float3(point + dir);

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
        public override void UpdateControlPoint(int index, float3 point, SplinePoint mode)
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
        public override void InsertControlPoint(int index, float3 point)
        {
            if(Points.Count < 1 || index >= ControlPointCount)
            {
                // add as there aren't enough points to insert between
                AddControlPoint(point);
                return;
            }

            if(index == 0)
            {
                // replace the first node
                float3 pre = math.lerp(point, Points[0], 0.3f);
                float3 post = math.lerp(Points[0], point, 0.3f);

                Points.Insert(0, point);
                Points.Insert(1, pre);
                Points.Insert(2, post);
            }
            else
            {
                int i = IndexMode(index, SplinePoint.Point);

                float3 lastPoint = Points[IndexMode(index - 1, SplinePoint.Point)];
                float3 delta = point - lastPoint;
                float angleXY1 = math.atan2(delta.x, delta.y);
                float angleYZ1 = math.atan2(delta.y, delta.z);

                float3 nextPoint = Points[IndexMode(index, SplinePoint.Point)];
                delta = point - nextPoint;
                float angleXY2 = math.atan2(delta.x, delta.y);
                float angleYZ2 = math.atan2(delta.y, delta.z);

                float combinedXY = (angleXY1 + angleXY2 + math.PI) / 2f;
                float combinedYZ = (angleYZ1 + angleYZ2 + math.PI) / 2f;

                float3 pre = new float3(
                    point.x - math.sin(combinedXY),
                    point.y - math.cos(combinedXY),
                    point.z + math.sin(combinedYZ));
                float3 post = new float3(
                    point.x + math.sin(combinedXY),
                    point.y + math.cos(combinedXY),
                    point.z - math.sin(combinedYZ));

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
        public float3 GetControlPoint(int i, SplinePoint point)
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
        public override float3 GetControlPoint(int i)
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
        protected override float3 SplineInterpolation(float t, int a)
        {
            float3 p0 = GetControlPoint(a, SplinePoint.Point);
            float3 p1 = GetControlPoint(a, SplinePoint.Post);
            float3 p2 = GetControlPoint((a + 1) % SegmentPointCount, SplinePoint.Pre);
            float3 p3 = GetControlPoint((a + 1) % SegmentPointCount, SplinePoint.Point);

            return BezierMath.CubicBezierPoint(t, p0, p1, p2, p3);
        }

        public override SplineType SplineDataType
        {
            get
            {
                if(ControlPointCount == 0) return SplineType.Empty;
                if(ControlPointCount == 1) return SplineType.Single;

                if(ArkParameterization) return SplineType.Linear;
                else return SplineType.Bezier;
            }
        }

        public override void Convert(Unity.Entities.Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<Spline3DData>(entity);
            Spline3DData splineData = ConvertData();
            SplineEntityData = splineData;
            dstManager.SetSharedComponentData(entity, splineData);
        }

        protected override Spline3DData ConvertData()
        {
            ClearData();

            if(ArkParameterization && SegmentPointCount >= 2)
            {
                Assert.IsFalse(hasSplineEntityData);
                SplineEntityData = SplineArkConversion(ArkLength);
            }
            else
            {
                NativeArray<float3> points = new NativeArray<float3>(Points.ToArray(), Allocator.Persistent);
                NativeArray<float> time = new NativeArray<float>(SegmentLength.ToArray(), Allocator.Persistent);

                Assert.IsFalse(hasSplineEntityData);
                SplineEntityData = new Spline3DData
                {
                    Length = Length(),
                    Points = points,
                    Time = time
                };
            }

            return SplineEntityData.Value;
        }
    }
}