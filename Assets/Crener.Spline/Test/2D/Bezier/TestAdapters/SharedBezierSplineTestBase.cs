using Crener.Spline.Test._2D.Bezier.TestTypes;
using Crener.Spline.Test.Helpers;
using NUnit.Framework;
using UnityEngine;

namespace Crener.Spline.Test._2D.Bezier.TestAdapters
{
    public class SharedBezierSplineTestBase : SelfCleanUpTestSet
    {
        protected ISimpleTestSpline2D CreateSpline()
        {
            GameObject game = new GameObject();
            ISimpleTestSpline2D bezierSpline2D = game.AddComponent<MeaninglessTestWrapper2.TestBezierSpline2D2DSimpleJob>();
            Assert.IsNotNull(bezierSpline2D);

            TestHelpers.ClearSpline(bezierSpline2D);
            
            m_disposables.Add(bezierSpline2D);
            return bezierSpline2D;
        }
    }
}