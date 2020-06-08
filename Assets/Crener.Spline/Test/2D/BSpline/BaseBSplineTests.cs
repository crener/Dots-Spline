using Crener.Spline.Test._2D.BSpline.TestAdapters;
using Crener.Spline.Test._2D.BSpline.TestTypes;
using Crener.Spline.Test.Helpers;
using NUnit.Framework;
using UnityEngine;

namespace Crener.Spline.Test._2D.BSpline
{
    /// <summary>
    /// Tests Point to point implementation of basic 2D spline functionality
    /// </summary>
    public class BaseBSplineTests : SelfCleanUpTestSet
    {
        protected ISimpleTestSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            MeaninglessTestWrapper.TestBSpline2DSimple spline = game.AddComponent<MeaninglessTestWrapper.TestBSpline2DSimple>();
            Assert.IsNotNull(spline);
            spline.Looped = false;

            TestHelpers.ClearSpline(spline);
            m_disposables.Add(spline);

            return spline;
        }
    }
}