using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Crener.Spline.BaseSpline;
using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Unity.Mathematics;
using UnityEngine;

namespace Crener.Spline.Experimental
{
    /// <summary>
    /// Simple spline which directly follows a set of points
    /// </summary>
    public class BSpline2DExperimental : BaseSpline2D, ILoopingSpline
    {
        [SerializeField]
        private bool looped = false;
        private List<float2> m_knots = new List<float2>();

        public bool Looped
        {
            get => looped;
            set
            {
                looped = value;
                RecalculateLengthBias();
            }
        }
        public override SplineType SplineDataType => SplineType.BSpline;

        public override int SegmentPointCount => Looped ? ControlPointCount + 1 : ControlPointCount - 1;

        private const float c_splineMidPoint = 0.5f;

        public override float2 Get2DPoint(float progress)
        {
            if(ControlPointCount == 0)
                return float2.zero;
            else if(ControlPointCount == 1)
                return GetControlPoint(0);
            else if(ControlPointCount == 2)
                return math.lerp(GetControlPoint(0), GetControlPoint(1), progress);
            else if(ControlPointCount == 3)
                return Cubic3Point(0,1,2, progress);
            // else if(progress <= 0f)
            // {
            //     float2 a = GetControlPoint(0);
            //     float2 b = GetControlPoint(1 % ControlPointCount);
            //     return math.lerp(a, b, c_splineMidPoint);
            // }
            // else if(progress >= 1f)
            // {
            //     float2 a = GetControlPoint(ControlPointCount - 1);
            //     float2 b = GetControlPoint((ControlPointCount - 2) % ControlPointCount);
            //     return math.lerp(a, b, c_splineMidPoint);
            // }

            int aIndex = FindSegmentIndex(progress);
            float pointProgress = SegmentProgress(progress, aIndex);

            return SplineInterpolation(pointProgress, aIndex);
        }

        protected override void RecalculateLengthBias()
        {
            ClearData();
            CalculateKnots();
            SegmentLength.Clear();

            if(ControlPointCount <= 1)
            {
                LengthCache = 0f;
                SegmentLength.Add(1f);
                return;
            }

            if(ControlPointCount == 2)
            {
                LengthCache = LengthBetweenPoints(0, 128);
                SegmentLength.Add(1f);
                return;
            }

            // fallback to known good code
            base.RecalculateLengthBias();
        }

        protected float LengthOfPoints(int a, int resolution)
        {
            float currentLength = 1;

            /*float2 aPoint = SplineInterpolation(0f, a, b);
            for (float i = 1; i <= resolution; i++)
            {
                float2 bPoint = SplineInterpolation(i / resolution, a, b);
                currentLength += math.distance(aPoint, bPoint);
                aPoint = bPoint;
            }*/

            return currentLength;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override float2 SplineInterpolation(float t, int a)
        {
            float2 p1 = m_knots[math.max(a - 3, 0)];
            float2 p2 = m_knots[math.max(a - 2, 0)];
            float2 p3 = m_knots[math.max(a - 1, 0)];
            float2 p4 = m_knots[a];

            float2 i0 = math.lerp(p1, p2, t);
            float2 i1 = math.lerp(p2, p3, t);
            float2 i2 = math.lerp(p3, p4, t);

            float2 pp0 = math.lerp(i0, i1, t);
            float2 pp1 = math.lerp(i1, i2, t);

            return math.lerp(pp0, pp1, t);
        }

        private float2 BasisFunction(int i, int k)
        {
            return float2.zero;
        }

        private float2 Cubic3Point(int a, int b, int c, float t)
        {
            float2 p1 = m_knots[a];
            float2 p2 = m_knots[b];
            float2 p3 = m_knots[c];

            float2 i0 = math.lerp(p1, p2, t);
            float2 i1 = math.lerp(p2, p3, t);

            return math.lerp(i0, i1, t);
        }

        private void CalculateKnots()
        {
            m_knots.Clear();
            for (int i = 0; i < ControlPointCount; i++)
            {
                if(i == 0 || i == ControlPointCount-1)
                {
                    // first and last point get 4 points
                    for (int j = 0; j < 4; j++)
                        m_knots.Add(Points[i]);
                }
                else if(i == 1 || i == ControlPointCount - 2)
                    // first point after the first 4 are skipped, 5th to last point is skipped
                    continue;
                else m_knots.Add(Points[i]);
            }
        }
    }
}