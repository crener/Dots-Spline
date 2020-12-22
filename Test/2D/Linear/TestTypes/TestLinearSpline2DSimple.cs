using System.Collections.Generic;
using Crener.Spline._2D;
using Crener.Spline.Common;
using Unity.Mathematics;

namespace Crener.Spline.Test._2D.Linear.TestTypes
{
    /// <summary>
    /// Unity won't allow the creation of a component if it's inside the editor folder so this wraps the type to allow tests to run 
    /// </summary>
    public class MeaninglessTestWrapper
    {
        /// <summary>
        /// override of <see cref="BezierSpline2DSimple"/> which implements the spline test interface
        /// </summary>
        public class TestLinearSpline2D2DSimple : Linear2DSpline, ISimpleTestSpline2D
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
            
            public int ExpectedControlPointCount(int controlPoints) => controlPoints;
            
            public int ExpectedTimeCount(int controlPoints) => math.max(1, controlPoints - 1);

            public float2 GetControlPoint(int i, SplinePoint point) => GetControlPoint2DLocal(i);
        }
    }
}