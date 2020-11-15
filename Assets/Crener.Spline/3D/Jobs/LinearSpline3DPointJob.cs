﻿using System;
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
    [BurstCompile, BurstCompatible]
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
            m_result = Run(ref Spline, ref m_splineProgress);
        }

        public static float3 Run(ref Spline3DData spline, ref SplineProgress progress)
        {
#if UNITY_EDITOR && NO_BURST
            if(spline.Points.Length == 0) throw new ArgumentException($"Should be using {nameof(Empty3DPointJob)}");
            if(spline.Points.Length == 1) throw new ArgumentException($"Should be using {nameof(SinglePoint3DPointJob)}");
            if(spline.Points.Length == 2) throw new ArgumentException($"Should be using {nameof(LinearSpline3DPointJob)}");
#endif

            int aIndex = SplineHelperMethods.SegmentIndex3D(ref spline, ref progress);
            return LinearLerp(ref spline, SplineHelperMethods.SegmentProgress3D(ref spline, ref progress, aIndex), aIndex, aIndex + 1);
        }

        private static float3 LinearLerp(ref Spline3DData spline, float t, int a, int b)
        {
#if UNITY_EDITOR && NO_BURST
            if(b <= 0)
                throw new ArgumentOutOfRangeException($"B is {b} which isn't within the valid point range! " +
                                                      $"Actual Range '0 - {spline.Points.Length}', requested range '{a} - {b}'");
#endif

            float3 p0 = spline.Points[a];
            float3 p1 = spline.Points[b];

            return math.lerp(p0, p1, math.clamp(t, 0f, 1f));
        }
    }
}