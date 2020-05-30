using System.Collections.Generic;
using Crener.Spline.BezierSpline;
using Crener.Spline.Common;
using Crener.Spline.PointToPoint;
using Unity.Mathematics;

namespace Crener.Spline.Test._2D.P2P.TestTypes
{
    /// <summary>
    /// Unity won't allow the creation of a component if it's inside the editor folder so this wraps the type to allow tests to run 
    /// </summary>
    public class MeaninglessTestWrapper
    {
        /// <summary>
        /// override of <see cref="BezierSpline2DSimple"/> which implements the spline test interface
        /// </summary>
        public class TestP2PSpline2DSimple : PointToPoint2DSpline, ISimpleTestSpline
        {
            public IReadOnlyList<float2> ControlPoints => Points;
            public IReadOnlyList<float> Times => SegmentLength;
            public IReadOnlyList<SplineEditMode> Modes
            {
                get
                {
                    List<SplineEditMode> mode = new List<SplineEditMode>(ControlPointCount);
                    for (int i = 0; i < ControlPointCount; i++)
                    {
                        mode.Add(GetEditMode(i));
                    }

                    return mode;
                }
            }
            
            public int ExpectedPointCountPerControlPoint(int controlPoints) => controlPoints;

            public float2 GetControlPoint(int i, SplinePoint point) => GetControlPoint(i);
        }
    }
}