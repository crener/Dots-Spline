using Crener.Spline.BezierSpline;
using Crener.Spline.Common.Interfaces;
using Crener.Spline.Test._3D.Bezier.TestAdapters;
using Crener.Spline.Test._3D.Bezier.TestTypes;
using Crener.Spline.Test.BaseTests;
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
    
    public class ArkBezierSplineTests3D : BaseArkTests3D
    {
        protected override IArkableSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            MeaninglessTestWrapper.TestBezierSpline3DSimple testBezierSpline = game.AddComponent<MeaninglessTestWrapper.TestBezierSpline3DSimple>();

            return testBezierSpline;
        }
    }
}