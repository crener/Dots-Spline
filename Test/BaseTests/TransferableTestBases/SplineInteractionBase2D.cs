using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Crener.Spline.Test.Helpers;
using NUnit.Framework;
using Unity.Mathematics;

namespace Crener.Spline.Test.BaseTests.TransferableTestBases
{
    public class SplineInteractionBase2D : ITransferableTestSet<ISpline2D>
    {
        public void AddControlPointLocalSpace(ISpline2D spline, float3 point)
        {
            Assert.NotNull(spline);

            ISpline2D spline2D = spline as ISpline2D;
            Assert.NotNull(spline2D, $"Failed to convert to 2D spline! Spline type was: {spline.GetType().Name}");

            int before = spline2D.ControlPointCount;
            spline2D.AddControlPoint(point.xy);

            Assert.Greater(spline2D.ControlPointCount, before, "Adding a point did not increase the control point count");
        }

        public void InsertControlPointWorldSpace(ISpline2D spline, int index, float3 point)
        {
            ISpline2DEditor spline2D = spline as ISpline2DEditor;
            Assert.NotNull(spline2D);
            
            spline2D.InsertControlPoint(index, point.xy);
        }

        public float3 GetControlPoint(ISpline2D spline, int index, SplinePoint pointType)
        {
            ISpline2DEditor spline2D = spline as ISpline2DEditor;
            Assert.NotNull(spline2D);
            
            return new float3(spline2D.GetControlPoint2DLocal(index), 0f);
        }

        public void UpdateControlPoint(ISpline2D spline, int index, float3 newPoint, SplinePoint pointType)
        {
            ISpline2DEditor spline2D = spline as ISpline2DEditor;
            Assert.NotNull(spline2D);
            
            spline2D.UpdateControlPointLocal(index, newPoint.xy, pointType);
        }

        public float Length(float3 a, float3 b) => math.distance(a.xy, b.xy);

        public float3 GetProgress(ISpline2D spline, float progress)
        {
            ISpline2D spline2D = spline as ISpline2D;
            Assert.NotNull(spline2D);

            float2 point = spline2D.Get2DPoint(progress);
            return new float3(point.x, point.y, 0f);
        }

        public void CompareProgressEquals(ISpline2D spline, float progress, float3 expectedPoint, float tolerance = 0.00001f)
        {
            ISpline2D spline2D = spline as ISpline2D;
            Assert.NotNull(spline2D);

            float2 point = spline2D.Get2DPoint(progress);
            TestHelpers.CheckFloat2(point, expectedPoint.xy, tolerance);
        }

        public void CompareProgressNotEquals(ISpline2D spline, float progress, float3 expectedPoint)
        {
            ISpline2D spline2D = spline as ISpline2D;
            Assert.NotNull(spline2D);

            float2 point = spline2D.Get2DPoint(progress);
            Assert.AreNotEqual(point, expectedPoint.xy);
        }

        public void ComparePoint(float3 expected, float3 actual, float tolerance = 0.00001f)
        {
            TestHelpers.CheckFloat2(expected.xy, actual.xy, tolerance);
        }
    }
}