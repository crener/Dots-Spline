using Crener.Spline.Common;
using Crener.Spline.Common.DataStructs;
using Crener.Spline.Common.Interfaces;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Crener.Spline._2D.Jobs
{
    public class Dynamic2DJob : ISplineJob2D
    {
        [ReadOnly]
        public Spline2DData Spline;
        [ReadOnly]
        private SplineProgress m_splineProgress;
        [ReadOnly]
        private SplineType m_type;
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

        public Dynamic2DJob(ISpline2D spline, float progress, Allocator allocator = Allocator.None)
            : this(spline, new SplineProgress(progress), allocator) { }

        public Dynamic2DJob(ISpline2D spline, SplineProgress progress, Allocator allocator = Allocator.None)
        {
            m_type = spline.SplineDataType;
            Spline = spline.SplineEntityData2D.Value;
            m_splineProgress = progress;
            m_result = new NativeReference<float2>(allocator);
        }

        public void Execute()
        {
            switch (m_type)
            {
                case SplineType.Empty:
                    m_result.Value =  Empty2DPointJob.Run();
                    return;
                case SplineType.Single:
                    m_result.Value = SinglePoint2DPointJob.Run(ref Spline);
                    return;
                case SplineType.Bezier:
                    m_result.Value = BezierSpline2DPointJob.Run(ref Spline, ref m_splineProgress);
                    return;
                case SplineType.CubicLinear:
                    m_result.Value =  LinearCubicSpline2DPointJob.Run(ref Spline, ref m_splineProgress);
                    return;
                case SplineType.CatmullRom:
                    m_result.Value = CatmullRomSpline2DPointJob.Run(ref Spline, ref m_splineProgress);
                    return;
                case SplineType.Cubic:
                //todo
                case SplineType.BSpline:
                //todo
                case SplineType.Linear: // falls over to the default by design
                default:
                    m_result.Value = LinearSpline2DPointJob.Run(ref Spline, ref m_splineProgress);
                    return;
            }
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