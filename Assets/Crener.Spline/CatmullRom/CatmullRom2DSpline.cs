using System.Runtime.CompilerServices;
using Crener.Spline.BaseSpline;
using Crener.Spline.Common;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Crener.Spline.CatmullRom
{
    /// <summary>
    /// Centripetal Catmull-rom spline based on https://en.wikipedia.org/wiki/Centripetal_Catmull%E2%80%93Rom_spline implementation 
    /// </summary>
    public class CatmullRom2DSpline : BaseSpline2D
    {
        public override SplineType SplineDataType => SplineType.CatmullRom;

        // 0.0 for the uniform spline, 0.5 for the centripetal spline, 1.0 for the chordal spline
        private const float c_alpha = 0.5f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override float2 SplineInterpolation(float t, int a)
        {
            // todo get points based on actual a and b values
            float2 p0 = Points[0];
            float2 p1 = Points[1];
            float2 p2 = Points[2];
            float2 p3 = Points[3];

            float t0 = 0.0f;
            float t1 = GetT(t0, p0, p1);
            float t2 = GetT(t1, p1, p2);
            float t3 = GetT(t2, p2, p3);

            float2 a1 = (t1 - t) / (t1 - t0) * p0 + (t - t0) / (t1 - t0) * p1;
            float2 a2 = (t2 - t) / (t2 - t1) * p1 + (t - t1) / (t2 - t1) * p2;
            float2 a3 = (t3 - t) / (t3 - t2) * p2 + (t - t2) / (t3 - t2) * p3;

            float2 b1 = (t2 - t) / (t2 - t0) * a1 + (t - t0) / (t2 - t0) * a2;
            float2 b2 = (t3 - t) / (t3 - t1) * a2 + (t - t1) / (t3 - t1) * a3;

            return (t2 - t) / (t2 - t1) * b1 + (t - t1) / (t2 - t1) * b2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        float GetT(float t, float2 p0, float2 p1)
        {
            float a = math.pow((p1.x - p0.x), 2.0f) + math.pow((p1.y - p0.y), 2.0f);
            float b = math.pow(a, c_alpha * 0.5f);

            return (b + t);
        }
    }
}