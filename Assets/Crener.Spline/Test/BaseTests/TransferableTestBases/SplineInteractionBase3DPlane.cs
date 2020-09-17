using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Crener.Spline.Test.Helpers;
using NUnit.Framework;
using Unity.Mathematics;

namespace Crener.Spline.Test.BaseTests.TransferableTestBases
{
    public class SplineInteractionBase3DPlane : ITransferableTestSet<ISpline3DPlane>
    {
        public void AddControlPoint(ISpline3DPlane spline, float3 point)
        {
            Assert.NotNull(spline);

            int before = spline.ControlPointCount;
            spline.AddControlPoint(point);

            Assert.Greater(spline.ControlPointCount, before, "Adding a point did not increase the control point count");
        }

        public void InsertControlPoint(ISpline3DPlane spline, int index, float3 point)
        {
            ISpline3DEditor spline3D = spline as ISpline3DEditor;
            Assert.NotNull(spline3D);

            spline3D.InsertControlPoint(index, point);
        }
        
        public float3 GetControlPoint(ISpline3DPlane spline, int index, SplinePoint pointType)
        {
            ISpline3DEditor spline3D = spline as ISpline3DEditor;
            Assert.NotNull(spline3D);
            
            return spline3D.GetControlPoint3D(index);
        }

        public void UpdateControlPoint(ISpline3DPlane spline, int index, float3 newPoint, SplinePoint pointType)
        {
            ISpline3DEditor spline3D = spline as ISpline3DEditor;
            Assert.NotNull(spline3D);

            spline3D.UpdateControlPoint(index, newPoint, pointType);
        }

        public float Length(float3 a, float3 b) => math.distance(a, b);

        public virtual float3 GetProgress(ISpline3DPlane spline, float progress)
        {
            Assert.NotNull(spline);
            return spline.Get3DPoint(progress);
        }
        
        public void CompareProgressEquals(ISpline3DPlane spline, float progress, float3 expectedPoint, float tolerance = 0.00001f)
        {
            Assert.NotNull(spline);

            float3 point = spline.Get3DPoint(progress);
            TestHelpers.CheckFloat2(point.xy, expectedPoint.xy, tolerance);
        }

        public void CompareProgressNotEquals(ISpline3DPlane spline, float progress, float3 unexpectedPoint)
        {
            Assert.NotNull(spline);
            
            float2 point2d = spline.Get2DPoint(progress);
            TestHelpers.CheckFloat2(point2d, unexpectedPoint.xy);
        }

        public void ComparePoint(float3 expected, float3 actual, float tolerance = 0.00001f)
        {
            TestHelpers.CheckFloat2(expected.xy, actual.xy, tolerance);
        }
    }
}