using System.Collections.Generic;
using Crener.Spline._3D;
using Crener.Spline._3D.Jobs;
using Crener.Spline.Common;
using NUnit.Framework;
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
                Dynamic3DJob job = new Dynamic3DJob(this, progress);
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
                Dynamic3DJob job = new Dynamic3DJob(this, progress);
                job.Execute();

                return job.Result;
            }
        }
    }
}