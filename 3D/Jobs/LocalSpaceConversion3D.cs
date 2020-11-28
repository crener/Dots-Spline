using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Crener.Spline._3D.Jobs
{
    /// <summary>
    /// Converts a world space position to a local position
    /// </summary>
    [BurstCompile, BurstCompatible]
    public struct LocalSpaceConversion3D : IJob, INativeDisposable
    {
        [ReadOnly]
        public float3 TransformPosition;
        [ReadOnly]
        public quaternion TransformRotation;
        
        public NativeReference<float3> SplinePosition;

        /// <summary>
        /// Job setup from a non-job source 
        /// </summary>
        /// <param name="position">position of the center of the local space transform position</param>
        /// <param name="forward">forward direction of the local space transform</param>
        /// <param name="worldSpacePosition">world space position to convert</param>
        /// <param name="allocator">allocator for the world space position reference</param>
        public LocalSpaceConversion3D(float3 position, Quaternion forward, float3 worldSpacePosition, Allocator allocator = Allocator.None)
        : this(position, forward)
        {
            SplinePosition = new NativeReference<float3>(allocator);
            SplinePosition.Value = worldSpacePosition;
        }
        
        /// <summary>
        /// Job setup when directly converting data from another job
        /// </summary>
        /// <param name="position">position of the center of the local space transform position</param>
        /// <param name="forward">forward direction of the local space transform</param>
        /// <param name="worldSpacePosition">world space position to convert</param>
        public LocalSpaceConversion3D(float3 position, Quaternion forward, NativeReference<float3> worldSpacePosition)
        : this(position, forward)
        {
            SplinePosition = worldSpacePosition;
        }

        /// <summary>
        /// default 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="forward"></param>
        private LocalSpaceConversion3D(float3 position, Quaternion forward)
        {
            TransformPosition = position;
            TransformRotation = forward;
            SplinePosition = default;
        }

        public void Execute()
        {
            SplinePosition.Value = math.mul(math.inverse(TransformRotation), SplinePosition.Value - TransformPosition);
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