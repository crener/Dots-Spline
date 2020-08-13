using System.Collections.Generic;
using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Unity.Mathematics;

namespace Crener.Spline.Test
{
    public interface ITestSpline
    {
        IReadOnlyList<float> Times { get; }
        IReadOnlyList<SplineEditMode> Modes { get; }

        /// <summary>
        /// Translates the amount of <paramref name="controlPoints"/> into the expected amount of points that the spline should actually
        /// contain
        /// </summary>
        /// <param name="controlPoints">Amount of control points</param>
        /// <returns>expected amount of points</returns>
        int ExpectedControlPointCount(int controlPoints);
        
        /// <summary>
        /// Translates the amount of <see cref="Times"/> items are expected from the given amount of <param name="controlPoints"/>
        /// </summary>
        /// <param name="controlPoints">amount of control points in the spline</param>
        /// <returns>expected amount of time points</returns>
        int ExpectedTimeCount(int controlPoints);
    }


    public interface IArkableTestSpline : ITestSpline, IArkableSpline
    {
        /// <summary>
        /// The amount of control points that the spline should be producing when converting data
        /// </summary>
        int ExpectedPointCount { get; }
    }
}