using Crener.Spline.Common;
using Crener.Spline.Common.DataStructs;
using Crener.Spline.Common.Interfaces;
using Crener.Spline.Common.Math;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Crener.Spline._3D.Jobs
{
    /// <summary>
    /// Simple way of sampling a single point from a 2D spline via <see cref="Spline3DData"/>
    /// </summary>
    [BurstCompile, BurstCompatible]
    public struct BezierSpline3DPointJob : IJob, ISplineJob3D
    {
        [ReadOnly]
        public Spline3DData Spline;
        [ReadOnly]
        private SplineProgress m_splineProgress;
        [WriteOnly]
        private NativeReference<float3> m_result;
        
        #region Interface properties
        public SplineProgress SplineProgress
        {
            get => m_splineProgress;
            set => m_splineProgress = value;
        }

        public float3 Result
        {
            get => m_result.Value;
            set => m_result.Value = value;
        }
        #endregion
        
        public BezierSpline3DPointJob(ISpline3D spline, float progress, Allocator allocator = Allocator.None)
            : this(spline, new SplineProgress(progress), allocator) { }

        public BezierSpline3DPointJob(ISpline3D spline, SplineProgress progress, Allocator allocator = Allocator.None)
        {
            Spline = spline.SplineEntityData3D.Value;
            m_splineProgress = progress;
            m_result = new NativeReference<float3>(allocator);
        }

        public void Execute()
        {
            m_result.Value = Run(ref Spline, ref m_splineProgress);
        }

        public static float3 Run(ref Spline3DData Spline, ref SplineProgress m_splineProgress)
        {
#if UNITY_EDITOR && NO_BURST
            if(Spline.Points.Length == 0) throw new ArgumentException($"Should be using {nameof(Empty3DPointJob)}");
            if(Spline.Points.Length == 1) throw new ArgumentException($"Should be using {nameof(SinglePoint3DPointJob)}");
#endif

            int aIndex = SplineHelperMethods.SegmentIndex3D(ref Spline, ref m_splineProgress);
            return CubicBezierPoint(ref Spline, SplineHelperMethods.SegmentProgress3D(ref Spline, ref m_splineProgress, aIndex), aIndex, aIndex + 1);
        }

        private static float3 CubicBezierPoint(ref Spline3DData Spline, float t, int a, int b)
        {
#if UNITY_EDITOR && NO_BURST
            if(b <= 0)
                throw new ArgumentOutOfRangeException($"B is {b} which isn't within the valid point range");
#endif

            float3 p0 = Spline.Points[(a * 3)];
            float3 p1 = Spline.Points[(a * 3) + 1];
            float3 p2 = Spline.Points[(b * 3) - 1];
            float3 p3 = Spline.Points[(b * 3)];

            return BezierMath.CubicBezierPoint(t, p0, p1, p2, p3);
        }

        public void Dispose()
        {
            m_result.Dispose();
        }

        public JobHandle Dispose(JobHandle inputDeps)
        {
            return m_result.Dispose(inputDeps);
        }
    }
}