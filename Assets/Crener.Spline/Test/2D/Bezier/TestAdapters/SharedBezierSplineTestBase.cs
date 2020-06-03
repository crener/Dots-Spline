using Crener.Spline.Common.Interfaces;
using Crener.Spline.Test._2D.Bezier.TestTypes;
using Crener.Spline.Test.Helpers;
using NUnit.Framework;
using UnityEngine;

namespace Crener.Spline.Test._2D.Bezier.TestAdapters
{
    public class SharedBezierSplineTestBase : SelfCleanUpTestSet
    {
        protected ISimpleTestSpline CreateSpline()
        {
            GameObject game = new GameObject();
            ISimpleTestSpline bezierSpline = game.AddComponent<MeaninglessTestWrapper2.TestBezierSpline2DSimpleJob>();
            Assert.IsNotNull(bezierSpline);

            TestHelpers.ClearSpline(bezierSpline);
            
            m_disposables.Add(bezierSpline);
            return bezierSpline;
        }
    }
}