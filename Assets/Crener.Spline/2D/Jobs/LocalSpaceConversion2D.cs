using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Crener.Spline._2D.Jobs
{
    [BurstCompile, BurstCompatible]
    public struct LocalSpaceConversion2D : IJob, INativeDisposable
    {
        [ReadOnly]
        public float2 TransformPosition;
        
        public NativeReference<float2> SplinePosition;

        public LocalSpaceConversion2D(float2 trans, float2 splineWorldPosition, Allocator allocator = Allocator.None) : this()
        {
            TransformPosition = trans;
            SplinePosition = new NativeReference<float2>(allocator);
            SplinePosition.Value = splineWorldPosition;
        }

        public void Execute()
        {
            SplinePosition.Value = SplinePosition.Value - TransformPosition;
        }

        public void Dispose()
        {
            SplinePosition.Dispose();
        }

        public JobHandle Dispose(JobHandle inputDeps)
        {
            return SplinePosition.Dispose(inputDeps);
        }
    }
}