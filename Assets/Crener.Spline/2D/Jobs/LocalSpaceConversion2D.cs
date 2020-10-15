using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Crener.Spline._2D.Jobs
{
    public struct LocalSpaceConversion2D : IJob
    {
        [ReadOnly]
        public float2 TransformPosition;
        
        public float2 SplinePosition;
        
        public void Execute()
        {
            SplinePosition = SplinePosition - TransformPosition;
        }
    }
}