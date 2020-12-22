using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Crener.Spline.Test.Helpers;
using NUnit.Framework;
using Unity.Mathematics;

namespace Crener.Spline.Test.BaseTests.TransferableTestBases
{
    public class SplineInteractionBase3D : ITransferableTestSet<ISimpleSpline3D>
    {
        public void AddControlPointLocalSpace(ISimpleSpline3D spline, float3 point)
        {
            Assert.NotNull(spline);

            int before = spline.ControlPointCount;
            spline.AddControlPoint(point);

            Assert.Greater(spline.ControlPointCount, before, "Adding a point did not increase the control point count");
        }

        public void InsertControlPointWorldSpace(ISimpleSpline3D spline, int index, float3 point)
        {
            ISpline3DEditor spline3D = spline as ISpline3DEditor;
            Assert.NotNull(spline3D);

            spline3D.InsertControlPointWorldSpace(index, point);
        }

        public void InsertControlPointLocalSpace(ISimpleSpline3D spline, int index, float3 point)
        {
            ISpline3DEditor spline3D = spline as ISpline3DEditor;
            Assert.NotNull(spline3D);

            spline3D.InsertControlPointLocalSpace(index, point);
        }

        public float3 GetControlPoint(ISimpleSpline3D spline, int index, SplinePoint pointType)
        {
            Assert.NotNull(spline);

            return spline.GetControlPoint(index, pointType);
        }

        public void UpdateControlPoint(ISimpleSpline3D spline, int index, float3 newPoint, SplinePoint pointType)
        {
            ISpline3DEditor spline3D = spline as ISpline3DEditor;
            Assert.NotNull(spline3D);

            spline3D.UpdateControlPointWorld(index, newPoint, pointType);
        }

        public float Length(float3 a, float3 b) => math.distance(a, b);

        public float3 GetProgressWorld(ISimpleSpline3D spline, float progress)
        {
            Assert.NotNull(spline);
            return spline.Get3DPointWorld(progress);
        }
        
        public virtual float3 GetProgressLocal(ISimpleSpline3D spline, float progress)
        {
            Assert.NotNull(spline);
            return spline.Get3DPointLocal(progress);
        }

        public void CompareProgressEquals(ISimpleSpline3D spline, float progress, float3 expectedPoint, float tolerance = 0.00001f)
        {
            Assert.NotNull(spline);

            float3 point = spline.Get3DPointWorld(progress);
            TestHelpers.CheckFloat3(expectedPoint, point, tolerance);
        }

        public void CompareProgressNotEquals(ISimpleSpline3D spline, float progress, float3 expectedPoint)
        {
            Assert.NotNull(spline);

            float3 point = spline.Get3DPointWorld(progress);
            TestHelpers.CheckNotFloat3(expectedPoint, point);
        }

        public void ComparePoint(float3 expected, float3 actual, float tolerance = 0.00001f)
        {
            TestHelpers.CheckFloat3(expected, actual, tolerance);
        }
    }
}