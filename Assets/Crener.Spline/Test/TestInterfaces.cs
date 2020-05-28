using System.Collections.Generic;
using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Unity.Mathematics;

namespace Crener.Spline.Test
{
    public interface ISimpleTestSpline : ISimpleSpline2D
    {
        IReadOnlyList<float2> ControlPoints { get; }
        IReadOnlyList<float> Times { get; }
        IReadOnlyList<SplineEditMode> Modes { get; }
    }
    
    public interface IVarianceTestSpline : ISimpleSpline2DVariance
    {
        IReadOnlyList<float2> ControlPoints { get; }
        IReadOnlyList<float> Times { get; }
        IReadOnlyList<SplineEditMode> Modes { get; }
    }
}