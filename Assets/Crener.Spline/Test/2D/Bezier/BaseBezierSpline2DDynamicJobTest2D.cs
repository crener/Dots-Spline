using Crener.Spline._2D.Jobs;
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
    public class BaseBezierSpline2DDynamicJobTest2D : BezierBaseTest2DAdapter
    {
        protected override ISimpleTestSpline2D CreateNewSpline()
        {
            GameObject game = new GameObject();
            MeaninglessTestWrapper3.TestBezierSpline2D2DDynamicJob testBezierSpline = game.AddComponent<MeaninglessTestWrapper3.TestBezierSpline2D2DDynamicJob>();
            Assert.IsNotNull(testBezierSpline);

            TestHelpers.ClearSpline(testBezierSpline);

            m_disposables.Add(testBezierSpline);
            return testBezierSpline;
        }
    }
}