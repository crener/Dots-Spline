using Crener.Spline.Test._2D.CatmullRom.TestAdapters;
using Crener.Spline.Test._2D.CatmullRom.TestTypes;
using UnityEngine;

namespace Crener.Spline.Test._2D.CatmullRom
{
    /// <summary>
    /// Tests Point to point implementation of basic 2D spline functionality
    /// </summary>
    public class BaseCatmullTests2DJob : CatmullBaseTest2DAdapter
    {
        protected override ISimpleTestSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ISimpleTestSpline spline = game.AddComponent<MeaninglessTestWrapper2.TestCatmullSpline2DSimpleJob>();

            return spline;
        }
    }
}