using System.Collections.Generic;
using Crener.Spline._2D;
using Crener.Spline._3D;
using Crener.Spline.Common;
using Unity.Mathematics;

namespace Crener.Spline.Test._3D.LinearCubic.TestTypes
{
    /// <summary>
    /// Unity won't allow the creation of a component if it's inside the editor folder so this wraps the type to allow tests to run 
    /// </summary>
    public class MeaninglessTestWrapper
    {
        /// <summary>
        /// override of <see cref="BezierSpline2DSimple"/> which implements the spline test interface
        /// </summary>
        public class TestLinearCubicSpline3DSimple : LinearCubic3DSpline, ISimpleTestSpline3D
        {
            public IReadOnlyList<float3> ControlPoints => Points;
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
            
            public int ExpectedTimeCount(int controlPoints)
            {
                if(ControlPointCount <= 2) return math.max(1, controlPoints - 1);
                return math.max(1, controlPoints - 2);
            }

            public float3 GetControlPoint(int i, SplinePoint point) => GetControlPoint3DLocal(i);
        }
    }
}