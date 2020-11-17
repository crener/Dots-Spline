using System.Collections.Generic;
using Crener.Spline._3D;
using Crener.Spline._3D.Jobs;
using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
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

            public override float3 Get3DPointLocal(float progress)
            {
                ClearData();
                ConvertData();

                Assert.IsTrue(SplineEntityData3D.HasValue, "Failed to generate spline");
                ISplineJob3D job = this.ExtractJob(new SplineProgress(progress));
                job.Execute();

                LocalSpaceConversion3D conversion = new LocalSpaceConversion3D(Position, Forward, job.Result);
                conversion.Execute();
                
                float3 pos = conversion.SplinePosition.Value;
                conversion.Dispose();
                job.Dispose();
                return pos;
            }
            
            public override float3 Get3DPointWorld(float progress)
            {
                ClearData();
                ConvertData();

                Assert.IsTrue(SplineEntityData3D.HasValue, "Failed to generate spline");
                ISplineJob3D job = this.ExtractJob(new SplineProgress(progress));
                job.Execute();

                float3 jobResult = job.Result;
                job.Dispose();
                return jobResult;
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