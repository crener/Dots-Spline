using Crener.Spline.Common.Interfaces;
using NUnit.Framework;

namespace Crener.Spline.Test.Helpers
{
    public static class TestHelpers
    {
        public static void ClearSpline(ISpline2D bezierSpline)
        {
            while (bezierSpline.ControlPointCount > 0)
            {
                bezierSpline.RemoveControlPoint(0);
            }

            Assert.AreEqual(0f, bezierSpline.Length());
            Assert.AreEqual(0, bezierSpline.ControlPointCount);
        }
        
        public static void ClearSpline(ISpline3D bezierSpline)
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