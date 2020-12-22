using Crener.Spline.Common.Interfaces;
using Crener.Spline.Test._3DPlane.LinearPlain.TestAdapters;
using Crener.Spline.Test._3DPlane.LinearPlain.TestTypes;
using Crener.Spline.Test.BaseTests;
using UnityEngine;

namespace Crener.Spline.Test._3DPlane.LinearPlain._3D
{
    /// <summary>
    /// Tests Point to point implementation of basic 2D spline functionality
    /// </summary>
    public class BaseLinearPlane3D : LinearBaseTest3DPlaneAdapter
    {
        public override ITestSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ISimpleTestSpline3D spline = game.AddComponent<MeaninglessTestWrapper.TestLinearSpline3DPlaneSimple>();

            return spline;
        }
    }
    
    public class BaseLoopingLinearPlane3D : BaseLoopingTests3DPlane
    {
        public override ILoopingSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ILoopingSpline spline = game.AddComponent<MeaninglessTestWrapper.TestLinearSpline3DPlaneSimple>();

            return spline;
        }
    }
    
    public class BasePlaneLinearPlane3D : Base3DPlaneTests
    {
        protected override ISpline3DPlaneEditor CreateNewSpline()
        {
            GameObject game = new GameObject();
            ISpline3DPlaneEditor spline = game.AddComponent<MeaninglessTestWrapper.TestLinearSpline3DPlaneSimple>();

            return spline;
        }
    }
}