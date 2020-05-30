using System.Collections.Generic;
using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Unity.Mathematics;

namespace Crener.Spline.Test
{
    public interface ITestSpline
    {
        IReadOnlyList<float2> ControlPoints { get; }
        IReadOnlyList<float> Times { get; }
        IReadOnlyList<SplineEditMode> Modes { get; }

        /// <summary>
        /// Translates the amount of <paramref name="controlPoints"/> into the expected amount of points that the spline should actually
        /// contain
        /// </summary>
        /// <param name="controlPoints">Amount of control points</param>
        /// <returns>expected amount of points</returns>
        int ExpectedPointCountPerControlPoint(int controlPoints);
    }
    
    public interface ISimpleTestSpline : ISimpleSpline2D, ITestSpline { }
    
    public interface IVarianceTestSpline : ISimpleSpline2DVariance, ISimpleTestSpline { }
}