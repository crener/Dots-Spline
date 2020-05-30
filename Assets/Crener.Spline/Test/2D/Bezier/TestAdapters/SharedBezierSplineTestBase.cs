using Crener.Spline.Test.Helpers;
using NUnit.Framework;
using UnityEngine;

namespace Crener.Spline.Test._2D.Bezier.TestAdapters
{
    public class SharedBezierSplineTestBase : SelfCleanUpTestSet
    {
        protected BezierSpline2DJobTest.BezierSpline2DSimpleInspector CreateSpline()
        {
            GameObject game = new GameObject();
            BezierSpline2DJobTest.BezierSpline2DSimpleInspector bezierSpline = game.AddComponent<BezierSpline2DJobTest.BezierSpline2DSimpleInspector>();
            Assert.IsNotNull(bezierSpline);

            TestHelpers.ClearSpline(bezierSpline);
            
            m_disposables.Add(bezierSpline);
            return bezierSpline;
        }
    }
}