using System.Collections.Generic;
using Crener.Spline._2D;
using Crener.Spline._2D.Jobs;
using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using NUnit.Framework;
using Unity.Mathematics;

namespace Crener.Spline.Test._2D.Linear.TestTypes
{
    /// <summary>
    /// Unity won't allow the creation of a component if it's inside the editor folder so this wraps the type to allow tests to run 
    /// </summary>
    public class MeaninglessTestWrapper2
    {
        public class TestLinearSpline2DSimpleJob : Linear2DSpline, ISimpleTestSpline
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

            public new float2 Get2DPointLocal(float progress)
            {
                ClearData();
                ConvertData();

                Assert.IsTrue(SplineEntityData2D.HasValue, "Failed to generate spline");
                ISplineJob2D job = this.ExtractJob(new SplineProgress {Progress = progress});
                job.Execute();

                LocalSpaceConversion2D conversion = new LocalSpaceConversion2D()
                {
                    SplinePosition = job.Result,
                    TransformPosition = this.Position.xy
                };
                conversion.Execute();

                return conversion.SplinePosition;
            }
            
            public new float2 Get2DPointWorld(float progress)
            {
                ClearData();
                ConvertData();

                Assert.IsTrue(SplineEntityData2D.HasValue, "Failed to generate spline");
                ISplineJob2D job = this.ExtractJob(new SplineProgress {Progress = progress});
                job.Execute();

                return job.Result;
            }

            public int ExpectedControlPointCount(int controlPoints) => controlPoints;

            public int ExpectedTimeCount(int controlPoints) => math.max(1, controlPoints - 1);
            
            public float2 GetControlPoint(int i, SplinePoint point) => GetControlPoint2DLocal(i);
        }
    }
}