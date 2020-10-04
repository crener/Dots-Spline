using System.Runtime.CompilerServices;
using Crener.Spline.BaseSpline;
using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Unity.Assertions;
using Unity.Mathematics;
using UnityEngine;

namespace Crener.Spline.CatmullRom
{
    /// <summary>
    /// Centripetal Catmull-rom spline based on https://en.wikipedia.org/wiki/Centripetal_Catmull%E2%80%93Rom_spline implementation 
    /// </summary>
    [AddComponentMenu("Spline/2D/Cutmull Spline")]
    public class CatmullRom2DSpline : BaseSpline2D, ILoopingSpline
    {
        [SerializeField]
        private bool looped = false;

        public override SplineType SplineDataType
        {
            get
            {
                if(ControlPointCount == 0) return SplineType.Empty;
                if(ControlPointCount == 1) return SplineType.Single;
                if(ControlPointCount == 2) return SplineType.Linear;
                //if(ControlPointCount <= 3) return SplineType.CubicLinear;
                return SplineType.CatmullRom;
            }
        }

        public override int SegmentPointCount
        {
            get
            {
                if(ControlPointCount < 3) return ControlPointCount + (looped ? 1 : 0);
                if(ControlPointCount == 3) return (looped ? 4 : 3);
                return ControlPointCount + (looped ? 1 : 0);
            }
        }

        public bool Looped
        {
            get => looped;
            set
            {
                looped = value;
                RecalculateLengthBias();
            }
        }

        // 0.0 for the uniform spline, 0.5 for the centripetal spline, 1.0 for the chordal spline
        private const float c_alpha = 0.5f;

        public override float2 Get2DPoint(float progress)
        {
            if(ControlPointCount == 0)
                return float2.zero;
            else if(progress <= 0f)
                return ConvertToWorldSpace(GetControlPoint2DLocal(0));
            else if(progress >= 1f)
                return ConvertToWorldSpace(GetControlPoint2DLocal(math.max(0, ControlPointCount - 1)));
            else if(ControlPointCount == 1)
                return ConvertToWorldSpace(GetControlPoint2DLocal(0));

            int aIndex = FindSegmentIndex(progress);
            float pointProgress = SegmentProgress(progress, aIndex);
            return ConvertToWorldSpace(SplineInterpolation(pointProgress, aIndex));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override float2 SplineInterpolation(float progress, int a)
        {
            float2 p0, p1;
            if(ControlPointCount == 2)
            {
                p0 = Points[a];
                p1 = Points[(a + 1) % ControlPointCount];
                return math.lerp(p0, p1, progress);
            }

            float2 p2, p3;
            if(looped)
            {
                // looped
                p0 = Points[a];
                p1 = Points[(a + 1) % ControlPointCount];
                p2 = Points[(a + 2) % ControlPointCount];
                p3 = Points[(a + 3) % ControlPointCount];
            }
            else
            {
                // not looped
                if(ControlPointCount == 3)
                {
                    // 3 points require 2 of the points at the start and end to be fabricated
                    if(progress == 0f) return Points[a];
                    if(progress == 1f) return Points[(a + 1) % ControlPointCount];

                    p1 = Points[a];
                    p2 = Points[(a + 1) % ControlPointCount];

                    float2 delta = p2 - p1;
                    float angle = math.atan2(delta.y, delta.x) - (math.PI / 2);

                    if(a == 0)
                    {
                        // need to create a fake point for p0
                        p0 = new float2(p1.x + math.sin(angle), p1.y - math.cos(angle));
                        p3 = Points[2];
                    }
                    else
                    {
                        Assert.AreEqual(1, a);
                        // need to create a fake point for p3

                        p0 = Points[(a - 1) % ControlPointCount];
                        p3 = new float2(p2.x + math.sin(-angle), p2.y + math.cos(-angle));
                    }
                }
                else
                {
                    if(a == 0)
                    {
                        p1 = Points[a];
                        p2 = Points[1 % ControlPointCount];
                        p3 = Points[2 % ControlPointCount];

                        float2 delta = p2 - p1;
                        float angle = math.atan2(delta.y, delta.x) - (math.PI / 2);
                        float size = math.max(math.length(delta) * 0.5f, float.Epsilon);
                        p0 = new float2(p1.x + (math.sin(angle) * size), p1.y - (math.cos(angle) * size));
                    }
                    else if(a == ControlPointCount - 2)
                    {
                        p1 = Points[a];
                        if(progress <= 0f) return p1;

                        p2 = Points[(a + 1) % ControlPointCount];
                        if(progress >= 1f) return p2;

                        p0 = Points[(a - 1) % ControlPointCount];

                        float2 delta = p2 - p1;
                        float angle = math.atan2(delta.y, delta.x) - (math.PI / 2);
                        float size = math.distance(delta.x, delta.y) * 0.5f;
                        p3 = new float2(p2.x + (math.sin(-angle) * size), p2.y + (math.cos(-angle) * size));
                    }
                    else
                    {
                        p0 = Points[(a - 1) % ControlPointCount];
                        p1 = Points[a];
                        p2 = Points[(a + 1) % ControlPointCount];
                        p3 = Points[(a + 2) % ControlPointCount];
                    }
                }
            }

            const float t0 = 0.0f;
            float start = GetT(t0, p0, p1);
            float end = GetT(start, p1, p2);
            float t3 = GetT(end, p2, p3);
            float t = start + ((end - start) * progress);

            float2 a1 = (start - t) / (start - t0) * p0 + (t - t0) / (start - t0) * p1;
            float2 a2 = (end - t) / (end - start) * p1 + (t - start) / (end - start) * p2;
            float2 a3 = (t3 - t) / (t3 - end) * p2 + (t - end) / (t3 - end) * p3;
            if(float.IsNaN(a3.x)) a3 = float2.zero;

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