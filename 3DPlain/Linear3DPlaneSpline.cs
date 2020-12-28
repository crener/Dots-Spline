using System.Linq;
using System.Runtime.CompilerServices;
using Crener.Spline.BaseSpline;
using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Unity.Mathematics;
using UnityEngine;

namespace Crener.Spline._3DPlain
{
    /// <summary>
    /// Simple spline which directly follows a set of points
    /// </summary>
    [AddComponentMenu("Spline/3D/Linear Spline 3D Plain")]
    public class Linear3DPlaneSpline : BaseSpline3DPlane, ILoopingSpline
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

        protected override float LengthBetweenPoints(int a, int resolution = LengthSampleCount)
        {
            float2 start = Points[a % ControlPointCount];
            float2 end = Points[(a + 1) % ControlPointCount];
            return math.distance(start, end);
        }

        protected override void DrawLineGizmos()
        {
            Gizmos.color = Color.gray;
            float3[] points3D = Points.Select(p => Convert2Dto3D(p)).ToArray();

            for (int i = 1; i < SegmentPointCount; i++)
            {
                int p1 = (i -1) % points3D.Length;
                int p2 = i % points3D.Length;

                Gizmos.DrawLine(points3D[p1], points3D[p2]);
            }
        }
    }
}