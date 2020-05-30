using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Crener.Spline.Common.DataStructs;
using Crener.Spline.Common.Interfaces;
using Unity.Mathematics;
using UnityEngine;

namespace Crener.Spline.BaseSpline
{
    /// <summary>
    /// Base implementation which contains base functionality and reusable methods
    /// </summary>
    public abstract class BaseSpline3D : BaseSpline, ISpline3D
    {
        [SerializeField]
        protected List<float2> Points = new List<float2>();
        
        private Spline3DData? m_splineData = null;

        public Spline3DData? SplineEntityData
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
        /// Retrieve a point on the spline at a specific control point
        /// </summary>
        /// <returns>point on spline segment</returns>
        public float3 GetPoint(float progress, int index)
        {
            return SplineInterpolation(progress, index, (index + 1) % ControlPointCount);
        }

        public abstract void AddControlPoint(float3 point);
        public abstract void InsertControlPoint(int index, float3 point);

        /// <summary>
        /// Relieve a point on the spline
        /// </summary>
        /// <param name="progress"></param>
        /// <returns>point on spline</returns>
        public float3 GetPoint(float progress)
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

        protected override float LengthBetweenPoints(int a, int b, int resolution = 64)
        {
            float currentLength = 0;

            float3 aPoint =  SplineInterpolation(0f, a, b);
            for (float i = 1; i <= resolution; i++)
            {
                float3 bPoint = SplineInterpolation(i / resolution, a, b);
                currentLength += math.distance(aPoint, bPoint);
                aPoint = bPoint;
            }

            return currentLength;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract float3 GetControlPoint(int i);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract float3 SplineInterpolation(float t, int a, int b);

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
            AddControlPoint(new float3(position.x - 2f, position.y, 0f));
            AddControlPoint(new float3(position.x + 2f, position.y, 0f));
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.gray;
            const float pointDensity = 13;

            for (int i = 0; i < ControlPointCount - 1; i++)
            {
                float3 f = GetPoint(0f, i);
                Vector3 lp = new Vector3(f.x, f.y, 0f);
                int points = (int) (pointDensity * (SegmentLength[i] * Length()));

                for (int s = 0; s <= points; s++)
                {
                    float progress = s / (float) points;
                    float3 p = GetPoint(progress, i);
                    Vector3 point = new Vector3(p.x, p.y, 0f);

                    Gizmos.DrawLine(lp, point);
                    lp = point;
                }
            }
        }

        protected abstract Spline3DData ConvertData();
    }
}