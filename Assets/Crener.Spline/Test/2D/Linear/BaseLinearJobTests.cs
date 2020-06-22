using Crener.Spline.Test._2D.P2P.TestAdapters;
using Crener.Spline.Test._2D.P2P.TestTypes;
using UnityEngine;

namespace Crener.Spline.Test._2D.P2P
{
    /// <summary>
    /// Tests Point to point implementation of basic 2D spline functionality
    /// </summary>
    public class BaseLinearJobTests : LinearBaseTestAdapter
    {
        protected override ISimpleTestSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ISimpleTestSpline spline = game.AddComponent<MeaninglessTestWrapper2.TestP2PSpline2DSimpleJob>();

            return spline;
        }
    }
}