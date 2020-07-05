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

        private Spline2DData? m_splineData = null;

        public Spline2DData? SplineEntityData
        {
            get
            {
                if(!hasSplineEntityData) ConvertData();
                return m_splineData;
            }
            protected set => m_splineData = value;
        }

        /// <summary>
        /// true if Spline Entity Data has been initialized, calling <see cref="SplineEntityData"/> directly will automatically generate data
        /// </summary>
        public override bool hasSplineEntityData => m_splineData.HasValue;

        /// <summary>
        /// Amount of control points in the spline
        /// </summary>
        public override int ControlPointCount => Points.Count;

        /// <summary>
        /// Is the data in the spline initialized
        /// </summary>
        protected virtual bool DataInitialized => SegmentPointCount > 0 && SegmentLength.Count >= 0;

        /// <summary>
        /// Retrieve a point on the spline at a specific control point
        /// </summary>
        /// <returns>point on spline segment</returns>
        public virtual float2 GetPoint(float progress, int index)
        {
            return SplineInterpolation(progress, index);
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
            if(Points.Count <= 1 || index >= ControlPointCount)
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
        public virtual float2 GetPoint(float progress)
        {
            if(ControlPointCount == 0)
                return float2.zero;
            else if(progress <= 0f || ControlPointCount == 1)
                return GetControlPoint(0);
            else if(progress >= 1f)
                return GetControlPoint(ControlPointCount - 1);

            int aIndex = FindSegmentIndex(progress);
            float pointProgress = SegmentProgress(progress, aIndex);
            return SplineInterpolation(pointProgress, aIndex);
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
        /// Gets the given point from a point segment
        /// </summary>
        /// <param name="i">index of the segment</param>
        /// <returns>World Space position for the point</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual float2 GetControlPoint(int i)
        {
            return Points[i];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract float2 SplineInterpolation(float t, int a);

        public override void ClearData()
        {
            if(hasSplineEntityData) // access directly to stop possible infinite loop
            {
                SplineEntityData.Value.Dispose();
                SplineEntityData = null;
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
                float2 f = GetPoint(0f, i);
                Vector3 lp = new Vector3(f.x, f.y, 0f);
                int points = math.min((int) (pointDensity * (SegmentLength[i] * Length())), maxPointAmount);

                for (int s = 1; s <= points; s++)
                {
                    float progress = s / (float) points;
                    float2 p = GetPoint(progress, i);
                    Vector3 point = new Vector3(p.x, p.y, 0f);

                    Gizmos.DrawLine(lp, point);
                    lp = point;
                }
            }
        }

        protected virtual Spline2DData ConvertData()
        {
            ClearData();
            NativeArray<float2> points = new NativeArray<float2>(Points.ToArray(), Allocator.Persistent);
            NativeArray<float> time = new NativeArray<float>(SegmentLength.ToArray(), Allocator.Persistent);

            Assert.IsFalse(hasSplineEntityData);
            SplineEntityData = new Spline2DData
            {
                Length = Length(),
                Points = points,
                Time = time
            };

            return SplineEntityData.Value;
        }
    }
}