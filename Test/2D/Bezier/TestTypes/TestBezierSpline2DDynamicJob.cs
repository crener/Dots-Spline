using Crener.Spline._2D.Jobs;
using Crener.Spline.Common.Interfaces;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace Crener.Spline.Test._2D.Bezier.TestTypes
{
    /// <summary>
    /// Unity won't allow the creation of a component if it's inside the editor folder so this wraps the type to allow tests to run 
    /// </summary>
    public class MeaninglessTestWrapper3
    {
        public class TestBezierSpline2D2DDynamicJob : MeaninglessTestWrapper2.TestBezierSpline2D2DSimpleJob
        {
            public override float2 Get2DPointLocal(float progress)
            {
                ClearData();
                ConvertData();

                Assert.IsTrue(SplineEntityData2D.HasValue, "Failed to generate spline");
                ISplineJob2D job = new Dynamic2DJob(this, progress, Allocator.TempJob);
                job.Execute();

                LocalSpaceConversion2D conversion = new LocalSpaceConversion2D(this.Position.xy, job.Result, Allocator.TempJob);
                conversion.Execute();

                float2 result = conversion.SplinePosition.Value;
                job.Dispose();
                conversion.Dispose();
                return result;
            }
            
            public override float2 Get2DPointWorld(float progress)
            {
                ClearData();
                ConvertData();

                Assert.IsTrue(SplineEntityData2D.HasValue, "Failed to generate spline");
                ISplineJob2D job = new Dynamic2DJob(this, progress, Allocator.TempJob);
                job.Execute();

                float2 result = job.Result;
                job.Dispose();
                return result;
            }
        }
    }
}