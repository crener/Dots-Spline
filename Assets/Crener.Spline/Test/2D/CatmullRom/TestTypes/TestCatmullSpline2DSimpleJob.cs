using System.Collections.Generic;
using Crener.Spline.CatmullRom;
using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using NUnit.Framework;
using Unity.Mathematics;

namespace Crener.Spline.Test._2D.CatmullRom.TestTypes
{
    /// <summary>
    /// Unity won't allow the creation of a component if it's inside the editor folder so this wraps the type to allow tests to run 
    /// </summary>
    public class MeaninglessTestWrapper2
    {
        public class TestCatmullSpline2DSimpleJob : CatmullRom2DSpline, ISimpleTestSpline
        {
            public IReadOnlyList<float2> ControlPoints => SplineEntityData.Value.Points.ToArray();
            public IReadOnlyList<float> Times => SplineEntityData.Value.Time.ToArray();
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

            public new float2 GetPoint(float progress)
            {
                ClearData();
                ConvertData();

                Assert.IsTrue(SplineEntityData.HasValue, "Failed to generate spline");
                ISplineJob2D job = this.ExtractJob(progress);
                job.Execute();

                return job.Result;
            }

            public int ExpectedControlPointCount(int controlPoints) => controlPoints;

            public int ExpectedTimeCount(int controlPoints) => math.max(1, controlPoints - 1);
            
            public float2 GetControlPoint(int i, SplinePoint point) => GetControlPoint(i);
        }
    }
}