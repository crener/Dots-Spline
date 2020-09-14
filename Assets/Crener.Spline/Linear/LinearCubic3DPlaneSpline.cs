using System.Linq;
using System.Runtime.CompilerServices;
using Crener.Spline.BaseSpline;
using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Unity.Mathematics;
using UnityEngine;

namespace Crener.Spline.Linear
{
    /// <summary>
    /// Simple spline which directly follows a set of points
    /// </summary>
    [AddComponentMenu("Spline/3D/Linear Cubic Spline Plain")]
    public class LinearCubic3DPlaneSpline : BaseSpline3DPlane, ILoopingSpline, IArkableSpline
    {
        [SerializeField]
        private bool looped = false;
        [SerializeField, Tooltip("Ensures constant length between points in spline")]
        private bool arkParameterization = false;
        [SerializeField]
        private float arkLength = 0.1f;

        public bool Looped
        {
            get => looped;
            set
            {
                looped = value;
                RecalculateLengthBias();
            }
        }

        public bool ArkParameterization
        {
            get => arkParameterization;
            set
            {
                if(arkParameterization != value)
                {
                    arkParameterization = value;

                    ClearData();
                }
            }
        }

        public float ArkLength
        {
            get => arkLength;
            set
            {
                if(arkLength != value)
                {
                    arkLength = value;
                    ClearData();
                }
            }
        }

        public override SplineType SplineDataType
        {
            get
            {
                if(ControlPointCount == 0) return SplineType.Empty;
                if(ControlPointCount == 1) return SplineType.Single;
                if(ControlPointCount == 2 && !Looped) return SplineType.Linear;
                return ArkParameterization ? SplineType.Linear : SplineType.CubicLinear;
            }
        }

        public override int SegmentPointCount
        {
            get
            {
                if(ControlPointCount == 2) return 2 + (Looped ? 1 : 0);
                return ControlPointCount + (Looped ? 1 : -1);
            }
        }
        
        private const float c_splineMidPoint = 0.5f;
        
        public override float2 Get2DPoint(float progress)
        {
            if(ControlPointCount == 0)
                return float2.zero;
            else if(ControlPointCount == 1)
                return GetControlPoint2D(0);
            else if(ControlPointCount == 2 && !Looped)
                return math.lerp(GetControlPoint2D(0), GetControlPoint2D(1), math.clamp(progress, 0f, 1f));

            progress = math.clamp(progress, 0f, 1f);
            int aIndex = FindSegmentIndex(progress);
            float pointProgress = SegmentProgress(progress, aIndex);

            return SplineInterpolation(pointProgress, aIndex);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override float2 SplineInterpolation(float t, int a)
        {
            float2 p0 = Points[a];
            float2 p1 = Points[(a + 1) % ControlPointCount];
            float2 p2 = Points[(a + 2) % ControlPointCount];

            float2 i0, i1;
            if(looped)
            {
                i0 = math.lerp(p0, p1, c_splineMidPoint);
                i1 = math.lerp(p1, p2, c_splineMidPoint);
            }
            else
            {
                if(ControlPointCount > 3)
                {
                    if(a == 0)
                    {
                        i0 = math.lerp(p0, p1, t);
                        i1 = math.lerp(p1, p2, c_splineMidPoint);
                    }
                    else if(a == SegmentPointCount - 2)
                    {
                        i0 = math.lerp(p0, p1, c_splineMidPoint);
                        i1 = math.lerp(p1, p2, t);
                    }
                    else
                    {
                        i0 = math.lerp(p0, p1, c_splineMidPoint);
                        i1 = math.lerp(p1, p2, c_splineMidPoint);
                    }
                }
                else
                {
                    i0 = math.lerp(p0, p1, t);
                    i1 = math.lerp(p1, p2, t);
                }
            }

            float2 pp0 = math.lerp(i0, p1, t);
            float2 pp1 = math.lerp(p1, i1, t);

            return math.lerp(pp0, pp1, t);
        }
        
        protected override void RecalculateLengthBias()
        {
            ClearData();
            SegmentLength.Clear();

            if(ControlPointCount <= 1)
            {
                LengthCache = 0f;
                SegmentLength.Add(1f);
                return;
            }

            if(ControlPointCount == 2)
            {
                LengthCache = math.distance(Points[0], Points[1]);
                if(looped)
                {
                    LengthCache *= 2;
                    SegmentLength.Add(0.5f);
                }

                SegmentLength.Add(1f);
                return;
            }

            // fallback to known good code
            base.RecalculateLengthBias();
        }

        protected override float LengthBetweenPoints(int a, int resolution = 64)
        {
            if(ControlPointCount <= 1) return 0f;
            if(ControlPointCount == 2) return math.distance(GetControlPoint2D(0), GetControlPoint2D(1));

            return base.LengthBetweenPoints(a, resolution);
        }
    }
}