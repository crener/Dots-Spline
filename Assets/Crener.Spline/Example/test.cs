using System.Collections;
using System.Collections.Generic;
using Crener.Spline._3D.Jobs;
using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Unity.Assertions;
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
        Dynamic3DJob dynamic2 = new Dynamic3DJob(spline3D, progress);
        dynamic2.Execute();
        float3 manualResult = dynamic2.Result;

        // calculate the point using dynamic job
        Dynamic3DJob dynamic = new Dynamic3DJob(spline3D, progress);
        JobHandle handle = dynamic.Schedule();
        handle.Complete();
        float3 dynamicJobResult = dynamic.Result;
        
        // calculate the point using specific job type
        BezierSpline3DPointJob direct = new BezierSpline3DPointJob
        {
            Spline = spline3D.SplineEntityData3D.Value, 
            SplineProgress = new SplineProgress(progress)
        };
        JobHandle handle2 = direct.Schedule();
        handle2.Complete();
        float3 directJobResult = direct.Result;

        if(handle.IsCompleted)
        {
            Assert.AreEqual(manualResult, directJobResult);
            Assert.AreEqual(manualResult, dynamicJobResult);
        }
    }
}
