using Crener.Spline.BezierSpline.Jobs;
using Crener.Spline.Test._2D.Bezier.TestAdapters;
using Crener.Spline.Test._2D.Bezier.TestTypes;
using Crener.Spline.Test.Helpers;
using NUnit.Framework;
using UnityEngine;

namespace Crener.Spline.Test._2D.Bezier
{
    /// <summary>
    /// Override for testing <see cref="BezierSpline2DPointJob"/>
    /// </summary>
    public class BaseBezierSpline2DJobTest2D : BezierBaseTest2DAdapter
    {
        protected override ISimpleTestSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            MeaninglessTestWrapper2.TestBezierSpline2DSimpleJob testBezierSpline = game.AddComponent<MeaninglessTestWrapper2.TestBezierSpline2DSimpleJob>();
            Assert.IsNotNull(testBezierSpline);

            TestHelpers.ClearSpline(testBezierSpline);

            m_disposables.Add(testBezierSpline);
            return testBezierSpline;
        }
    }
}