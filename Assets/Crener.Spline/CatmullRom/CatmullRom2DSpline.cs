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

        public override int SegmentPointCount
        {
            get
            {
                if(ControlPointCount <= 3) return ControlPointCount;
                return ControlPointCount - 2;
            }
        }

        // 0.0 for the uniform spline, 0.5 for the centripetal spline, 1.0 for the chordal spline
        private const float c_alpha = 0.5f;

        public override float2 GetPoint(float progress)
        {
            if(ControlPointCount == 0)
                return float2.zero;
            else if(progress <= 0f)
                return GetControlPoint(ControlPointCount == 1 ? 0 : 1);
            else if(progress >= 1f)
                return GetControlPoint(ControlPointCount - 2);
            else if(ControlPointCount == 1 || progress <= 0f)
                return GetControlPoint(0);
            //else if(ControlPointCount == 2)
            //    return math.lerp(GetControlPoint(0), GetControlPoint(1), progress);
            //else if(ControlPointCount == 3)
            //    return Cubic3Point(0, 1, 2, progress);

            int aIndex = FindSegmentIndex(progress);
            float pointProgress = SegmentProgress(progress, aIndex);
            return SplineInterpolation(pointProgress, aIndex);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override float2 SplineInterpolation(float progress, int a)
        {
            if(progress == 0f) return Points[(a + 1) % ControlPointCount];
            if(progress == 1f) return Points[(a + 2) % ControlPointCount];
            
            float2 p0 = Points[a];
            float2 p1 = Points[(a + 1) % ControlPointCount];
            float2 p2 = Points[(a + 2) % ControlPointCount];
            float2 p3 = Points[(a + 3) % ControlPointCount];

            const float t0 = 0.0f;
            float start = GetT(t0, p0, p1);
            float end = GetT(start, p1, p2);
            float t3 = GetT(end, p2, p3);
            float t = start + ((end - start) * progress);

            float2 a1 = (start - t) / (start - t0) * p0 + (t - t0) / (start - t0) * p1;
            float2 a2 = (end - t) / (end - start) * p1 + (t - start) / (end - start) * p2;
            float2 a3 = (t3 - t) / (t3 - end) * p2 + (t - end) / (t3 - end) * p3;

            float2 b1 = (end - t) / (end - t0) * a1 + (t - t0) / (end - t0) * a2;
            float2 b2 = (t3 - t) / (t3 - start) * a2 + (t - start) / (t3 - start) * a3;

            return (end - t) / (end - start) * b1 + (t - start) / (end - start) * b2;
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