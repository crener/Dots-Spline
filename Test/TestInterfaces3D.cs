using System.Collections.Generic;
using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Unity.Mathematics;

namespace Crener.Spline.Test
{
    public interface ITestSpline3D : ITestSpline
    {
        IReadOnlyList<float3> ControlPoints { get; }
    }
    
    public interface ISimpleTestSpline3D : ISimpleSpline3D, ITestSpline3D { }
    
    //public interface IVarianceTestSpline3D : ISimpleSpline3DVariance, ISimpleTestSpline { }
}