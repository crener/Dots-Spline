using Crener.Spline.Common.Interfaces;
using Crener.Spline.Test._3DPlane.LinearCubicPlain.TestAdapters;
using Crener.Spline.Test._3DPlane.LinearCubicPlain.TestTypes;
using Crener.Spline.Test.BaseTests;
using UnityEngine;

namespace Crener.Spline.Test._3DPlane.LinearCubicPlain._2D
{
    /// <summary>
    /// Tests Point to point implementation of basic 3D spline functionality
    /// </summary>
    public class DynamicJobLinearCubicPlane2D : LinearCubicBaseTest2DPlaneAdapter
    {
        public override ITestSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ITestSpline spline = game.AddComponent<MeaninglessTestWrapper3.TestLinearCubicSpline3DPlaneDynamicJob>();

            return spline;
        }
    }
    
    public class DynamicJobLoopingLinearCubicPlane2D : BaseLoopingTests2D
    {
        public override ILoopingSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ILoopingSpline spline = game.AddComponent<MeaninglessTestWrapper3.TestLinearCubicSpline3DPlaneDynamicJob>();

            return spline;
        }
    }
}