using Crener.Spline._3D.Jobs;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace Crener.Spline.Test._3D.Linear.TestTypes
{
    /// <summary>
    /// Unity won't allow the creation of a component if it's inside the editor folder so this wraps the type to allow tests to run 
    /// </summary>
    public class MeaninglessTestWrapper3
    {
        public class TestLinearSpline3DDynamicJob : MeaninglessTestWrapper2.TestLinearSpline3DSimpleJob, ISimpleTestSpline3D
        {
            public override float3 Get3DPointLocal(float progress)
            {
                ClearData();
                ConvertData();

                Assert.IsTrue(SplineEntityData3D.HasValue, "Failed to generate spline");
                Dynamic3DJob job = new Dynamic3DJob(this, progress, Allocator.Temp);
                job.Execute();

                LocalSpaceConversion3D conversion = new LocalSpaceConversion3D(Position, Forward, job.Result, Allocator.Temp);
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
                Dynamic3DJob job = new Dynamic3DJob(this, progress, Allocator.Temp);
                job.Execute();

                float3 jobResult = job.Result;
                job.Dispose();
                return jobResult;
            }
        }
    }
}