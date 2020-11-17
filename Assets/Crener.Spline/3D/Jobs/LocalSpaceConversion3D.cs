using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Crener.Spline._3D.Jobs
{
    [BurstCompile, BurstCompatible]
    public struct LocalSpaceConversion3D : IJob, INativeDisposable
    {
        [ReadOnly]
        public float3 TransformPosition;
        [ReadOnly]
        public quaternion TransformRotation;
        
        public NativeReference<float3> SplinePosition;

        public LocalSpaceConversion3D(float3 position, Quaternion forward, float3 worldSpacePosition)
        {
            SplinePosition = new NativeReference<float3>(Allocator.TempJob);
            SplinePosition.Value = worldSpacePosition;
            TransformPosition = position;
            TransformRotation = forward;
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