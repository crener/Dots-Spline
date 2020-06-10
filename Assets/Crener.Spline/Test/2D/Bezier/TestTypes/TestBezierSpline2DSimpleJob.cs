using System.Collections.Generic;
using Crener.Spline.BezierSpline;
using Crener.Spline.BezierSpline.Jobs;
using Crener.Spline.Common;
using NUnit.Framework;
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
            public IReadOnlyList<float2> ControlPoints => SplineEntityData.Value.Points.ToArray();
            public IReadOnlyList<float> Times => SplineEntityData.Value.Time.ToArray();
            public IReadOnlyList<SplineEditMode> Modes => PointEdit;

            public new float Length
            {
                get
                {
                    ClearData();
                    ConvertData();

                    Assert.IsTrue(SplineEntityData.HasValue, "Failed to generate spline");
                    return SplineEntityData.Value.Length;
                }
            }

            public new float2 GetPoint(float progress)
            {
                ClearData();
                ConvertData();

                Assert.IsTrue(SplineEntityData.HasValue, "Failed to generate spline");
                BezierSpline2DPointJob job = new BezierSpline2DPointJob()
                {
                    Spline = SplineEntityData.Value,
                    SplineProgress = new SplineProgress() {Progress = progress}
                };
                job.Execute();

                return job.Result;
            }

            public override float2 GetControlPoint(int i) => SplineEntityData.Value.Points[i];

            public int ExpectedControlPointCount(int controlPoints)
            {
                return math.max(0, ((controlPoints - 1) * c_floatsPerControlPoint) + 1);
            }
            
            public int ExpectedTimeCount(int controlPoints) => math.max(1, controlPoints - 1);
        }
    }
}