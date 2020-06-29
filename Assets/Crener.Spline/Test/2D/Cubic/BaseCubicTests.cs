using Crener.Spline.Test._2D.Cubic.TestAdapters;
using Crener.Spline.Test._2D.Cubic.TestTypes;
using UnityEngine;

namespace Crener.Spline.Test._2D.Cubic
{
    /// <summary>
    /// Tests Point to point implementation of basic 2D spline functionality
    /// </summary>
    public class BaseCubicTests : CubicBaseTestAdapter
    {
        protected override ISimpleTestSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ISimpleTestSpline spline = game.AddComponent<MeaninglessTestWrapper.TestCubicSpline2DSimple>();

            return spline;
        }
    }
}