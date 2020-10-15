using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Crener.Spline._3D.Jobs
{
    public struct LocalSpaceConversion3D : IJob
    {
        [ReadOnly]
        public float3 TransformPosition;
        [ReadOnly]
        public quaternion TransformRotation;
        
        public float3 SplinePosition;
        
        public void Execute()
        {
            SplinePosition = math.mul(math.inverse(TransformRotation), SplinePosition - TransformPosition);
        }
    }
}