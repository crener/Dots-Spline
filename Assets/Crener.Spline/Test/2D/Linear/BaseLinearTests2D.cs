using Crener.Spline.Common.Interfaces;
using Crener.Spline.Test._2D.Linear.TestAdapters;
using Crener.Spline.Test._2D.Linear.TestTypes;
using Crener.Spline.Test.BaseTests;
using UnityEngine;

namespace Crener.Spline.Test._2D.Linear
{
    /// <summary>
    /// Tests Point to point implementation of basic 2D spline functionality
    /// </summary>
    public class BaseLinearTests2D : LinearBaseTest2DAdapter
    {
        protected override ISimpleTestSpline2D CreateNewSpline()
        {
            GameObject game = new GameObject();
            ISimpleTestSpline2D spline2D = game.AddComponent<MeaninglessTestWrapper.TestLinearSpline2D2DSimple>();

            return spline2D;
        }
    }
    
    public class LoopingLinearTests2D : BaseLoopingTests2D
    {
        public override ILoopingSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ILoopingSpline spline = game.AddComponent<MeaninglessTestWrapper.TestLinearSpline2D2DSimple>();

            return spline;
        }
    }
}