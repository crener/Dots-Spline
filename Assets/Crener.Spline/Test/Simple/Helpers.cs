using Crener.Spline.BezierSpline;
using NUnit.Framework;
using UnityEngine;

namespace Crener.Spline.Test.Simple
{
    public static class BezierSplineHelpers
    {
        public static BezierSpline2DJobTest.BezierSpline2DSimpleInspector CreateSpline()
        {
            GameObject game = new GameObject();
            BezierSpline2DJobTest.BezierSpline2DSimpleInspector bezierSpline = game.AddComponent<BezierSpline2DJobTest.BezierSpline2DSimpleInspector>();
            Assert.IsNotNull(bezierSpline);

            ClearSpline(bezierSpline);

            return bezierSpline;
        }
        
        private static void ClearSpline(BezierSpline2DSimple bezierSpline)
        {
            while (bezierSpline.ControlPointCount > 0)
            {
                bezierSpline.RemoveControlPoint(0);
            }

            Assert.AreEqual(0f, bezierSpline.Length());
            Assert.AreEqual(0, bezierSpline.ControlPointCount);
        }
    }
}