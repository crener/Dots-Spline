using Crener.Spline.Test._2D.Linear.TestAdapters;
using Crener.Spline.Test._2D.Linear.TestTypes;
using UnityEngine;

namespace Crener.Spline.Test._2D.Linear
{
    /// <summary>
    /// Tests Point to point implementation of basic 2D spline functionality
    /// </summary>
    public class BaseLinearJobTests2D : LinearBaseTest2DAdapter
    {
        protected override ISimpleTestSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ISimpleTestSpline spline = game.AddComponent<MeaninglessTestWrapper2.TestLinearSpline2DSimpleJob>();

            return spline;
        }
    }
}