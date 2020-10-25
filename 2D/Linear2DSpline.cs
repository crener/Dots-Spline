using System.Runtime.CompilerServices;
using Crener.Spline.BaseSpline;
using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Unity.Mathematics;
using UnityEngine;

namespace Crener.Spline._2D
{
    /// <summary>
    /// Simple spline which directly follows a set of points
    /// </summary>
    [AddComponentMenu("Spline/2D/Linear Spline 2D")]
    public class Linear2DSpline : BaseSpline2D, ILoopingSpline
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
                return SplineType.Linear;
            }
        }
        public override int SegmentPointCount => ControlPointCount + (Looped ? 1 : 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override float2 SplineInterpolation(float t, int a)
        {
            float2 start = Points[a % ControlPointCount];
            float2 end = Points[(a + 1) % ControlPointCount];
            return math.lerp(start, end, t);
        }

        protected override float LengthBetweenPoints(int a, int resolution = 64)
        {
            float2 start = Points[a % ControlPointCount];
            float2 end = Points[(a + 1) % ControlPointCount];
            return math.distance(start, end);
        }
    }
}