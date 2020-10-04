using Unity.Jobs;
using Unity.Mathematics;

namespace Crener.Spline.Common.Interfaces
{
    public interface ISplineJob : IJob
    {
        /// <summary>
        /// The progress along the spline
        /// </summary>
        SplineProgress SplineProgress { get; set; }
    }

    public interface ISplineJob2D : ISplineJob
    {
        /// <summary>
        /// resulting spline position based on the <see cref="ISplineJob.SplineProgress"/>
        /// </summary>
        float2 Result { get; set; }
    }

    public interface ISplineJob3D : ISplineJob
    {
        /// <summary>
        /// resulting spline position based on the <see cref="ISplineJob.SplineProgress"/>
        /// </summary>
        float3 Result { get; set; }
    }
}