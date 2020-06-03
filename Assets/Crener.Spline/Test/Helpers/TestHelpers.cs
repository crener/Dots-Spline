using Crener.Spline.Common.Interfaces;
using NUnit.Framework;
using Unity.Mathematics;

namespace Crener.Spline.Test.Helpers
{
    public static class TestHelpers
    {
        public static void ClearSpline(ISpline spline)
        {
            while (spline.ControlPointCount > 0)
            {
                spline.RemoveControlPoint(0);
            }

            Assert.AreEqual(0f, spline.Length());
            Assert.AreEqual(0, spline.ControlPointCount);
        }

        public static void CheckFloat2(float2 expected, float2 reality, float tolerance = 0.00001f)
        {
            Assert.IsTrue(math.length(math.abs(expected.x - reality.x)) <= tolerance,
                $"X axis is out of range!\n Expected: {expected.x}\n Received: {reality.x}\n Tolerance: {tolerance:N3}");
            Assert.IsTrue(math.length(math.abs(expected.y - reality.y)) <= tolerance,
                $"Y axis is out of range!\n Expected: {expected.x}\n Received: {reality.x}\n Tolerance: {tolerance:N3}");
        }
    }
}