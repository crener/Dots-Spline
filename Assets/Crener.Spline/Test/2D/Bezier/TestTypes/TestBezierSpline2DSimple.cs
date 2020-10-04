using System.Collections.Generic;
using Crener.Spline._2D;
using Crener.Spline.Common;
using Unity.Mathematics;

namespace Crener.Spline.Test._2D.Bezier.TestTypes
{
    /// <summary>
    /// Unity won't allow the creation of a component if it's inside the editor folder so this wraps the type to allow tests to run 
    /// </summary>
    public class MeaninglessTestWrapper
    {
        /// <summary>
        /// override of <see cref="BezierSpline2DSimple"/> which implements the spline test interface
        /// </summary>
        public class TestBezierSpline2DSimple : BezierSpline2DSimple, ISimpleTestSpline
        {
            public IReadOnlyList<float2> ControlPoints => Points;
            public IReadOnlyList<float> Times => SegmentLength;
            public IReadOnlyList<SplineEditMode> Modes => PointEdit;
            
            public int ExpectedControlPointCount(int controlPoints)
            {
                return math.max(0, ((controlPoints - 1) * c_floatsPerControlPoint) + 1);
            }
            
            public int ExpectedTimeCount(int controlPoints) => math.max(1, controlPoints - 1);
        }
    }
}