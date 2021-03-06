using System.Runtime.CompilerServices;
using Crener.Spline.BaseSpline;
using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Unity.Mathematics;
using UnityEngine;

namespace Crener.Spline._3D
{
    /// <summary>
    /// Simple spline which directly follows a set of points
    /// </summary>
    [AddComponentMenu("Spline/3D/Linear Cubic Spline 3D")]
    public class LinearCubic3DSpline : BaseSpline3D, ILoopingSpline, IArkableSpline
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
                if(ArkParameterization) return SplineType.Linear;
                if(ControlPointCount == 2 && !Looped) return SplineType.Linear;
                return SplineType.CubicLinear;
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

        public override float3 Get3DPointLocal(float progress)
        {
            float3 translation = Position;
            if(ControlPointCount == 0)
                return translation;
            else if(ControlPointCount == 1)
                return GetControlPoint3DLocal(0);
            else if(ControlPointCount == 2 && !Looped)
            {
                return math.lerp(GetControlPoint3DLocal(0), GetControlPoint3DLocal(1), math.clamp(progress, 0f, 1f));
            }

            progress = math.clamp(progress, 0f, 1f);
            int aIndex = FindSegmentIndex(progress);
            float pointProgress = SegmentProgress(progress, aIndex);

            return SplineInterpolation(pointProgress, aIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override float3 SplineInterpolation(float t, int a)
        {
            float3 p0 = Points[a];
            float3 p1 = Points[(a + 1) % ControlPointCount];
            
            if(ControlPointCount == 2) return math.lerp(p0, p1, t);
            
            float3 p2 = Points[(a + 2) % ControlPointCount];

            float3 i0, i1;
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

            float3 pp0 = math.lerp(i0, p1, t);
            float3 pp1 = math.lerp(p1, i1, t);

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

        protected override float LengthBetweenPoints(int a, int resolution = LengthSampleCount)
        {
            if(ControlPointCount <= 1) return 0f;
            if(ControlPointCount == 2) return math.distance(GetControlPoint3DLocal(0), GetControlPoint3DLocal(1));

            return base.LengthBetweenPoints(a, resolution);
        }
    }
}