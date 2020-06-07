using System.Collections.Generic;
using Crener.Spline.BezierSpline;
using Crener.Spline.BezierSpline.Jobs;
using Crener.Spline.Common;
using Crener.Spline.PointToPoint;
using Crener.Spline.PointToPoint.Jobs._2D;
using NUnit.Framework;
using Unity.Jobs;
using Unity.Mathematics;

namespace Crener.Spline.Test._2D.P2P.TestTypes
{
    /// <summary>
    /// Unity won't allow the creation of a component if it's inside the editor folder so this wraps the type to allow tests to run 
    /// </summary>
    public class MeaninglessTestWrapper2
    {
        public class TestP2PSpline2DSimpleJob : PointToPoint2DSpline, ISimpleTestSpline
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
                PointToPointSpline2DPointJob job = new PointToPointSpline2DPointJob()
                {
                    Spline = SplineEntityData.Value,
                    SplineProgress = new SplineProgress() {Progress = progress}
                };
                job.Execute();

                return job.Result;
            }

            public int ExpectedPointCountPerControlPoint(int controlPoints) => controlPoints;
            
            public float2 GetControlPoint(int i, SplinePoint point) => GetControlPoint(i);
        }
    }
}