using Crener.Spline.Common;
using Crener.Spline.Test.BaseTests;
using NUnit.Framework;
using Unity.Mathematics;

namespace Crener.Spline.Test._3DPlane.LinearCubicPlain.TestAdapters
{
    public abstract class LinearCubicBaseTest3DPlaneAdapter : BaseFunctionalityTests3DPlane
    {
        [Test]
        public void Point3()
        {
            ITestSpline testSpline = PrepareSpline();

            float3 a = float3.zero;
            AddControlPoint(testSpline, a);
            float3 b = new float3(2.5f, 0f, 0f);
            AddControlPoint(testSpline, b);
            float3 c = new float3(7.5f, 0f, 0f);
            AddControlPoint(testSpline, c);
            float3 d = new float3(10f, 0f, 0f);
            AddControlPoint(testSpline, d);

            Assert.AreEqual(4, testSpline.ControlPointCount);
            Assert.AreEqual(4, testSpline.Modes.Count);
            Assert.AreEqual(10f, testSpline.Length());

            Assert.AreEqual(2, testSpline.Times.Count);
            // a-b-c 1st spline segment
            Assert.AreEqual(0.5f, testSpline.Times[0]);
            // b-c-d 2nd spline segment
            Assert.AreEqual(1f, testSpline.Times[1]);

            ComparePoint(a, GetProgress(testSpline, -1f));
            ComparePoint(a, GetProgress(testSpline, 0f));
            ComparePoint(new float3(5f, 0f, 0f), GetProgress(testSpline, 0.5f));
            ComparePoint(d, GetProgress(testSpline, 1f));
            ComparePoint(d, GetProgress(testSpline, 1.5f));
            ComparePoint(d, GetProgress(testSpline, 5f));
        }

        [Test]
        public void NoEditModeChange([Values] SplineEditMode mode)
        {
            ITestSpline testSpline = PrepareSpline();

            float3 a = float3.zero;
            AddControlPoint(testSpline, a);

            testSpline.ChangeEditMode(0, mode);
            Assert.AreEqual(SplineEditMode.Standard, testSpline.GetEditMode(0));
        }

        [Test]
        public void ZigZagLength()
        {
            ITestSpline testSpline = PrepareSpline();

            float3 a = new float3(0f, 0f, 1f);
            AddControlPoint(testSpline, a);
            float3 b = new float3(1f, 3f, 1f);
            AddControlPoint(testSpline, b);
            
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