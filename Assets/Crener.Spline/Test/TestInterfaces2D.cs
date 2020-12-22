using System.Collections.Generic;
using Crener.Spline.Common.Interfaces;
using Unity.Mathematics;

namespace Crener.Spline.Test
{
    public interface ITestSpline2D : ITestSpline
    {
        IReadOnlyList<float2> ControlPoints { get; }
    }
    
    public interface ISimpleTestSpline2D : ISimpleSpline2D, ITestSpline2D { }
    
    public interface IVarianceTestSpline2D : ISimpleSpline2DVariance, ISimpleTestSpline2D { }
}