using System.Runtime.CompilerServices;
using Crener.Spline.BaseSpline;
using Crener.Spline.Common;
using Crener.Spline.Common.DataStructs;
using Unity.Mathematics;

namespace Crener.Spline.Linear
{
    /// <summary>
    /// Simple spline which directly follows a set of points
    /// </summary>
    public class Linear3DSpline : BaseSpline3D
    {
        public override SplineType SplineDataType
        {
            get
            {
                if(ControlPointCount == 0) return SplineType.Empty;
                if(ControlPointCount == 1) return SplineType.Single;
                return SplineType.Linear;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override float3 SplineInterpolation(float t, int a)
        {
            return math.lerp(Points[a], Points[(a + 1) % SegmentPointCount], t);
        }

        protected override float LengthBetweenPoints(int a, int resolution = 64)
        {
            return math.distance(
                GetControlPoint(a),
                GetControlPoint((a + 1) % SegmentPointCount));
        }
    }
}