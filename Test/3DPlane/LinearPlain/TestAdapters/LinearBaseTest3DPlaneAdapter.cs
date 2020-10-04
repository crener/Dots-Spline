using Crener.Spline.Common;
using Crener.Spline.Test._3D;
using Crener.Spline.Test.BaseTests;
using Crener.Spline.Test.Helpers;
using NUnit.Framework;
using Unity.Mathematics;

namespace Crener.Spline.Test._3DPlane.LinearPlain.TestAdapters
{
    public abstract class LinearBaseTest3DPlaneAdapter : BaseFunctionalityTests3DPlane
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

            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            Assert.AreEqual(0.25f, testSpline.Times[0]);
            Assert.AreEqual(0.75f, testSpline.Times[1]);
            Assert.AreEqual(1f, testSpline.Times[2]);

            ComparePoint(a, GetProgress(testSpline, 0f));
            ComparePoint(b, GetProgress(testSpline, 0.25f));
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
            float3 b = new float3(10f, 10f, 1f);
            AddControlPoint(testSpline, b);
            float3 c = new float3(0f, 20f, 1f);
            AddControlPoint(testSpline, c);
            float3 d = new float3(20f, 30f, 1f);
            AddControlPoint(testSpline, d);

            float length = math.distance(a, b) + math.distance(b, c) + math.distance(c, d);
            float spline = testSpline.Length();
            Assert.IsTrue(math.abs(length - spline) <= 0.00005f, $"Expected: {length}, but received: {spline}");
        }

        [Test]
        public void ZigZagLength2()
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