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
    public abstract class BaseSpline3D : BaseSpline, ISpline3DEditor
    {
        [SerializeField]
        protected List<float3> Points = new List<float3>();

        private Spline3DData? m_splineData = null;

        public Spline3DData? SplineEntityData3D
        {
            get
            {
                if(!hasSplineEntityData) ConvertData();
                return m_splineData;
            }
            protected set => m_splineData = value;
        }

        /// <summary>
        /// true if Spline Entity Data has been initialized, calling <see cref="SplineEntityData3D"/> directly will automatically generate data
        /// </summary>
        public override bool hasSplineEntityData => m_splineData.HasValue;

        /// <summary>
        /// Amount of control points in the spline
        /// </summary>
        public override int ControlPointCount => Points.Count;

        /// <summary>
        /// Retrieve a point on the spline at a specific control point
        /// </summary>
        /// <returns>point on spline segment</returns>
        public float3 Get3DPoint(float progress, int index)
        {
            return ConvertToWorldSpace(SplineInterpolation(progress, index));
        }

        /// <summary>
        /// Adds a point to the end of the spline
        /// </summary>
        public virtual void AddControlPoint(float3 point)
        {
            Points.Add(point);
            RecalculateLengthBias();
        }

        /// <summary>
        /// inserts a point before the specified segment index
        /// </summary>
        /// <param name="index">segment index</param>
        /// <param name="point">location to insert</param>
        public virtual void InsertControlPointWorldSpace(int index, float3 point)
        {
            InsertControlPointLocalSpace(index, ConvertToLocalSpace(point));
        }

        public virtual void InsertControlPointLocalSpace(int index, float3 point)
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
        public virtual void UpdateControlPointWorld(int index, float3 point, SplinePoint mode)
        {
            UpdateControlPointLocal(index, ConvertToLocalSpace(point), mode);
        }

        /// <summary>
        /// Update an existing control points data
        /// </summary>
        /// <param name="index">control point index</param>
        /// <param name="point">location of the point</param>
        /// <param name="mode">type of point to update</param>
        public virtual void UpdateControlPointLocal(int index, float3 point, SplinePoint mode)
        {
            Assert.IsTrue(index <= ControlPointCount);

            Points[index] = point;
            RecalculateLengthBias();
        }

        /// <summary>
        /// Move all points by <paramref name="delta"/> amount
        /// </summary>
        /// <param name="delta">amount to move all point by</param>
        public void MoveControlPoints(float3 delta)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                // float2 is struct so it needs to override the stored value
                float3 local = Points[i] += delta;
                Points[i] = local;
            }
        }

        /// <summary>
        /// Relieve a point on the spline
        /// </summary>
        /// <param name="progress"></param>
        /// <returns>point on spline</returns>
        public virtual float3 Get3DPointWorld(float progress)
        {
            return ConvertToWorldSpace(Get3DPointLocal(progress));
        }

        /// <summary>
        /// Relieve a point on the spline
        /// </summary>
        /// <param name="progress"></param>
        /// <returns>point on spline</returns>
        public virtual float3 Get3DPointLocal(float progress)
        {
            if(ControlPointCount == 0)
                return float3.zero;
            if(progress <= 0f || ControlPointCount <= 1)
                return GetControlPoint3DLocal(0);
            if(progress >= 1f)
            {
                if(this is ILoopingSpline looped && looped.Looped)
                    return GetControlPoint3DLocal(0);
                return GetControlPoint3DLocal((ControlPointCount - 1));
            }

            int aIndex = FindSegmentIndex(progress);
            float pointProgress = SegmentProgress(progress, aIndex);
            return SplineInterpolation(pointProgress, aIndex);
        }

        protected override float LengthBetweenPoints(int a, int resolution = 64)
        {
            float currentLength = 0;

            float3 aPoint = SplineInterpolation(0f, a);
            for (float i = 1; i <= resolution; i++)
            {
                float3 bPoint = SplineInterpolation(i / resolution, a);
                currentLength += math.distance(aPoint, bPoint);
                aPoint = bPoint;
            }

            return currentLength;
        }

        /// <summary>
        /// Gets the given point from a point segment
        /// </summary>
        /// <param name="i">index of the segment</param>
        /// <returns>World Space position for the point</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual float3 GetControlPoint3DLocal(int i)
        {
            return Points[i];
        }

        /// <summary>
        /// Gets the given point from a point segment
        /// </summary>
        /// <param name="i">index of the segment</param>
        /// <returns>World Space position for the point</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual float3 GetControlPoint3DWorld(int i)
        {
            return ConvertToWorldSpace(GetControlPoint3DLocal(i));
        }

        /// <summary>
        /// Take the <paramref name="position"/> in local space and return the world space location
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected float3 ConvertToWorldSpace(float3 position)
        {
            return Position + (float3) (Forward * position);
        }

        /// <summary>
        /// Take the <paramref name="position"/>s in local space and convert all to world space locations
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected IEnumerable<float3> ConvertToWorldSpace(IEnumerable<float3> position)
        {
            float3 transPosCache = Position;
            foreach (float3 pos in position)
            {
                yield return transPosCache + (float3) (Forward * pos);
            }
        }

        /// <summary>
        /// Take the <paramref name="position"/> in world space and return the local space location
        /// </summary>
        protected float3 ConvertToLocalSpace(float3 position)
        {
            return Quaternion.Inverse(Forward) * (position - Position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract float3 SplineInterpolation(float t, int a);

        public override void ClearData()
        {
            base.ClearData();

            if(hasSplineEntityData) // access directly to stop possible infinite loop
            {
                SplineEntityData3D?.Dispose();
                SplineEntityData3D = null;
            }
        }

        private void Reset()
        {
            Points.Clear();

            Vector3 position = transform.position;
            AddControlPoint(new float3(position.x - 2f, position.y, 0f));
            AddControlPoint(new float3(position.x + 2f, position.y, 0f));
        }

        protected override void DrawLineGizmos()
        {
            for (int i = 0; i < SegmentPointCount - 1; i++)
            {
                float3 f = Get3DPoint(0f, i);
                Vector3 lp = new Vector3(f.x, f.y, f.z);
                int points = (int) (pointDensityF * (SegmentLength[i] * Length()));
                AddToGizmoPointCache(lp);

                for (int s = 0; s <= points; s++)
                {
                    float progress = s / (float) points;
                    float3 p = Get3DPoint(progress, i);
                    Vector3 point = new Vector3(p.x, p.y, p.z);

                    Gizmos.DrawLine(lp, point);
                    AddToGizmoPointCache(point);
                    lp = point;
                }
            }
        }

        protected virtual Spline3DData ConvertData()
        {
            ClearData();

            if(SegmentPointCount >= 2 && this is IArkableSpline arkSpline && arkSpline.ArkParameterization)
            {
                SplineEntityData3D = SplineArkConversion(arkSpline.ArkLength);
                return SplineEntityData3D.Value;
            }

            bool looped = this is ILoopingSpline loopSpline && loopSpline.Looped;
            float3[] pointData = new float3[Points.Count + (looped ? 1 : 0)];

            for (int i = 0; i < Points.Count; i++)
            {
                pointData[i] = ConvertToWorldSpace(Points[i]);
            }

            if(looped) pointData[Points.Count] = pointData[0];

            NativeArray<float3> points = new NativeArray<float3>(pointData, Allocator.Persistent);
            NativeArray<float> time = new NativeArray<float>(SegmentLength.ToArray(), Allocator.Persistent);

            Assert.IsFalse(hasSplineEntityData);
            SplineEntityData3D = new Spline3DData
            {
                Length = Length(),
                Points = points,
                Time = time
            };

            return SplineEntityData3D.Value;
        }

        /// <summary>
        /// Convert the spline into smaller linear segments with an equal distance between each point (see: <paramref name="arkLength"/>)
        /// </summary>
        /// <returns>Linear spline data</returns>
        protected virtual Spline3DData SplineArkConversion(float arkLength)
        {
            float previousTime = 0;
            float normalizedArkLength = math.max(0.001f, arkLength);
            float splineLength = Length();
            double splineCompleted = 0f;
            List<float3> points = new List<float3>((int) (splineLength / normalizedArkLength * 1.3f));
            List<float> times = new List<float>(points.Count);

            const int perPointIterationAttempts = 100; // lower values = fast, high values = high precision but longer generation times

            for (int i = 0; i < SegmentLength.Count; i++)
            {
                float currentTime = SegmentLength[i];
                float sectionLength = Length() * currentTime;
                float pointCount = ((currentTime - previousTime) * sectionLength) / normalizedArkLength;
                previousTime = currentTime;

                double previousProgress = 0f;
                float3 previous = Get3DPoint((float) previousProgress, i);
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

                    float3 point = previous;
                    int attempts = -1;
                    while (++attempts < perPointIterationAttempts)
                    {
                        point = Get3DPoint((float) currentProgress, i);

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

            return new Spline3DData
            {
                Length = Length(),
                Points = new NativeArray<float3>(points.ToArray(), Allocator.Persistent),
                Time = new NativeArray<float>(times.ToArray(), Allocator.Persistent)
            };
        }
    }
}