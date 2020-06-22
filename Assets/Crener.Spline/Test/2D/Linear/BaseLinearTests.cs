using Crener.Spline.Test._2D.P2P.TestTypes;
using UnityEngine;

namespace Crener.Spline.Test._2D.P2P
{
    /// <summary>
    /// Tests Point to point implementation of basic 2D spline functionality
    /// </summary>
    public class BaseLinearTests : BaseSimpleSplineTests
    {
        protected override ISimpleTestSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ISimpleTestSpline spline = game.AddComponent<MeaninglessTestWrapper.TestP2PSpline2DSimple>();

            return spline;
        }
    }
}