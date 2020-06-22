using System;
using Crener.Spline.Common;
using Crener.Spline.Common.DataStructs;
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
    public struct LinearSpline2DPointJob : IJob
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
            Result = LinearLerp(SegmentProgress(aIndex), aIndex, aIndex + 1);
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

        private float2 LinearLerp(float t, int a, int b)
        {
#if UNITY_EDITOR
            if(b <= 0)
                throw new ArgumentOutOfRangeException($"B is {b} which isn't within the valid point range! " +
                                                      $"Actual Range '0 - {Spline.Points.Length}', requested range '{a} - {b}'");
#endif

            float2 p0 = Spline.Points[a];
            float2 p1 = Spline.Points[b];

            return math.lerp(p0, p1, math.clamp(t, 0f, 1f));
        }
    }
}