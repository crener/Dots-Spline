using System.Collections.Generic;
using System.Linq;
using Crener.Spline.Common;
using Crener.Spline.Linear;
using Unity.Mathematics;

namespace Crener.Spline.Test._3DPlane.LinearCubicPlain.TestTypes
{
    /// <summary>
    /// Unity won't allow the creation of a component if it's inside the editor folder so this wraps the type to allow tests to run 
    /// </summary>
    public class MeaninglessTestWrapper
    {
        /// <summary>
        /// override of base spline which implements the spline test interface
        /// </summary>
        public class TestLinearCubicSpline3DPlaneSimple : LinearCubic3DPlaneSpline, ISimpleTestSpline3D
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
            
            public int ExpectedTimeCount(int controlPoints)
            {
                if(controlPoints == 0) return 1;
                if(controlPoints == 1) return 1;
                if(controlPoints == 2) return 1;
                
                return math.max(1, controlPoints - 2);
            }
            
            public float3 GetControlPoint(int i, SplinePoint point) => GetControlPoint3DLocal(i);
        }
    }
}