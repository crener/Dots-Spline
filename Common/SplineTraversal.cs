using Crener.Spline.BezierSpline.Entity;
using Unity.Entities;
using Unity.Mathematics;

namespace Crener.Spline.Common
{
    /// <summary>
    /// Data about the progress of an object along a spline
    /// </summary>
    public struct SplineProgress : IComponentData
    {
        public float Progress;
        
        public SplineProgress(float progress)
        {
            Progress = progress;
        }
    }

    /// <summary>
    /// Speed of an entity moving across the spline
    /// </summary>
    public struct TraversalSpeed : IComponentData
    {
        public float Speed;
    }
    
    /// <summary>
    /// Variance from center spline for a <see cref="Spline2DVarianceData"/>
    /// </summary>
    public struct SplineVariance : IComponentData
    {
        public half Variance;
    }
}