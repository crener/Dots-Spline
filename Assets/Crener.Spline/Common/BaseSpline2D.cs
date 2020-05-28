using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Crener.Spline.BezierSpline.Entity;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Crener.Spline.Common
{
    /// <summary>
    /// Base implementation which contains base functionality and reusable methods
    /// </summary>
    public abstract class BaseSpline2D : MonoBehaviour, ISpline2D, IDisposable
    {
        [SerializeField]
        protected List<float2> Points = new List<float2>();
        [SerializeField]
        protected List<float> SegmentLength = new List<float>();
        // this is generated in editor and then reused in built versions.
        [SerializeField, HideInInspector]
        protected float LengthCache;
        
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
        public bool hasSplineEntityData => m_splineData.HasValue; 
        
        /// <summary>
        /// Length of the spline
        /// </summary>
        public float Length() => LengthCache;
        
        /// <summary>
        /// Amount of control points in the spline
        /// </summary>
        public virtual int ControlPointCount => Points.Count;
        
        public abstract SplineType SplineDataType { get; }
        
        /// <summary>
        /// Retrieve a point on the spline at a specific control point
        /// </summary>
        /// <returns>point on spline segment</returns>
        public float2 GetPoint(float progress, int index)
        {
            return SplineInterpolation(progress, index, (index + 1) % ControlPointCount);
        }

        public abstract void AddControlPoint(float2 point);
        public abstract void InsertControlPoint(int index, float2 point);
        public abstract void RemoveControlPoint(int index);
        public abstract SplineEditMode GetEditMode(int index);
        public abstract void ChangeEditMode(int index, SplineEditMode mode);

        /// <summary>
        /// Relieve a point on the spline
        /// </summary>
        /// <param name="progress"></param>
        /// <returns>point on spline</returns>
        public float2 GetPoint(float progress)
        {
            if(progress == 0f || ControlPointCount <= 1)
                return GetControlPoint(0);
            else if(progress >= 1f)
                return GetControlPoint(ControlPointCount - 1);

            int aIndex = FindSegmentIndex(progress);
            float pointProgress = SegmentProgress(progress, aIndex);

            int bIndex = (aIndex + 1) % ControlPointCount;
            return SplineInterpolation(pointProgress, aIndex, bIndex);
        }

        protected float LengthBetweenPoints(int a, int b, int resolution = 64)
        {
            float currentLength = 0;

            float2 aPoint =  SplineInterpolation(0f, a, b);
            for (float i = 1; i <= resolution; i++)
            {
                float2 bPoint = SplineInterpolation(i / resolution, a, b);
                currentLength += math.distance(aPoint, bPoint);
                aPoint = bPoint;
            }

            return currentLength;
        }

        protected int FindSegmentIndex(float progress)
        {
            int seg = SegmentLength.Count;
            for (int i = 0; i < seg; i++)
            {
                float time = SegmentLength[i];
                if(time >= progress) return i;
            }

            return 0;
        }

        /// <summary>
        /// Calculates the segment progress given the spline progress
        /// </summary>
        /// <param name="progress">spline progress</param>
        /// <param name="index">segment index</param>
        /// <returns>segment progress</returns>
        protected float SegmentProgress(float progress, int index)
        {
            if(ControlPointCount <= 2) return progress;

            if(index == 0)
            {
                float segmentProgress = SegmentLength[0];

                return progress / segmentProgress;
            }

            float aLn = SegmentLength[index - 1];
            float bLn = SegmentLength[index];

            return (progress - aLn) / (bLn - aLn);
        }

        protected void RecalculateLengthBias()
        {
            ClearData();

            if(ControlPointCount <= 1)
            {
                LengthCache = 0f;
                return;
            }

            // calculate the distance that the entire spline covers
            float currentLength = 0f;
            for (int a = 0; a < ControlPointCount - 1; a++)
            {
                int b = (a + 1) % ControlPointCount;
                float length = LengthBetweenPoints(a, b);

                currentLength += length;
            }

            LengthCache = currentLength;

            SegmentLength.Clear();
            if(ControlPointCount == 2)
            {
                SegmentLength.Add(1f);
                return;
            }

            // calculate the distance that a single segment covers
            float segmentCount = 0f;
            for (int a = 0; a < ControlPointCount - 1; a++)
            {
                int b = (a + 1) % ControlPointCount;
                float length = LengthBetweenPoints(a, b);

                segmentCount = (length / LengthCache) + segmentCount;
                SegmentLength.Add(segmentCount);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract float2 GetControlPoint(int i);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract float2 SplineInterpolation(float t, int a, int b);
        
        public void Dispose()
        {
            ClearData();
        }
        
        private void OnDestroy()
        {
            ClearData();
        }

        public void ClearData()
        {
            if(m_splineData.HasValue) // access directly to stop possible infinite loop
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

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.gray;
            const float pointDensity = 13;

            for (int i = 0; i < ControlPointCount - 1; i++)
            {
                float2 f = GetPoint(0f, i);
                Vector3 lp = new Vector3(f.x, f.y, 0f);
                int points = (int) (pointDensity * (SegmentLength[i] * Length()));

                for (int s = 0; s <= points; s++)
                {
                    float progress = s / (float) points;
                    float2 p = GetPoint(progress, i);
                    Vector3 point = new Vector3(p.x, p.y, 0f);

                    Gizmos.DrawLine(lp, point);
                    lp = point;
                }
            }
        }

        public abstract void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem);
        protected abstract Spline2DData ConvertData();
    }
}