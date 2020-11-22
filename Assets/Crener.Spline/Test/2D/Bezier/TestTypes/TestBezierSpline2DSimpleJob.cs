using System.Collections.Generic;
using Crener.Spline._2D;
using Crener.Spline._2D.Jobs;
using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace Crener.Spline.Test._2D.Bezier.TestTypes
{
    /// <summary>
    /// Unity won't allow the creation of a component if it's inside the editor folder so this wraps the type to allow tests to run 
    /// </summary>
    public class MeaninglessTestWrapper2
    {
        public class TestBezierSpline2DSimpleJob : BezierSpline2DSimple, ISimpleTestSpline
        {
            public IReadOnlyList<float2> ControlPoints => SplineEntityData2D.Value.Points.ToArray();
            public IReadOnlyList<float> Times => SplineEntityData2D.Value.Time.ToArray();
            public IReadOnlyList<SplineEditMode> Modes => PointEdit;

            public new float Length
            {
                get
                {
                    ClearData();
                    ConvertData();

                    Assert.IsTrue(SplineEntityData2D.HasValue, "Failed to generate spline");
                    return SplineEntityData2D.Value.Length;
                }
            }

            public new virtual float2 Get2DPointLocal(float progress)
            {
                ClearData();
                ConvertData();

                Assert.IsTrue(SplineEntityData2D.HasValue, "Failed to generate spline");
                ISplineJob2D job = this.ExtractJob(progress, Allocator.TempJob);
                job.Execute();

                LocalSpaceConversion2D conversion = new LocalSpaceConversion2D(this.Position.xy, job.Result, Allocator.TempJob);
                conversion.Execute();

                float2 result = conversion.SplinePosition.Value;
                job.Dispose();
                conversion.Dispose();
                return result;
            }
            
            public new virtual float2 Get2DPointWorld(float progress)
            {
                ClearData();
                ConvertData();

                Assert.IsTrue(SplineEntityData2D.HasValue, "Failed to generate spline");
                ISplineJob2D job = this.ExtractJob(progress, Allocator.TempJob);
                job.Execute();

                float2 result = job.Result;
                job.Dispose();
                return result;
            }

            public override float2 GetControlPoint2DLocal(int i) => SplineEntityData2D.Value.Points[i];

            public int ExpectedControlPointCount(int controlPoints)
            {
                return math.max(0, ((controlPoints - 1) * c_floatsPerControlPoint) + 1);
            }

            public int ExpectedTimeCount(int controlPoints) => math.max(1, controlPoints - 1);
        }
    }
}