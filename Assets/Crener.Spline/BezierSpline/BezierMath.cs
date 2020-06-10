using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace Crener.Spline.BezierSpline
{
    /// <summary>
    /// Math functions for calculating bezier functions
    /// </summary>
    [BurstCompile]
    public static class BezierMath
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 CubicBezierPoint(float t, float2 aPoint, float2 aDir, float2 bDir, float2 bPoint)
        {
            t = math.clamp(t, 0f, 1f);
            float oneMinusT = 1f - t;
            return
                (oneMinusT * oneMinusT * oneMinusT * aPoint) +
                (3f * oneMinusT * oneMinusT * t * aDir) +
                (3f * oneMinusT * t * t * bDir) +
                (t * t * t * bPoint);
        }
    }
}