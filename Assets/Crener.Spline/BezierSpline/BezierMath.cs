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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 CubicBezierPoint(float2 t, float2x4 a, float2x4 b, half variance)
        {
            float2 oneMinusT = 1f - t;
            return math.lerp(
                (oneMinusT.x * oneMinusT.x * oneMinusT.x * a.c0) +
                (3f * oneMinusT.x * oneMinusT.x * t.x * a.c1) +
                (3f * oneMinusT.x * t.x * t.x * a.c2) +
                (t.x * t.x * t.x * a.c3),
                (oneMinusT.y * oneMinusT.y * oneMinusT.y * b.c0) +
                (3f * oneMinusT.y * oneMinusT.y * t.y * b.c1) +
                (3f * oneMinusT.y * t.y * t.y * b.c2) +
                (t.y * t.y * t.y * b.c3), math.abs(variance));
        }
    }
}