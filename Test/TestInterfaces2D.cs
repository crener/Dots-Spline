using System.Collections.Generic;
using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Unity.Mathematics;

namespace Crener.Spline.Test
{
    public interface ITestSpline2D : ITestSpline
    {
        IReadOnlyList<float2> ControlPoints { get; }
    }
    
    public interface ISimpleTestSpline : ISimpleSpline2D, ITestSpline2D { }
    
    public interface IVarianceTestSpline : ISimpleSpline2DVariance, ISimpleTestSpline { }
}