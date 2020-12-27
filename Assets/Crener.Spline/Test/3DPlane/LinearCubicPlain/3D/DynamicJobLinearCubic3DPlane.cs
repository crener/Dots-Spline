using Crener.Spline.Common.Interfaces;
using Crener.Spline.Test._3DPlane.LinearCubicPlain.TestAdapters;
using Crener.Spline.Test._3DPlane.LinearCubicPlain.TestTypes;
using Crener.Spline.Test.BaseTests;
using UnityEngine;

namespace Crener.Spline.Test._3DPlane.LinearCubicPlain._3D
{
    /// <summary>
    /// Tests Point to point implementation of basic 3D spline functionality
    /// </summary>
    public class DynamicJobLinearCubicPlane3D : LinearCubicBaseTest3DPlaneAdapter
    {
        public override ITestSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ISimpleTestSpline3D spline = game.AddComponent<MeaninglessTestWrapper3.TestLinearCubicSpline3DPlaneDynamicJob>();

            return spline;
        }
    }
    
    public class DynamicJobLoopingLinearCubicPlane3D : BaseLoopingTests3DPlane
    {
        public override ILoopingSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ILoopingSpline spline = game.AddComponent<MeaninglessTestWrapper3.TestLinearCubicSpline3DPlaneDynamicJob>();

            return spline;
        }
    }
    
    public class DynamicJobArkLinearCubicPlane3D : BaseArkTests3DPlane
    {
        public override IArkableSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            IArkableSpline spline = game.AddComponent<MeaninglessTestWrapper3.TestLinearCubicSpline3DPlaneDynamicJob>();

            return spline;
        }
    }
}