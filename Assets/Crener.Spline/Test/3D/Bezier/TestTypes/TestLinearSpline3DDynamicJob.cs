using Crener.Spline._3D.Jobs;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace Crener.Spline.Test._3D.Bezier.TestTypes
{
    /// <summary>
    /// Unity won't allow the creation of a component if it's inside the editor folder so this wraps the type to allow tests to run 
    /// </summary>
    public class MeaninglessTestWrapper3
    {
        public class TestBezierSpline3DDynamicJob : MeaninglessTestWrapper2.TestBezierSpline3DSimpleJob, ISimpleTestSpline3D
        {
            public override float3 Get3DPointLocal(float progress)
            {
                ClearData();
                ConvertData();

                Assert.IsTrue(SplineEntityData3D.HasValue, "Failed to generate spline");
                Dynamic3DJob job = new Dynamic3DJob(this, progress, Allocator.TempJob);
                job.Execute();

                LocalSpaceConversion3D conversion = new LocalSpaceConversion3D(Position, Forward, job.Result, Allocator.TempJob);
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
                Dynamic3DJob job = new Dynamic3DJob(this, progress, Allocator.TempJob);
                job.Execute();

                float3 jobResult = job.Result;
                job.Dispose();
                return jobResult;
            }
        }
    }
}