using System.Collections.Generic;
using Crener.Spline._2D.Jobs;
using Crener.Spline._3D;
using Crener.Spline._3D.Jobs;
using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using NUnit.Framework;
using Unity.Mathematics;

namespace Crener.Spline.Test._3D.Bezier.TestTypes
{
    /// <summary>
    /// Unity won't allow the creation of a component if it's inside the editor folder so this wraps the type to allow tests to run 
    /// </summary>
    public class MeaninglessTestWrapper2
    {
        public class TestBezierSpline3DSimpleJob : BezierSpline3DSimple, ISimpleTestSpline3D
        {
            public IReadOnlyList<float3> ControlPoints => SplineEntityData3D.Value.Points.ToArray();
            public IReadOnlyList<float> Times => SplineEntityData3D.Value.Time.ToArray();
            public IReadOnlyList<SplineEditMode> Modes => PointEdit;

            public new float Length
            {
                get
                {
                    ClearData();
                    ConvertData();

                    Assert.IsTrue(SplineEntityData3D.HasValue, "Failed to generate spline");
                    return SplineEntityData3D.Value.Length;
                }
            }

            public override float3 Get3DPointLocal(float progress)
            {
                ClearData();
                ConvertData();

                Assert.IsTrue(SplineEntityData3D.HasValue, "Failed to generate spline");
                ISplineJob3D job = this.ExtractJob(new SplineProgress {Progress = progress});
                job.Execute();

                LocalSpaceConversion3D conversion = new LocalSpaceConversion3D()
                {
                    SplinePosition = job.Result,
                    TransformPosition = this.Position,
                    TransformRotation = this.Forward
                };
                conversion.Execute();
                
                return conversion.SplinePosition;
            }
            
            public override float3 Get3DPointWorld(float progress)
            {
                ClearData();
                ConvertData();

                Assert.IsTrue(SplineEntityData3D.HasValue, "Failed to generate spline");
                ISplineJob3D job = this.ExtractJob(new SplineProgress {Progress = progress});
                job.Execute();

                return job.Result;
            }

            public override float3 GetControlPoint3DLocal(int i) => SplineEntityData3D.Value.Points[i];

            public int ExpectedControlPointCount(int controlPoints)
            {
                return math.max(0, ((controlPoints - 1) * c_floatsPerControlPoint) + 1);
            }

            public int ExpectedTimeCount(int controlPoints) => math.max(1, controlPoints - 1);
        }
    }
}