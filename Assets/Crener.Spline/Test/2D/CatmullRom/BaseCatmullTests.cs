using Crener.Spline.Test._2D.CatmullRom.TestAdapters;
using Crener.Spline.Test._2D.CatmullRom.TestTypes;
using Crener.Spline.Test._2D.Linear.TestAdapters;
using UnityEngine;

namespace Crener.Spline.Test._2D.CatmullRom
{
    /// <summary>
    /// Tests Point to point implementation of basic 2D spline functionality
    /// </summary>
    public class BaseCatmullTests : CatmullBaseTestAdapter
    {
        protected override ISimpleTestSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ISimpleTestSpline spline = game.AddComponent <MeaninglessTestWrapper.TestCatmullSpline2DSimple>();

            return spline;
        }
    }
}