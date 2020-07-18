using System;
using Crener.Spline.Common;
using Crener.Spline.Common.DataStructs;
using Crener.Spline.Common.Interfaces;
using Crener.Spline.Linear.Jobs._2D;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Crener.Spline.Linear.Jobs._3D
{
    /// <summary>
    /// Cubic linear with no looping support
    /// </summary>
    [BurstCompile]
    public struct LinearCubicSpline3DPointJob : IJob, ISplineJob3D
    {
        [ReadOnly]
        public Spline3DData Spline;
        [ReadOnly]
        private SplineProgress m_splineProgress;
        [WriteOnly]
        private float3 m_result;
        
        #region Interface properties
        public SplineProgress SplineProgress
        {
            get => m_splineProgress;
            set => m_splineProgress = value;
        }

        public float3 Result
        {
            get => m_result;
            set => m_result = value;
        }
        #endregion

        public void Execute()
        {
#if UNITY_EDITOR
            if(Spline.Points.Length == 0) throw new ArgumentException($"Should be using {nameof(Empty2DPointJob)}");
            if(Spline.Points.Length == 1) throw new ArgumentException($"Should be using {nameof(SinglePoint2DPointJob)}");
            if(Spline.Points.Length == 2) throw new ArgumentException($"Should be using {nameof(LinearSpline2DPointJob)}");
#endif

            int aIndex = SegmentIndex();
            m_result = LinearLerp(SegmentProgress(aIndex), aIndex);
        }

        private int SegmentIndex()
        {
            int seg = Spline.Time.Length;
            float tempProgress = math.clamp(SplineProgress.Progress, 0f, 1f);
            for (int i = 0; i < seg; i++)
            {
                float time = Spline.Time[i];
                if(time >= tempProgress) return i;
            }

#if UNITY_EDITOR
            if(seg - 1 != Spline.Points.Length - 2)
            {
                // if the progress is greater than the spline time it should result in the last point being returned
                throw new IndexOutOfRangeException("Spline time has less data than expected for the requested point range!");
            }
#endif

            return seg - 1;
        }

        private float SegmentProgress(int index)
        {
            float tempProgress = math.clamp(SplineProgress.Progress, 0f, 1f);
            
            if(index == 0) return tempProgress / Spline.Time[0];
            if(Spline.Time.Length <= 1) return tempProgress;

            float aLn = Spline.Time[index - 1];
            float bLn = Spline.Time[index];

            return (tempProgress - aLn) / (bLn - aLn);
        }

        private float3 LinearLerp(float t, int a)
        {
            float3 p0 = Spline.Points[a];
            float3 p1 = Spline.Points[(a + 1) % Spline.Points.Length];
            
            if(Spline.Points.Length == 2) return math.lerp(p0, p1, t);
            
            float3 p2 = Spline.Points[(a + 2) % Spline.Points.Length];

            float3 i0, i1;
            const float splineMidPoint = 0.5f;

            if(Spline.Points.Length > 3)
            {
                // todo add an extra segment for the first and last segment of the spline so it doesn't get stretched
                if(a == 0)
                {
                    i0 = math.lerp(p0, p1, t);
                    i1 = math.lerp(p1, p2, splineMidPoint);
                }
                else if(a == Spline.Points.Length - 3)
                {
                    i0 = math.lerp(p0, p1, splineMidPoint);
                    i1 = math.lerp(p1, p2, t);
                }
                else
                {
                    i0 = math.lerp(p0, p1, splineMidPoint);
                    i1 = math.lerp(p1, p2, splineMidPoint);
                }
            }
            else
            {
                i0 = math.lerp(p0, p1, t);
                i1 = math.lerp(p1, p2, t);
            }

            float3 pp0 = math.lerp(i0, p1, t);
            float3 pp1 = math.lerp(p1, i1, t);

            return math.lerp(pp0, pp1, t);
        }
    }
}