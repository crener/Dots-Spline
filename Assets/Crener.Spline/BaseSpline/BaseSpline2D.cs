using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Crener.Spline.Common;
using Crener.Spline.Common.DataStructs;
using Crener.Spline.Common.Interfaces;
using Unity.Assertions;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Crener.Spline.BaseSpline
{
    /// <summary>
    /// Base implementation which contains base functionality and reusable methods
    /// </summary>
    public abstract class BaseSpline2D : BaseSpline, ISpline2DEditor
    {
        [SerializeField]
        protected List<float2> Points = new List<float2>();

        public Spline2DData? SplineEntityData2D
        {
            get
            {
                if(!hasSplineEntityData) ConvertData();
                return m_splineData2D;
            }
            protected set => m_splineData2D = value;
        }

        /// <summary>
        /// true if Spline Entity Data has been initialized, calling <see cref="SplineEntityData2D"/> directly will automatically generate data
        /// </summary>
        public override bool hasSplineEntityData => m_splineData2D.HasValue;

        /// <summary>
        /// Amount of control points in the spline
        /// </summary>
        public override int ControlPointCount => Points.Count;

        /// <summary>
        /// Is the data in the spline initialized
        /// </summary>
        protected virtual bool DataInitialized => SegmentPointCount > 0 && SegmentLength.Count >= 0;
        
        private Spline2DData? m_splineData2D = null;

        /// <summary>
        /// Retrieve a point on the spline at a specific control point
        /// </summary>
        /// <returns>point on spline segment</returns>
        public virtual float2 Get2DPoint(float progress, int index)
        {
            return Position.xy + SplineInterpolation(progress, index);
        }

        /// <summary>
        /// Adds a point to the end of the spline
        /// </summary>
        public virtual void AddControlPoint(float2 point)
        {
            Points.Add(point);
            RecalculateLengthBias();
        }

        /// <summary>
        /// inserts a point before the specified segment index
        /// </summary>
        /// <param name="index">segment index</param>
        /// <param name="point">location to insert</param>
        public virtual void InsertControlPoint(int index, float2 point)
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
                Points.Insert(0, point);
            }
            else
            {
                Points.Insert(index, point);
            }

            RecalculateLengthBias();
        }

        /// <summary>
        /// Remove existing control points data
        /// </summary>
        /// <param name="index">control point index</param>
        public override void RemoveControlPoint(int index)
        {
            if(ControlPointCount == 0 || index < 0) return;

            Points.RemoveAt(math.min(index, ControlPointCount - 1));
            RecalculateLengthBias();
        }

        /// <summary>
        /// Update an existing control points data
        /// </summary>
        /// <param name="index">control point index</param>
        /// <param name="point">location of the point</param>
        /// <param name="mode">type of point to update</param>
        public virtual void UpdateControlPoint(int index, float2 point, SplinePoint mode)
        {
            Assert.IsTrue(index <= ControlPointCount);

            Points[index] = point;
            RecalculateLengthBias();
        }

        /// <summary>
        /// Move all points by <paramref name="delta"/> amount
        /// </summary>
        /// <param name="delta">amount to move all point by</param>
        public void MoveControlPoints(float2 delta)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                // float2 is struct so it needs to override the stored value
                float2 local = Points[i] += delta;
                Points[i] = local;
            }
        }

        /// <summary>
        /// Relieve a point on the spline
        /// </summary>
        /// <param name="progress"></param>
        /// <returns>point on spline</returns>
        public virtual float2 Get2DPoint(float progress)
        {
            float2 translation = Position.xy;
            if(ControlPointCount == 0)
                return translation;
            else if(progress <= 0f || ControlPointCount == 1)
                return translation + GetControlPoint2DLocal(0);
            else if(progress >= 1f)
            {
                if(this is ILoopingSpline looped && looped.Looped)
                    return translation + GetControlPoint2DLocal(0);
                return translation + GetControlPoint2DLocal((ControlPointCount - 1));
            }

            int aIndex = FindSegmentIndex(progress);
            float pointProgress = SegmentProgress(progress, aIndex);
            return translation + SplineInterpolation(pointProgress, aIndex);
        }

        protected override float LengthBetweenPoints(int a, int resolution = 64)
        {
            float currentLength = 0;

            float2 aPoint = SplineInterpolation(0f, a);
            for (float i = 1; i <= resolution; i++)
            {
                float2 bPoint = SplineInterpolation(i / resolution, a);
                currentLength += math.distance(aPoint, bPoint);
                aPoint = bPoint;
            }

            return currentLength;
        }

        /// <summary>
        /// Take the <paramref name="position"/> in local space and return the world space location
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected float2 ConvertToWorldSpace(float2 position)
        {
            return (Position + (float3)(Forward * new float3(position, 0f))).xy;
        }

        /// <summary>
        /// Take the <paramref name="position"/>s in local space and convert all to world space locations
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected IEnumerable<float2> ConvertToWorldSpace(IEnumerable<float2> position)
        {
            float2 transPosCache = Position.xy;
            foreach (float2 pos in position)
            {
                yield return transPosCache + ((float3) (Forward * new float3(pos, 0f))).xy;
            }
        }

        /// <summary>
        /// Take the <paramref name="position"/> in world space and return the local space location
        /// </summary>
        protected float2 ConvertToLocalSpace(float2 position)
        {
            return (Position - (float3)(Quaternion.Inverse(Forward) * new float3(position, 0f))).xy;
        }

        /// <summary>
        /// Gets the given point from a point segment
        /// </summary>
        /// <param name="i">index of the segment</param>
        /// <returns>World Space position for the point</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual float2 GetControlPoint2DLocal(int i)
        {
            return Points[i];
        }

        /// <summary>
        /// Gets the given point from a point segment
        /// </summary>
        /// <param name="i">index of the segment</param>
        /// <returns>World Space position for the point</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual float2 GetControlPoint2DWorld(int i)
        {
            return Points[i] + Position.xy;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract float2 SplineInterpolation(float t, int a);

        public override void ClearData()
        {
            if(hasSplineEntityData) // access directly to stop possible infinite loop
            {
                SplineEntityData2D.Value.Dispose();
                SplineEntityData2D = null;
            }
        }

        private void Reset()
        {
            Points.Clear();

            Vector3 position = transform.position;
            AddControlPoint(new float2(position.x - 2f, position.y));
            AddControlPoint(new float2(position.x + 2f, position.y));
        }

        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.gray;
            const float pointDensity = 13;
            const int maxPointAmount = 15000;

            if(!DataInitialized)
            {
                // needs to calculate length as it might not have been saved correctly after saving
                RecalculateLengthBias();
            }

            for (int i = 0; i < SegmentPointCount - 1; i++)
            {
                float2 f = Get2DPoint(0f, i);
                Vector3 lp = new Vector3(f.x, f.y, 0f);
                int points = math.min((int) (pointDensity * (SegmentLength[i] * Length())), maxPointAmount);

                for (int s = 1; s <= points; s++)
                {
                    float progress = s / (float) points;
                    float2 p = Get2DPoint(progress, i);
                    Vector3 point = new Vector3(p.x, p.y, 0f);

                    Gizmos.DrawLine(lp, point);
                    lp = point;
                }
            }
        }

        protected virtual Spline2DData ConvertData()
        {
            ClearData();

            if(SegmentPointCount >= 2 && this is IArkableSpline arkSpline && arkSpline.ArkParameterization)
            {
                SplineEntityData2D = SplineArkConversion(arkSpline.ArkLength);
                return SplineEntityData2D.Value;
            }

            bool looped = this is ILoopingSpline loopSpline && loopSpline.Looped;
            float2[] pointData = new float2[Points.Count + (looped ? 1 : 0)];

            for (int i = 0; i < Points.Count; i++)
            {
                pointData[i] = Position.xy + Points[i];
            }

            if(looped) pointData[Points.Count] = pointData[0];

            NativeArray<float2> points = new NativeArray<float2>(pointData, Allocator.Persistent);
            NativeArray<float> time = new NativeArray<float>(SegmentLength.ToArray(), Allocator.Persistent);

            Assert.IsFalse(hasSplineEntityData);
            SplineEntityData2D = new Spline2DData
            {
                Length = Length(),
                Points = points,
                Time = time
            };

            return SplineEntityData2D.Value;
        }

        /// <summary>
        /// Convert the spline into smaller linear segments with an equal distance between each point (see: <see cref="arkLength"/>)
        /// </summary>
        /// <returns>Linear spline data</returns>
        protected virtual Spline2DData SplineArkConversion(float arkLength)
        {
            float previousTime = 0;
            float normalizedArkLength = math.max(0.001f, arkLength);
            float splineLength = Length();
            double splineCompleted = 0f;
            List<float2> points = new List<float2>((int) (splineLength / normalizedArkLength * 1.3f));
            List<float> times = new List<float>(points.Count);

            const int perPointIterationAttempts = 100; // lower values = fast, high values = high precision but longer generation times

            for (int i = 0; i < SegmentLength.Count; i++)
            {
                float currentTime = SegmentLength[i];
                float sectionLength = Length() * currentTime;
                float pointCount = ((currentTime - previousTime) * sectionLength) / normalizedArkLength;
                previousTime = currentTime;

                double previousProgress = 0f;
                float2 previous = Get2DPoint((float) previousProgress, i);
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
                        point = Get2DPoint((float) currentProgress, i);

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