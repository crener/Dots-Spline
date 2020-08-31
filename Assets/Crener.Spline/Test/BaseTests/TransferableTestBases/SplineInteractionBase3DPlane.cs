using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Crener.Spline.Test.Helpers;
using NUnit.Framework;
using Unity.Mathematics;

namespace Crener.Spline.Test.BaseTests.TransferableTestBases
{
    public class SplineInteractionBase3DPlane : SplineInteractionBase3D
    {
        public override float3 GetControlPoint(ISimpleSpline3D spline, int index, SplinePoint pointType)
        {
            ISpline3DEditor spline3D = spline as ISpline3DEditor;
            Assert.NotNull(spline3D);
            
            return spline3D.GetControlPoint3D(index);
        }
        
        public override void CompareProgressEquals(ISimpleSpline3D spline, float progress, float3 expectedPoint, float tolerance = 0.00001f)
        {
            Assert.NotNull(spline);

            float3 point = spline.Get3DPoint(progress);
            TestHelpers.CheckFloat2(point.xy, expectedPoint.xy, tolerance);
        }

        public override void CompareProgress(ISimpleSpline3D spline, float progress, float3 unexpectedPoint)
        {
            ISpline3DPlane spline3D = spline as ISpline3DPlane;
            Assert.NotNull(spline3D);
            
            float2 point2d = spline3D.Get2DPoint(progress);
            TestHelpers.CheckFloat2(point2d, unexpectedPoint.xy);
        }

        public override void ComparePoint(float3 expected, float3 actual, float tolerance = 0.00001f)
        {
            TestHelpers.CheckFloat2(expected.xy, actual.xy, tolerance);
        }
    }
}