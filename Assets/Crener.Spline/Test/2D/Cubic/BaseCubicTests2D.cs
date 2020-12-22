using Crener.Spline.Test._2D.Cubic.TestAdapters;
using Crener.Spline.Test._2D.Cubic.TestTypes;
using UnityEngine;

namespace Crener.Spline.Test._2D.Cubic
{
    /// <summary>
    /// Tests Point to point implementation of basic 2D spline functionality
    /// </summary>
    public class BaseCubicTests2D : CubicBaseTest2DAdapter
    {
        protected override ISimpleTestSpline2D CreateNewSpline()
        {
            GameObject game = new GameObject();
            ISimpleTestSpline2D spline2D = game.AddComponent<MeaninglessTestWrapper.TestCubicSpline2D2DSimple>();

            return spline2D;
        }
    }
}