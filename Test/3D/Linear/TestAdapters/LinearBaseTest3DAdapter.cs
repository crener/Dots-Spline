using Crener.Spline.Common;
using Crener.Spline.Test.Helpers;
using NUnit.Framework;
using Unity.Mathematics;

namespace Crener.Spline.Test._3D.Linear.TestAdapters
{
    public abstract class LinearBaseTest3DAdapter : BaseSimpleSplineTests3D
    {
        [Test]
        public void Point3()
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

            float3 a = float3.zero;
            testSpline.AddControlPoint(a);
            float3 b = new float3(2.5f, 0f, 0f);
            testSpline.AddControlPoint(b);
            float3 c = new float3(7.5f, 0f, 0f);
            testSpline.AddControlPoint(c);
            float3 d = new float3(10f, 0f, 0f);
            testSpline.AddControlPoint(d);

            Assert.AreEqual(4, testSpline.ControlPointCount);
            Assert.AreEqual(4, testSpline.Modes.Count);
            Assert.AreEqual(10f, testSpline.Length());

            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            Assert.AreEqual(0.25f, testSpline.Times[0]);
            Assert.AreEqual(0.75f, testSpline.Times[1]);
            Assert.AreEqual(1f, testSpline.Times[2]);

            TestHelpers.CheckFloat3(a, testSpline.GetPoint(0f));
            TestHelpers.CheckFloat3(b, testSpline.GetPoint(0.25f));
            TestHelpers.CheckFloat3(new float3(5f, 0f, 0f), testSpline.GetPoint(0.5f));
            TestHelpers.CheckFloat3(d, testSpline.GetPoint(1f));
            TestHelpers.CheckFloat3(d, testSpline.GetPoint(1.5f));
            TestHelpers.CheckFloat3(d, testSpline.GetPoint(5f));
        }

        [Test]
        public void NoEditModeChange([Values] SplineEditMode mode)
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

            float3 a = float3.zero;
            testSpline.AddControlPoint(a);

            testSpline.ChangeEditMode(0, mode);
            Assert.AreEqual(SplineEditMode.Standard, testSpline.GetEditMode(0));
        }

        [Test]
        public void ZigZagLength()
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

            float3 a = new float3(0f, 0f, 1f);
            testSpline.AddControlPoint(a);
            float3 b = new float3(10f, 10f, 1f);
            testSpline.AddControlPoint(b);
            float3 c = new float3(0f, 20f, 1f);
            testSpline.AddControlPoint(c);
            float3 d = new float3(20f, 30f, 1f);
            testSpline.AddControlPoint(d);

            float length = math.distance(a, b) + math.distance(b, c) + math.distance(c, d);
            float spline = testSpline.Length();
            Assert.IsTrue(math.abs(length - spline) <= 0.00005f, $"Expected: {length}, but received: {spline}");
        }

        [Test]
        public void ZigZagLength2()
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

            float3 a = new float3(0f, 0f, 1f);
            testSpline.AddControlPoint(a);
            float3 b = new float3(1f, 3f, 1f);
            testSpline.AddControlPoint(b);

            testSpline.UpdateControlPoint(0, new float3(1f, 0f, 1f), SplinePoint.Post);
            testSpline.UpdateControlPoint(1, new float3(0f, 3f, 1f), SplinePoint.Pre);

            float length = math.distance(a, b);
            float spline = testSpline.Length();
            Assert.IsTrue(math.abs(length - spline) <= 0.00005f, $"Expected: {length}, but received: {spline}");
        }
    }
}