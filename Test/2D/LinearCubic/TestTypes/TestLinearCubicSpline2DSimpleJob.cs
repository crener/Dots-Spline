using System.Collections.Generic;
using Crener.Spline._2D;
using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using NUnit.Framework;
using Unity.Mathematics;

namespace Crener.Spline.Test._2D.LinearCubic.TestTypes
{
    /// <summary>
    /// Unity won't allow the creation of a component if it's inside the editor folder so this wraps the type to allow tests to run 
    /// </summary>
    public class MeaninglessTestWrapper
    {
        public class TestLinearCubic2DSplineSimpleJob : LinearCubic2DSpline, ISimpleTestSpline
        {
            public IReadOnlyList<float2> ControlPoints => SplineEntityData2D.Value.Points.ToArray();
            public IReadOnlyList<float> Times => SplineEntityData2D.Value.Time.ToArray();
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

            public new float2 Get2DPoint(float progress)
            {
                ClearData();
                ConvertData();

                Assert.IsTrue(SplineEntityData2D.HasValue, "Failed to generate spline");
                ISplineJob2D job = this.ExtractJob(progress);
                job.Execute();

                return job.Result;
            }

            public int ExpectedControlPointCount(int controlPoints) => controlPoints;

            public int ExpectedTimeCount(int controlPoints)
            {
                if(controlPoints == 0) return 1;
                if(controlPoints == 1) return 1;
                if(controlPoints == 2) return 1;
                
                return math.max(1, controlPoints - 2);
            }
            
            public float2 GetControlPoint(int i, SplinePoint point) => GetControlPoint2DLocal(i);
        }
    }
}