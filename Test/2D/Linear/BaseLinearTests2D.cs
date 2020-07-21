using Crener.Spline.Common.Interfaces;
using Crener.Spline.Test._2D.Linear.TestAdapters;
using Crener.Spline.Test._2D.Linear.TestTypes;
using UnityEngine;

namespace Crener.Spline.Test._2D.Linear
{
    /// <summary>
    /// Tests Point to point implementation of basic 2D spline functionality
    /// </summary>
    public class BaseLinearTests2D : LinearBaseTest2DAdapter
    {
        protected override ISimpleTestSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ISimpleTestSpline spline = game.AddComponent<MeaninglessTestWrapper.TestLinearSpline2DSimple>();

            return spline;
        }
    }
    
    public class LoopingLinearTests2D : BaseLoopingTests2D
    {
        protected override ILoopingSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ILoopingSpline spline = game.AddComponent<MeaninglessTestWrapper.TestLinearSpline2DSimple>();

            return spline;
        }
    }
}