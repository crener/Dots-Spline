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
        private NativeReference<float2> m_result;
        
        #region Interface properties
        public SplineProgress SplineProgress
        {
            get => m_splineProgress;
            set => m_splineProgress = value;
        }

        public float2 Result
        {
            get => m_result.Value;
            set => m_result.Value = value;
        }
        #endregion

        public BezierSpline2DPointJob(ISpline2D spline, float progress, Allocator allocator = Allocator.None)
            : this(spline, new SplineProgress(progress), allocator) { }
        
        public BezierSpline2DPointJob(ISpline2D spline, SplineProgress splineProgress, Allocator allocator = Allocator.None)
        {
            Spline = spline.SplineEntityData2D.Value;
            m_splineProgress = splineProgress;
            m_result = new NativeReference<float2>(allocator);
        }

        public void Execute()
        {
            m_result.Value = Run(ref Spline, ref m_splineProgress);
        }

        public static float2 Run(ref Spline2DData spline, ref SplineProgress progress)
        {
#if UNITY_EDITOR && NO_BURST
            if(Spline.Points.Length == 0) throw new ArgumentException($"Should be using {nameof(Empty2DPointJob)}");
            if(Spline.Points.Length == 1) throw new ArgumentException($"Should be using {nameof(SinglePoint2DPointJob)}");
#endif

            int aIndex = SplineHelperMethods.SegmentIndex(ref spline, ref progress);
            return CubicBezierPoint(ref spline,SplineHelperMethods.SegmentProgress(ref spline, ref progress, aIndex), aIndex, aIndex + 1);
        }

        private static float2 CubicBezierPoint(ref Spline2DData spline, float t, int a, int b)
        {
#if UNITY_EDITOR && NO_BURST
            if(b <= 0)
                throw new ArgumentOutOfRangeException($"B is {b} which isn't within the valid point range");
#endif

            float2 p0 = spline.Points[(a * 3)];
            float2 p1 = spline.Points[(a * 3) + 1];
            float2 p2 = spline.Points[(b * 3) - 1];
            float2 p3 = spline.Points[(b * 3)];

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