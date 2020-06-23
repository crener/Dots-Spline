using System.Runtime.CompilerServices;
using Crener.Spline.BaseSpline;
using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Crener.Spline.CubicSpline
{
    /// <summary>
    /// Simple spline which directly follows a set of points
    /// </summary>
    public class CubicSpline2D : BaseSpline2D, ILoopingSpline
    {
        [SerializeField]
        private bool looped = false;
        [SerializeField]
        private int smoothing = 2;

        public bool Looped
        {
            get => looped && ControlPointCount > 2;
            set
            {
                looped = value;
                RecalculateLengthBias();
            }
        }
        public override SplineType SplineDataType => SplineType.Cubic;

        public override int SegmentPointCount => Looped ? ControlPointCount + 1 : ControlPointCount - 1;

        private const float c_splineMidPoint = 0.5f;

        public override float2 GetPoint(float progress)
        {
            if(ControlPointCount == 0)
                return float2.zero;
            else if(ControlPointCount == 1 || progress <= 0f)
                return GetControlPoint(0);
            else if(ControlPointCount == 2)
                return math.lerp(GetControlPoint(0), GetControlPoint(1), progress);
            else if(ControlPointCount == 3)
                return Cubic3Point(0, 1, 2, progress);
            else if(progress <= 0f)
                return GetControlPoint(0);
            else if(progress >= 1f)
                return GetControlPoint(ControlPointCount - 1);

            const int precesion = 1000;
            CubicSpline spline = new CubicSpline(Points.ToArray(), precesion, smoothing);
            return spline.Interpolated[(int) (precesion * progress)];

            //int aIndex = FindSegmentIndex(progress);
            //float pointProgress = SegmentProgress(progress, aIndex);
            //return SplineInterpolation(pointProgress, aIndex);
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
                LengthCache = LengthBetweenPoints(0, 128);
                SegmentLength.Add(1f);
                return;
            }

            // calculate the distance that the entire spline covers
            float currentLength = 0f;
            for (int a = 0; a < SegmentPointCount - 1; a++)
            {
                float length = LengthBetweenPoints(a, 128);
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
                float length = LengthBetweenPoints(a, 128);
                segmentCount = (length / LengthCache) + segmentCount;
                SegmentLength.Add(segmentCount);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override float2 SplineInterpolation(float t, int a)
        {
            float2 p0 = Points[a];
            float2 p1 = Points[(a + 1) % ControlPointCount];
            float2 p2 = Points[(a + 2) % ControlPointCount];

            float2 i0 = math.lerp(p0, p1, c_splineMidPoint);
            float2 i1 = math.lerp(p1, p2, c_splineMidPoint);

            float2 pp0 = math.lerp(i0, p1, t);
            float2 pp1 = math.lerp(p1, i1, t);

            return math.lerp(pp0, pp1, t);
        }

        private float2 Cubic3Point(int a, int b, int c, float t)
        {
            float2 p1 = Points[a];
            float2 p2 = Points[b];
            float2 p3 = Points[c];

            float2 i0 = math.lerp(p1, p2, t);
            float2 i1 = math.lerp(p2, p3, t);

            return math.lerp(i0, i1, t);
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