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
    /// Cubic linear with no looping support
    /// </summary>
    [BurstCompile, BurstCompatible]
    public struct LinearCubicSpline3DPointJob : IJob, ISplineJob3D
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

        public LinearCubicSpline3DPointJob(ISpline3D spline, float progress, Allocator allocator = Allocator.None)
            : this(spline, new SplineProgress(progress), allocator) { }

        public LinearCubicSpline3DPointJob(ISpline3D spline, SplineProgress progress, Allocator allocator = Allocator.None)
        {
            Spline = spline.SplineEntityData3D.Value;
            m_splineProgress = progress;
            m_result = new NativeReference<float3>(allocator);
        }
        
        public void Execute()
        {
            m_result.Value = Run(ref Spline, ref m_splineProgress);
        }

        public static float3 Run(ref Spline3DData spline, ref SplineProgress progress)
        {
#if UNITY_EDITOR && NO_BURST
            if(spline.Points.Length == 0) throw new ArgumentException($"Should be using {nameof(Empty3DPointJob)}");
            if(spline.Points.Length == 1) throw new ArgumentException($"Should be using {nameof(SinglePoint3DPointJob)}");
            if(spline.Points.Length == 2) throw new ArgumentException($"Should be using {nameof(LinearSpline3DPointJob)}");
#endif

            int aIndex = SplineHelperMethods.SegmentIndex3D(ref spline, ref progress);
            return LinearLerp(ref spline, SplineHelperMethods.SegmentProgress3DClamp(ref spline, ref progress, aIndex), aIndex);
        }
        
        private static float3 LinearLerp(ref Spline3DData spline, float t, int a)
        {
            float3 p0 = spline.Points[a];
            float3 p1 = spline.Points[(a + 1) % spline.Points.Length];
            
            if(spline.Points.Length == 2) return math.lerp(p0, p1, t);
            
            float3 p2 = spline.Points[(a + 2) % spline.Points.Length];

            float3 i0, i1;
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

            float3 pp0 = math.lerp(i0, p1, t);
            float3 pp1 = math.lerp(p1, i1, t);

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