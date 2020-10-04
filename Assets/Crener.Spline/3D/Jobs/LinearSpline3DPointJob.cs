using System;
using Crener.Spline.Common;
using Crener.Spline.Common.DataStructs;
using Crener.Spline.Common.Interfaces;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Crener.Spline._3D.Jobs
{
    /// <summary>
    /// Simple way of sampling a single point from a 2D spline via <see cref="Spline2DData"/>
    /// </summary>
    [BurstCompile]
    public struct LinearSpline3DPointJob : IJob, ISplineJob3D
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
            #endif

            int aIndex = SegmentIndex();
            m_result = LinearLerp(SegmentProgress(aIndex), aIndex, aIndex + 1);
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

        private float3 LinearLerp(float t, int a, int b)
        {
#if UNITY_EDITOR
            if(b <= 0)
                throw new ArgumentOutOfRangeException($"B is {b} which isn't within the valid point range! " +
                                                      $"Actual Range '0 - {Spline.Points.Length}', requested range '{a} - {b}'");
#endif

            float3 p0 = Spline.Points[a];
            float3 p1 = Spline.Points[b];

            return math.lerp(p0, p1, math.clamp(t, 0f, 1f));
        }
    }
}