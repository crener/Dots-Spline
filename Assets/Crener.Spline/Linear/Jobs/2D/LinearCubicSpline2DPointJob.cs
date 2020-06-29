using System;
using Crener.Spline.Common;
using Crener.Spline.Common.DataStructs;
using Crener.Spline.Common.Interfaces;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Crener.Spline.Linear.Jobs._2D
{
    /// <summary>
    /// Simple way of sampling a single point from a 2D spline via <see cref="Spline2DData"/>
    /// </summary>
    [BurstCompile]
    public struct LinearCubicSpline2DPointJob : IJob, ISplineJob2D
    {
        [ReadOnly]
        public Spline2DData Spline;
        [ReadOnly]
        private SplineProgress m_splineProgress;
        [WriteOnly]
        private float2 m_result;
        
        #region Interface properties
        public SplineProgress SplineProgress
        {
            get => m_splineProgress;
            set => m_splineProgress = value;
        }

        public float2 Result
        {
            get => m_result;
            set => m_result = value;
        }
        #endregion

        public void Execute()
        {
            if(Spline.Points.Length == 0)
            {
                m_result = new float2();
                return;
            }

            if(Spline.Points.Length == 1)
            {
                m_result = Spline.Points[0];
                return;
            }

            if(Spline.Points.Length == 2)
            {
                m_result = math.lerp(Spline.Points[0], Spline.Points[1], m_splineProgress.Progress);
                return;
            }

            int aIndex = SegmentIndex();
            m_result = LinearLerp(SegmentProgress(aIndex), aIndex);
        }

        private int SegmentIndex()
        {
            int seg = Spline.Time.Length;
            for (int i = 0; i < seg; i++)
            {
                float time = Spline.Time[i];
                if(time >= SplineProgress.Progress) return i;
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
            if(index == 0) return SplineProgress.Progress / Spline.Time[0];
            if(Spline.Time.Length <= 1) return SplineProgress.Progress;

            float aLn = Spline.Time[index - 1];
            float bLn = Spline.Time[index];

            return (SplineProgress.Progress - aLn) / (bLn - aLn);
        }

        private float2 LinearLerp(float t, int a)
        {
            float2 p0 = Spline.Points[a];
            float2 p1 = Spline.Points[a+1];
            float2 p2 = Spline.Points[a+2];
            
            float2 i0 = math.lerp(p0, p1, t);
            float2 i1 = math.lerp(p1, p2, t);

            float2 pp0 = math.lerp(i0, p1, t);
            float2 pp1 = math.lerp(p1, i1, t);

            return math.lerp(pp0, pp1, t);
        }
    }
}