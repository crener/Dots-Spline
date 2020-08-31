using Crener.Spline.Common.Interfaces;
using Crener.Spline.Test._3D.Linear.TestAdapters;
using Crener.Spline.Test._3DPlane.LinearPlain.TestAdapters;
using Crener.Spline.Test._3DPlane.LinearPlain.TestTypes;
using Crener.Spline.Test.BaseTests;
using UnityEngine;

namespace Crener.Spline.Test._3DPlane.LinearPlain
{
    /// <summary>
    /// Tests Point to point implementation of basic 2D spline functionality
    /// </summary>
    public class BaseLinearTests3DPlane : LinearBaseTest3DPlaneAdapter
    {
        public override ITestSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ISimpleTestSpline3D spline = game.AddComponent<MeaninglessTestWrapper.TestLinearSpline3DPlaneSimple>();

            return spline;
        }
    }
    
    public class LoopingLinearTests3DPlane : BaseLoopingTests3DPlane
    {
        public override ILoopingSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ILoopingSpline spline = game.AddComponent<MeaninglessTestWrapper.TestLinearSpline3DPlaneSimple>();

            return spline;
        }
    }
}