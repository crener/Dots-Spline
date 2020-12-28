using Crener.Spline.Common.Interfaces;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

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
            Debug.Log($"Testing '{expected:N3}' (Expected) against '{reality:N3}' (Reality)");
            CheckFloat2Internal(expected, reality, tolerance);
        }

        private static void CheckFloat2Internal(float2 expected, float2 reality, float tolerance = 0.00001f)
        {
            Assert.IsTrue(math.length(math.abs(expected.x - reality.x)) <= tolerance,
                $"X axis is out of range! (by {math.abs(expected.x - reality.x):N5})\n Expected: {expected.x:N3}, Received: {reality.x:N3} ({math.abs(expected.x - reality.x):N5} out of range, Tolerance: {tolerance:N5})");
            Assert.IsTrue(math.length(math.abs(expected.y - reality.y)) <= tolerance,
                $"Y axis is out of range! (by {math.abs(expected.y - reality.y):N5})\n Expected: {expected.y:N3}, Received: {reality.y:N3} ({math.abs(expected.y - reality.y):N5} out of range, Tolerance: {tolerance:N5})");
        }

        public static void CheckFloat3(float3 expected, float3 reality, float tolerance = 0.00001f)
        {
            Debug.Log($"Testing '{expected:N3}' (Expected) against '{reality:N3}' (Reality)");
            CheckFloat2Internal(expected.xy, reality.xy, tolerance);
            Assert.IsTrue(math.length(math.abs(expected.z - reality.z)) <= tolerance,
                $"Z axis is out of range! (by {math.abs(expected.z - reality.z):N5})\n Expected: {expected.z:N3}, Received: {reality.z:N3} ({math.abs(expected.z - reality.z):N5} out of range, Tolerance: {tolerance:N5})");
        }

        public static void CheckNotFloat2(float2 expected, float2 reality, float tolerance = 0.00001f)
        {
            Debug.Log($"Testing (Not) '{expected:N3}' (Expected) against '{reality:N3}' (Reality)");
            CheckNotFloat2Internal(expected, reality, tolerance);
        }

        private static void CheckNotFloat2Internal(float2 expected, float2 reality, float tolerance = 0.00001f)
        {
            Assert.IsTrue(math.length(math.abs(expected.x - reality.x)) >= tolerance,
                $"X axis is out of range!\n Expected: {expected.x:N3}, Received: {reality.x:N3} ({math.abs(expected.x - reality.x):N5} out of range, Tolerance: {tolerance:N5})");
            Assert.IsTrue(math.length(math.abs(expected.y - reality.y)) >= tolerance,
                $"Y axis is out of range!\n Expected: {expected.y:N3}, Received: {reality.y:N3} ({math.abs(expected.y - reality.y):N5} out of range, Tolerance: {tolerance:N5})");
        }

        public static void CheckNotFloat3(float3 expected, float3 reality, float tolerance = 0.00001f)
        {
            Debug.Log($"Testing (Not) '{expected:N3}' (Expected) against '{reality:N3}' (Reality)");
            CheckNotFloat2Internal(expected.xy, reality.xy, tolerance);
            Assert.IsTrue(math.length(math.abs(expected.z - reality.z)) >= tolerance,
                $"Z axis is out of range!\n Expected: {expected.z:N3}, Received: {reality.z:N3} ({math.abs(expected.z - reality.z):N5} out of range, Tolerance: {tolerance:N5})");
        }
    }
}