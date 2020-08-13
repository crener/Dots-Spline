using Crener.Spline.BezierSpline;
using Crener.Spline.Common.Interfaces;
using Crener.Spline.Test._2D.Bezier.TestAdapters;
using Crener.Spline.Test._2D.Bezier.TestTypes;
using Crener.Spline.Test.BaseTests;
using Crener.Spline.Test.Helpers;
using NUnit.Framework;
using UnityEngine;

namespace Crener.Spline.Test._2D.Bezier
{
    /// <summary>
    /// Override for testing <see cref="BezierSpline2DSimple"/>
    /// </summary>
    public class BaseBezierSpline2DTests2D : BezierBaseTest2DAdapter
    {
        protected override ISimpleTestSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            MeaninglessTestWrapper.TestBezierSpline2DSimple testBezierSpline = game.AddComponent<MeaninglessTestWrapper.TestBezierSpline2DSimple>();
            Assert.IsNotNull(testBezierSpline);

            TestHelpers.ClearSpline(testBezierSpline);

            m_disposables.Add(testBezierSpline);
            return testBezierSpline;
        }
    }
    
    public class ArkBezierSplineTests2D : BaseArkTests2D
    {
        protected override IArkableSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            MeaninglessTestWrapper.TestBezierSpline2DSimple testBezierSpline = game.AddComponent<MeaninglessTestWrapper.TestBezierSpline2DSimple>();

            return testBezierSpline;
        }
    }
}