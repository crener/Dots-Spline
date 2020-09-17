using System.Collections.Generic;
using System.Linq;
using Crener.Spline.BezierSpline;
using Crener.Spline.Common;
using Crener.Spline.Linear;
using Unity.Mathematics;

namespace Crener.Spline.Test._3DPlane.LinearPlain.TestTypes
{
    /// <summary>
    /// Unity won't allow the creation of a component if it's inside the editor folder so this wraps the type to allow tests to run 
    /// </summary>
    public class MeaninglessTestWrapper
    {
        /// <summary>
        /// override of base spline which implements the spline test interface
        /// </summary>
        public class TestLinearSpline3DPlaneSimple : Linear3DPlaneSpline, ISimpleTestSpline3D
        {
            public IReadOnlyList<float3> ControlPoints => Points.Select(p => Convert2Dto3D(p, true)).ToList();
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

            public float3 GetControlPoint(int i, SplinePoint point) => GetControlPoint3D(i);
        }
    }
}