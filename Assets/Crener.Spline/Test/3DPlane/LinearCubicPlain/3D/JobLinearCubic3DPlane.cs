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
    public class JobLinearCubicPlane3D : LinearCubicBaseTest3DPlaneAdapter
    {
        public override ITestSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ITestSpline spline = game.AddComponent<MeaninglessTestWrapper2.TestLinearCubicSpline3DPlaneSimpleJob>();

            return spline;
        }
    }
    
    public class JobLoopingLinearCubicPlane3D : BaseLoopingTests3DPlane
    {
        public override ILoopingSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ILoopingSpline spline = game.AddComponent<MeaninglessTestWrapper2.TestLinearCubicSpline3DPlaneSimpleJob>();

            return spline;
        }
    }
    
    public class JobPlaneLinearCubicPlane3D : Base3DPlaneTests
    {
        protected override ISpline3DPlaneEditor CreateNewSpline()
        {
            GameObject game = new GameObject();
            ISpline3DPlaneEditor spline = game.AddComponent<MeaninglessTestWrapper2.TestLinearCubicSpline3DPlaneSimpleJob>();

            return spline;
        }
    }
    
    public class JobArkLinearCubicPlane3D : BaseArkTests3DPlane
    {
        public override IArkableSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            IArkableSpline spline = game.AddComponent<MeaninglessTestWrapper2.TestLinearCubicSpline3DPlaneSimpleJob>();

            return spline;
        }
    }
}