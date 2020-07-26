using Crener.Spline.BezierSpline;
using Crener.Spline.Test._3D.Bezier.TestAdapters;
using Crener.Spline.Test._3D.Bezier.TestTypes;
using Crener.Spline.Test.Helpers;
using NUnit.Framework;
using UnityEngine;

namespace Crener.Spline.Test._3D.Bezier
{
    /// <summary>
    /// Override for testing <see cref="BezierSpline2DSimple"/>
    /// </summary>
    public class BaseBezierSpline3DTests : BezierBaseTest3DAdapter
    {
        protected override ISimpleTestSpline3D CreateNewSpline()
        {
            GameObject game = new GameObject();
            MeaninglessTestWrapper.TestBezierSpline3DSimple testBezierSpline = game.AddComponent<MeaninglessTestWrapper.TestBezierSpline3DSimple>();
            Assert.IsNotNull(testBezierSpline);

            TestHelpers.ClearSpline(testBezierSpline);

            m_disposables.Add(testBezierSpline);
            return testBezierSpline;
        }
    }
}