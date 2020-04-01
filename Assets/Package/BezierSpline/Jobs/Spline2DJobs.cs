using System;
using Code.Spline2.BezierSpline.Entity;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Code.Spline2.BezierSpline.Jobs
{
    /// <summary>
    /// Simple way of sampling a single point from a 2D spline via <see cref="Spline2DData"/>
    /// </summary>
    [BurstCompile]
    public struct Spline2DPointJob : IJob
    {
        [ReadOnly]
        public Spline2DData Spline;
        [ReadOnly]
        public SplineProgress SplineProgress;
        [WriteOnly]
        public float2 Result;

        public void Execute()
        {
            if(Spline.Points.Length == 0)
            {
                Result = new float2();
                return;
            }

            if(Spline.Points.Length == 1)
            {
                Result = Spline.Points[0];
                return;
            }
            
            int aIndex = SegmentIndex();
            Result = CubicBezierPoint(SegmentProgress(aIndex), aIndex, aIndex + 1);
        }

        private int SegmentIndex()
        {
            int seg = Spline.Time.Length;
            for (int i = 0; i < seg; i++)
            {
                float time = Spline.Time[i];
                if(time >= SplineProgress.Progress) return i;
            }

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

        private float2 CubicBezierPoint(float t, int a, int b)
        {
#if UNITY_EDITOR && ENABLE_UNITY_COLLECTIONS_CHECKS
            if(b <= 0)
                throw new ArgumentOutOfRangeException($"B is {b} which isn't within the valid point range");
#endif

            float2 p0 = Spline.Points[(a * 3)];
            float2 p1 = Spline.Points[(a * 3) + 1];
            float2 p2 = Spline.Points[(b * 3) - 1];
            float2 p3 = Spline.Points[(b * 3)];

            t = math.clamp(t, 0f, 1f);
            float oneMinusT = 1f - t;
            return
                (oneMinusT * oneMinusT * oneMinusT * p0) +
                (3f * oneMinusT * oneMinusT * t * p1) +
                (3f * oneMinusT * t * t * p2) +
                (t * t * t * p3);
        }
    }
}