using System.Collections.Generic;
using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Crener.Spline.Linear;
using NUnit.Framework;
using Unity.Mathematics;

namespace Crener.Spline.Test._3D.LinearCubic.TestTypes
{
    /// <summary>
    /// Unity won't allow the creation of a component if it's inside the editor folder so this wraps the type to allow tests to run 
    /// </summary>
    public class MeaninglessTestWrapper2
    {
        public class TestLinearCubicSpline3DSimpleJob : LinearCubic3DSpline, ISimpleTestSpline3D
        {
            public IReadOnlyList<float3> ControlPoints => SplineEntityData3D.Value.Points.ToArray();
            public IReadOnlyList<float> Times => SplineEntityData3D.Value.Time.ToArray();
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

            public new float3 Get3DPoint(float progress)
            {
                ClearData();
                ConvertData();

                Assert.IsTrue(SplineEntityData3D.HasValue, "Failed to generate spline");
                ISplineJob3D job = this.ExtractJob(progress);
                job.Execute();

                return job.Result;
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