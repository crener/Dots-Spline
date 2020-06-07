using System.Runtime.CompilerServices;
using Crener.Spline.BaseSpline;
using Crener.Spline.Common;
using Crener.Spline.Common.DataStructs;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Assertions;

namespace Crener.Spline.PointToPoint
{
    /// <summary>
    /// Simple spline which directly follows a set of points
    /// </summary>
    public class PointToPoint2DSpline : BaseSpline2D
    {
        public override SplineType SplineDataType => SplineType.PointToPoint;
        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override float2 SplineInterpolation(float t, int a, int b)
        {
            return math.lerp(Points[a], Points[b], t);
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