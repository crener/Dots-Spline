using Crener.Spline.Common;
using Crener.Spline.Test.BaseTests;
using NUnit.Framework;
using Unity.Mathematics;

namespace Crener.Spline.Test._3DPlane.LinearCubicPlain.TestAdapters
{
    public abstract class LinearCubicBaseTest2DPlaneAdapter : BaseFunctionalityTests2DPlane
    {
        [Test]
        public void Point3()
        {
            ITestSpline testSpline = PrepareSpline();

            float3 a = float3.zero;
            AddControlPointLocalSpace(testSpline, a);
            float3 b = new float3(2.5f, 0f, 0f);
            AddControlPointLocalSpace(testSpline, b);
            float3 c = new float3(7.5f, 0f, 0f);
            AddControlPointLocalSpace(testSpline, c);
            float3 d = new float3(10f, 0f, 0f);
            AddControlPointLocalSpace(testSpline, d);

            Assert.AreEqual(4, testSpline.ControlPointCount);
            Assert.AreEqual(4, testSpline.Modes.Count);
            Assert.AreEqual(10f, testSpline.Length());

            Assert.AreEqual(2, testSpline.Times.Count);
            // a-b-c 1st spline segment
            Assert.AreEqual(0.5f, testSpline.Times[0]);
            // b-c-d 2nd spline segment
            Assert.AreEqual(1f, testSpline.Times[1]);

            ComparePoint(a, GetProgressWorld(testSpline, -1f));
            ComparePoint(a, GetProgressWorld(testSpline, 0f));
            ComparePoint(new float3(5f, 0f, 0f), GetProgressWorld(testSpline, 0.5f));
            ComparePoint(d, GetProgressWorld(testSpline, 1f));
            ComparePoint(d, GetProgressWorld(testSpline, 1.5f));
            ComparePoint(d, GetProgressWorld(testSpline, 5f));
        }

        [Test]
        public void NoEditModeChange([Values] SplineEditMode mode)
        {
            ITestSpline testSpline = PrepareSpline();

            float3 a = float3.zero;
            AddControlPointLocalSpace(testSpline, a);

            testSpline.ChangeEditMode(0, mode);
            Assert.AreEqual(SplineEditMode.Standard, testSpline.GetEditMode(0));
        }

        [Test]
        public void ZigZagLength()
        {
            ITestSpline testSpline = PrepareSpline();

            float3 a = new float3(0f, 0f, 1f);
            AddControlPointLocalSpace(testSpline, a);
            float3 b = new float3(1f, 3f, 1f);
            AddControlPointLocalSpace(testSpline, b);
            
            float splinePreUpdate = testSpline.Length();
            UpdateControlPoint(testSpline,0, new float3(1f, 0f, 1f), SplinePoint.Post);
            UpdateControlPoint(testSpline,1, new float3(2f, 3f, 1f), SplinePoint.Pre);

            float spline = testSpline.Length();
            Assert.AreEqual(splinePreUpdate, spline);
            
            float length = Length(a, b);
            Assert.IsTrue(math.abs(length - spline) <= 0.00005f, $"Expected: {length}, but received: {spline}");
        }
    }
}