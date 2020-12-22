using Crener.Spline.Common.Interfaces;
using Crener.Spline.Test._3DPlane.LinearCubicPlain.TestAdapters;
using Crener.Spline.Test._3DPlane.LinearCubicPlain.TestTypes;
using Crener.Spline.Test.BaseTests;
using UnityEngine;

namespace Crener.Spline.Test._3DPlane.LinearCubicPlain._3D
{
    /// <summary>
    /// Tests Point to point implementation of basic 2D spline functionality
    /// </summary>
    public class BaseLinearCubicPlane3D : LinearCubicBaseTest3DPlaneAdapter
    {
        public override ITestSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ISimpleTestSpline3D spline = game.AddComponent<MeaninglessTestWrapper.TestLinearCubicSpline3DPlaneSimple>();

            return spline;
        }
    }
    
    public class BaseLoopingLinearCubicPlane3D : BaseLoopingTests3DPlane
    {
        public override ILoopingSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ILoopingSpline spline = game.AddComponent<MeaninglessTestWrapper.TestLinearCubicSpline3DPlaneSimple>();

            return spline;
        }
    }
    
    public class BasePlaneLinearCubicPlane3D : Base3DPlaneTests
    {
        protected override ISpline3DPlaneEditor CreateNewSpline()
        {
            GameObject game = new GameObject();
            ISpline3DPlaneEditor spline = game.AddComponent<MeaninglessTestWrapper.TestLinearCubicSpline3DPlaneSimple>();

            return spline;
        }
    }
}