using System;
using Crener.Spline.Common;
using Crener.Spline.Common.DataStructs;
using Crener.Spline.Common.Interfaces;
using Crener.Spline.Common.Math;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Crener.Spline._2D.Jobs
{
    /// <summary>
    /// Simple way of sampling a single point from a 2D spline via <see cref="Spline2DData"/>
    /// </summary>
    [BurstCompile, BurstCompatible]
    public struct BezierSpline2DPointJob : IJob, ISplineJob2D
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
#if UNITY_EDITOR && NO_BURST
            if(Spline.Points.Length == 0) throw new ArgumentException($"Should be using {nameof(Empty2DPointJob)}");
            if(Spline.Points.Length == 1) throw new ArgumentException($"Should be using {nameof(SinglePoint2DPointJob)}");
#endif

            int aIndex = SegmentIndex();
            m_result = CubicBezierPoint(SegmentProgress(aIndex), aIndex, aIndex + 1);
        }

        private int SegmentIndex()
        {
            int seg = Spline.Time.Length;
            for (int i = 0; i < seg; i++)
            {
                float time = Spline.Time[i];
                if(time >= m_splineProgress.Progress) return i;
            }

            return seg - 1;
        }

        private float SegmentProgress(int index)
        {
            if(index == 0) return m_splineProgress.Progress / Spline.Time[0];
            if(Spline.Time.Length <= 1) return m_splineProgress.Progress;

            float aLn = Spline.Time[index - 1];
            float bLn = Spline.Time[index];

            return (m_splineProgress.Progress - aLn) / (bLn - aLn);
        }

        private float2 CubicBezierPoint(float t, int a, int b)
        {
#if UNITY_EDITOR && NO_BURST
            if(b <= 0)
                throw new ArgumentOutOfRangeException($"B is {b} which isn't within the valid point range");
#endif

            float2 p0 = Spline.Points[(a * 3)];
            float2 p1 = Spline.Points[(a * 3) + 1];
            float2 p2 = Spline.Points[(b * 3) - 1];
            float2 p3 = Spline.Points[(b * 3)];

            return BezierMath.CubicBezierPoint(t, p0, p1, p2, p3);
        }
    }
}