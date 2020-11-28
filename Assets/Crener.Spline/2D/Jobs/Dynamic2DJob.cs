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
        public NativeReference<float2> NativeResult;

        #region Interface properties
        public SplineProgress SplineProgress
        {
            get => m_splineProgress;
            set => m_splineProgress = value;
        }

        public float2 Result
        {
            get => NativeResult.Value;
            set => NativeResult.Value = value;
        }
        #endregion

        public Dynamic2DJob(ISpline2D spline, float progress, Allocator allocator = Allocator.None)
            : this(spline, new SplineProgress(progress), allocator) { }

        public Dynamic2DJob(ISpline2D spline, SplineProgress progress, Allocator allocator = Allocator.None)
        {
            m_type = spline.SplineDataType;
            Spline = spline.SplineEntityData2D.Value;
            m_splineProgress = progress;
            NativeResult = new NativeReference<float2>(allocator);
        }

        public void Execute()
        {
            switch (m_type)
            {
                case SplineType.Empty:
                    NativeResult.Value =  Empty2DPointJob.Run();
                    return;
                case SplineType.Single:
                    NativeResult.Value = SinglePoint2DPointJob.Run(ref Spline);
                    return;
                case SplineType.Bezier:
                    NativeResult.Value = BezierSpline2DPointJob.Run(ref Spline, ref m_splineProgress);
                    return;
                case SplineType.CubicLinear:
                    NativeResult.Value =  LinearCubicSpline2DPointJob.Run(ref Spline, ref m_splineProgress);
                    return;
                case SplineType.CatmullRom:
                    NativeResult.Value = CatmullRomSpline2DPointJob.Run(ref Spline, ref m_splineProgress);
                    return;
                case SplineType.Cubic:
                //todo
                case SplineType.BSpline:
                //todo
                case SplineType.Linear: // falls over to the default by design
                default:
                    NativeResult.Value = LinearSpline2DPointJob.Run(ref Spline, ref m_splineProgress);
                    return;
            }
        }

        public void Dispose()
        {
            NativeResult.Dispose();
        }

        public JobHandle Dispose(JobHandle inputDeps)
        {
            return NativeResult.Dispose(inputDeps);
        }
    }
}