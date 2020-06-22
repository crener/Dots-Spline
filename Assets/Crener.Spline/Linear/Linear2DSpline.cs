using System.Runtime.CompilerServices;
using Crener.Spline.BaseSpline;
using Crener.Spline.Common;
using Unity.Entities;
using Unity.Mathematics;

namespace Crener.Spline.Linear
{
    /// <summary>
    /// Simple spline which directly follows a set of points
    /// </summary>
    public class Linear2DSpline : BaseSpline2D
    {
        public override SplineType SplineDataType => SplineType.Linear;
        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override float2 SplineInterpolation(float t, int a)
        {
            return math.lerp(Points[a], Points[(a + 1) % SegmentPointCount], t);
        }

        public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            return;

            //todo implement this for point to point
            /*dstManager.AddComponent<Spline2DData>(entity);
            Spline2DData splineData = ConvertData();
            SplineEntityData = splineData;
            dstManager.SetSharedComponentData(entity, splineData);*/
        }
    }
}