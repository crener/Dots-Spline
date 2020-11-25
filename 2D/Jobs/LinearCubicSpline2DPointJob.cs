using Crener.Spline.Common;
using Crener.Spline.Common.DataStructs;
using Crener.Spline.Common.Interfaces;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Crener.Spline._2D.Jobs
{
    /// <summary>
    /// Cubic linear with no looping support
    /// </summary>
    [BurstCompile, BurstCompatible]
    public struct LinearCubicSpline2DPointJob : IJob, ISplineJob2D
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

        public LinearCubicSpline2DPointJob(ISpline2D spline, float progress, Allocator allocator = Allocator.None)
            : this(spline, new SplineProgress(progress), allocator) { }
        
        public LinearCubicSpline2DPointJob(ISpline2D spline, SplineProgress splineProgress, Allocator allocator = Allocator.None)
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
            if(Spline.Points.Length == 2) throw new ArgumentException($"Should be using {nameof(LinearSpline2DPointJob)}");
#endif

            int aIndex = SplineHelperMethods.SegmentIndexClamp(ref spline, ref progress);
            return LinearLerp(ref spline, SplineHelperMethods.SegmentProgressClamp(ref spline, ref progress, aIndex), aIndex);
        }

        private static float2 LinearLerp(ref Spline2DData spline, float t, int a)
        {
            float2 p0 = spline.Points[a];
            float2 p1 = spline.Points[(a + 1) % spline.Points.Length];
            float2 p2 = spline.Points[(a + 2) % spline.Points.Length];

            float2 i0, i1;
            const float splineMidPoint = 0.5f;

            if(spline.Points.Length > 3)
            {
                // todo add an extra segment for the first and last segment of the spline so it doesn't get stretched
                if(a == 0)
                {
                    i0 = math.lerp(p0, p1, t);
                    i1 = math.lerp(p1, p2, splineMidPoint);
                }
                else if(a == spline.Points.Length - 3)
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

            float2 pp0 = math.lerp(i0, p1, t);
            float2 pp1 = math.lerp(p1, i1, t);

            return math.lerp(pp0, pp1, t);
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