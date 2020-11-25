using Crener.Spline._3D.Jobs;
using Crener.Spline.Common.Interfaces;
using Unity.Assertions;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        const float progress = 0.3f;
        ISpline3D spline3D = GetComponent<ISpline3D>();
        
        // calculate the point manually
        Dynamic3DJob dynamic2 = new Dynamic3DJob(spline3D, progress, Allocator.Temp);
        dynamic2.Execute();
        float3 manualResult = dynamic2.Result;
        dynamic2.Dispose();

        // calculate the point using dynamic job
        Dynamic3DJob dynamic = new Dynamic3DJob(spline3D, progress, Allocator.TempJob);
        JobHandle handle = dynamic.Schedule();
        handle.Complete();
        float3 dynamicJobResult = dynamic.Result;
        dynamic.Dispose();
        
        // calculate the point using specific job type
        BezierSpline3DPointJob direct = new BezierSpline3DPointJob(spline3D, progress, Allocator.TempJob);
        JobHandle handle2 = direct.Schedule(handle);
        handle2.Complete();
        float3 directJobResult = direct.Result;
        direct.Dispose();

        if(handle2.IsCompleted)
        {
            Assert.AreEqual(manualResult, directJobResult);
            Assert.AreEqual(manualResult, dynamicJobResult);
        }
    }
}
