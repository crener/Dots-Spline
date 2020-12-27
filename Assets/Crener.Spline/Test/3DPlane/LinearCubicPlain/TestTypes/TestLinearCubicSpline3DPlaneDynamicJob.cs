using Crener.Spline._2D.Jobs;
using Crener.Spline._3D.Jobs;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace Crener.Spline.Test._3DPlane.LinearCubicPlain.TestTypes
{
    /// <summary>
    /// Unity won't allow the creation of a component if it's inside the editor folder so this wraps the type to allow tests to run 
    /// </summary>
    public class MeaninglessTestWrapper3
    {
        /// <summary>
        /// override of base spline which implements the spline test interface
        /// </summary>
        public class TestLinearCubicSpline3DPlaneDynamicJob : MeaninglessTestWrapper.TestLinearCubicSpline3DPlaneSimple, ISimpleTestSpline3D
        {
            public new float3 Get3DPointLocal(float progress)
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
            
            public new float3 Get3DPointWorld(float progress)
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
            
            public new float2 Get2DPointLocal(float progress)
            {
                ClearData();
                ConvertData();

                Assert.IsTrue(SplineEntityData3D.HasValue, "Failed to generate spline");
                Dynamic2DJob job = new Dynamic2DJob(this, progress, Allocator.TempJob);
                job.Execute();

                LocalSpaceConversion2D conversion = new LocalSpaceConversion2D(Position.xy, job.Result, Allocator.TempJob);
                conversion.Execute();
                
                float2 pos = conversion.SplinePosition.Value;
                conversion.Dispose();
                job.Dispose();
                return pos;
            }
            
            public new float2 Get2DPointWorld(float progress)
            {
                ClearData();
                ConvertData();

                Assert.IsTrue(SplineEntityData3D.HasValue, "Failed to generate spline");
                Dynamic2DJob job = new Dynamic2DJob(this, progress, Allocator.TempJob);
                job.Execute();

                float2 jobResult = job.Result;
                job.Dispose();
                return jobResult;
            }
        }
    }
}