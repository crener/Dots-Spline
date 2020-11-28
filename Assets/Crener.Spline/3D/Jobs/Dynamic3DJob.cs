using Crener.Spline.Common;
using Crener.Spline.Common.DataStructs;
using Crener.Spline.Common.Interfaces;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Crener.Spline._3D.Jobs
{
    [BurstCompile, BurstCompatible]
    public struct Dynamic3DJob : ISplineJob3D
    {
        [ReadOnly]
        public Spline3DData Spline;
        [ReadOnly]
        private SplineProgress m_splineProgress;
        [ReadOnly]
        private SplineType m_type;
        [WriteOnly]
        public NativeReference<float3> NativeResult;

        #region Interface properties
        public SplineProgress SplineProgress
        {
            get => m_splineProgress;
            set => m_splineProgress = value;
        }

        public float3 Result
        {
            get => NativeResult.Value;
            set => NativeResult.Value = value;
        }
        #endregion

        public Dynamic3DJob(ISpline3D spline, float progress, Allocator allocator = Allocator.None)
            : this(spline, new SplineProgress(progress), allocator) { }

        public Dynamic3DJob(ISpline3D spline, SplineProgress progress, Allocator allocator = Allocator.None)
        {
            m_type = spline.SplineDataType;
            Spline = spline.SplineEntityData3D.Value;
            m_splineProgress = progress;
            NativeResult = new NativeReference<float3>(allocator);
        }

        public void Execute()
        {
            switch (m_type)
            {
                case SplineType.Empty:
                    NativeResult.Value = Empty3DPointJob.Run(ref Spline);
                    return;
                case SplineType.Single:
                    NativeResult.Value = SinglePoint3DPointJob.Run(ref Spline);
                    return;
                case SplineType.Bezier:
                    NativeResult.Value = BezierSpline3DPointJob.Run(ref Spline, ref m_splineProgress);
                    return;
                case SplineType.CubicLinear:
                    NativeResult.Value = LinearCubicSpline3DPointJob.Run(ref Spline, ref m_splineProgress);
                    return;
                case SplineType.Cubic:
                //todo
                case SplineType.BSpline:
                //todo
                case SplineType.CatmullRom:
                //todo
                case SplineType.Linear: // falls over to the default by design
                default:
                    NativeResult.Value = LinearSpline3DPointJob.Run(ref Spline, ref m_splineProgress);
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