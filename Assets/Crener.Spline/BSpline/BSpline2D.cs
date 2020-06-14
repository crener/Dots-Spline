using System.Runtime.CompilerServices;
using Crener.Spline.BaseSpline;
using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Crener.Spline.BSpline
{
    /// <summary>
    /// Simple spline which directly follows a set of points
    /// </summary>
    public class BSpline2D : BaseSpline2D, ILoopingSpline
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
        public override SplineType SplineDataType => SplineType.BSpline;

        public override int SegmentPointCount => Looped ? ControlPointCount + 1 : ControlPointCount - 1;

        private const float c_splineMidPoint = 0.5f;
        
        public override float2 GetPoint(float progress)
        {
            if(ControlPointCount == 0)
                return float2.zero;
            else if(ControlPointCount == 1)
                return GetControlPoint(0);
            else if(ControlPointCount == 2)
                return math.lerp(GetControlPoint(0), GetControlPoint(1), progress);
            else if(progress <= 0f)
            {
                float2 a = GetControlPoint(0);
                float2 b = GetControlPoint(1 % ControlPointCount);
                return math.lerp(a, b, c_splineMidPoint);
            }
            else if(ControlPointCount == 1)
                return GetControlPoint(0);
            else if(progress >= 1f)
            {
                float2 a = GetControlPoint(ControlPointCount - 1);
                float2 b = GetControlPoint((ControlPointCount - 2) % ControlPointCount);
                return math.lerp(a, b, c_splineMidPoint);
            }

            int aIndex = FindSegmentIndex(progress);
            float pointProgress = SegmentProgress(progress, aIndex);

            int bIndex = (aIndex + 1) % SegmentPointCount;
            return SplineInterpolation(pointProgress, aIndex, bIndex);
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
                LengthCache = LengthBetweenPoints(0, 1, 128);;
                SegmentLength.Add(1f);
                return;
            }

            // calculate the distance that the entire spline covers
            float currentLength = 0f;
            for (int a = 0; a < SegmentPointCount - 1; a++)
            {
                int b = (a + 1) % SegmentPointCount;
                float length = LengthBetweenPoints(a, b, 128);

                currentLength += length;
            }

            LengthCache = currentLength;

            if(SegmentPointCount == 2)
            {
                SegmentLength.Add(1f);
                return;
            }

            // calculate the distance that a single segment covers
            float segmentCount = 0f;
            for (int a = 0; a < SegmentPointCount - 1; a++)
            {
                int b = (a + 1) % SegmentPointCount;
                float length = LengthBetweenPoints(a, b);

                segmentCount = (length / LengthCache) + segmentCount;
                SegmentLength.Add(segmentCount);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override float2 SplineInterpolation(float t, int a, int b)
        {
            float2 p0 = Points[a];
            float2 p1 = Points[b % ControlPointCount];
            float2 p2 = Points[(b + 1) % ControlPointCount];

            float2 i0 = math.lerp(p0, p1, c_splineMidPoint);
            float2 i1 = math.lerp(p1, p2, c_splineMidPoint);

            float2 pp0 = math.lerp(i0, p1, t);
            float2 pp1 = math.lerp(p1, i1, t);

            return math.lerp(pp0, pp1, t);
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