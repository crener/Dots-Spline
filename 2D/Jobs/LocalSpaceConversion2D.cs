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

        /// <summary>
        /// Job setup when directly converting data from another job
        /// </summary>
        /// <param name="trans">position of the center of the local space transform position</param>
        /// <param name="splineWorldPosition">world space position to convert</param>
        /// <param name="allocator">allocator for the world space position reference</param>
        public LocalSpaceConversion2D(float2 trans, float2 splineWorldPosition, Allocator allocator = Allocator.None)
        {
            TransformPosition = trans;
            SplinePosition = new NativeReference<float2>(allocator);
            SplinePosition.Value = splineWorldPosition;
        }
        
        /// <summary>
        /// Job setup when directly converting data from another job
        /// </summary>
        /// <param name="trans">position of the center of the local space transform position</param>
        /// <param name="worldSpacePosition">world space position to convert</param>
        public LocalSpaceConversion2D(float2 trans, NativeReference<float2> worldSpacePosition)
        {
            TransformPosition = trans;
            SplinePosition = worldSpacePosition;
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