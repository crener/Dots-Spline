using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Crener.Spline.BaseSpline;
using Crener.Spline.BezierSpline;
using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace Crener.Spline.Linear
{
    /// <summary>
    /// Simple spline which directly follows a set of points
    /// </summary>
    [AddComponentMenu("Spline/2D/Linear Cubic Spline")]
    public class LinearCubicSpline2D : BaseSpline2D, ILoopingSpline
    {
        [SerializeField]
        private bool looped = false;

        public bool Looped
        {
            get => looped;
            set
            {
                looped = value;
                RecalculateLengthBias();
            }
        }
        
        public override SplineType SplineDataType
        {
            get
            {
                if(ControlPointCount == 0) return SplineType.Empty;
                if(ControlPointCount == 1) return SplineType.Single;
                if(ControlPointCount == 2) return SplineType.Linear;
                return SplineType.CubicLinear;
            }
        }
        public override int SegmentPointCount => Looped ? ControlPointCount + 1 : ControlPointCount -1 ;

        private const float c_splineMidPoint = 0.5f;

        public override float2 GetPoint(float progress)
        {
            if(ControlPointCount == 0)
                return float2.zero;
            else if(ControlPointCount == 1)
                return GetControlPoint(0);
            else if(ControlPointCount == 2)
                return math.lerp(GetControlPoint(0), GetControlPoint(1), math.clamp(progress, 0f, 1f));
            else if(progress <= 0f) progress = 0f;
            else if(progress > 1f) progress = 1f;

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
                SegmentLength.Add(1f);
                return;
            }

            // fallback to known good code
            base.RecalculateLengthBias();
        }

        protected override float LengthBetweenPoints(int a, int resolution = 64)
        {
            if(ControlPointCount <= 1) return 0f;
            if(ControlPointCount == 2) return math.distance(GetControlPoint(0), GetControlPoint(1));

            return base.LengthBetweenPoints(a, resolution);
        }

        public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            return;

            //todo implement this for b-spline
            /*dstManager.AddComponent<Spline2DData>(entity);
            Spline2DData splineData = ConvertData();
            SplineEntityData = splineData;
            dstManager.SetSharedComponentData(entity, splineData);*/
        }
    }
}