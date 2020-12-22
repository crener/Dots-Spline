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
    public class JobLinearPlane2D : LinearCubicBaseTest2DPlaneAdapter
    {
        public override ITestSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ITestSpline spline = game.AddComponent<MeaninglessTestWrapper2.TestLinearCubicSpline3DPlaneSimpleJob>();

            return spline;
        }
    }
    
    public class JobLoopingLinearPlane2D : BaseLoopingTests2D
    {
        public override ILoopingSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ILoopingSpline spline = game.AddComponent<MeaninglessTestWrapper2.TestLinearCubicSpline3DPlaneSimpleJob>();

            return spline;
        }
    }
    
    public class JobArkLinearPlane2D : BaseArkTests2D
    {
        public override IArkableSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            IArkableSpline spline = game.AddComponent<MeaninglessTestWrapper2.TestLinearCubicSpline3DPlaneSimpleJob>();

            return spline;
        }
    }
}