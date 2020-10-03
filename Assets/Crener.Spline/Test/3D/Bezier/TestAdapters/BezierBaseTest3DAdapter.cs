using Crener.Spline.Common;
using Crener.Spline.Test.Helpers;
using NUnit.Framework;
using Unity.Mathematics;

namespace Crener.Spline.Test._3D.Bezier.TestAdapters
{
    public abstract class BezierBaseTest3DAdapter : BaseSimpleSplineTests3D
    {
        [Test]
        public void Point3()
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

            float3 a = float3.zero;
            testSpline.AddControlPoint(float3.zero);
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

            TestHelpers.CheckFloat3(a, testSpline.Get3DPoint(0f));
            TestHelpers.CheckFloat3(new float3(2.5f, 0f, 0f), testSpline.Get3DPoint(0.25f));
            TestHelpers.CheckFloat3(new float3(5f, 0f, 0f), testSpline.Get3DPoint(0.5f));
            TestHelpers.CheckFloat3(new float3(10f, 0f, 0f), testSpline.Get3DPoint(1f));
            TestHelpers.CheckFloat3(new float3(10f, 0f, 0f), testSpline.Get3DPoint(1.5f));
            TestHelpers.CheckFloat3(new float3(10f, 0f, 0f), testSpline.Get3DPoint(5f));
        }

        [Test]
        public void PointCreation()
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

            float3 a = float3.zero;
            testSpline.AddControlPoint(a);

            Assert.AreEqual(1, testSpline.ControlPointCount);
            Assert.AreEqual(1, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.Times.Count);

            Assert.AreEqual(1, testSpline.ControlPoints.Count);
            TestHelpers.CheckFloat3(a, testSpline.GetControlPoint(0, SplinePoint.Point));

            float3 b = new float3(10f, 0f, 0f);
            testSpline.AddControlPoint(b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(10f, testSpline.Length());

            Assert.AreEqual(4, testSpline.ControlPoints.Count);
            TestHelpers.CheckFloat3(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat3(new float3(1f, 0f, 0f), testSpline.GetControlPoint(0, SplinePoint.Post));
            TestHelpers.CheckFloat3(new float3(9f, 0f, 0f), testSpline.GetControlPoint(1, SplinePoint.Pre));
            TestHelpers.CheckFloat3(b, testSpline.GetControlPoint(1, SplinePoint.Point));
        }

        [Test]
        public void Point5()
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

            float3 a = new float3(1f, 10f, 0f);
            testSpline.AddControlPoint(a);
            float3 b = new float3(2f, 10f, 0f);
            testSpline.AddControlPoint(b);
            float3 c = new float3(3f, 10f, 0f);
            testSpline.AddControlPoint(c);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(2f, testSpline.Length());

            TestHelpers.CheckFloat3(new float3(2.5f, 10f, 0f), testSpline.Get3DPoint(0.7f), 0.01f);
        }

        [Test]
        public void NoEditModeChange([Values] SplineEditMode mode)
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

            float3 a = float3.zero;
            testSpline.AddControlPoint(a);

            testSpline.ChangeEditMode(0, mode);
            Assert.AreEqual(mode, testSpline.GetEditMode(0));
        }

        [Test]
        public void ZigZagLength()
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

            float3 a = new float3(0f, 0f, 0f);
            float3 b = new float3(10f, 10f, 0f);
            float3 c = new float3(0f, 20f, 0f);
            float3 d = new float3(20f, 30f, 0f);
            testSpline.AddControlPoint(a);
            testSpline.AddControlPoint(d);
            testSpline.InsertControlPointWorldSpace(1, b);
            testSpline.InsertControlPointWorldSpace(2, c);

            float minLength = math.distance(a, b) + math.distance(b, c) + math.distance(c, d);
            Assert.Greater(testSpline.Length(), minLength);
        }

        [Test]
        public void ZigZagLength2()
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

            float3 a = new float3(0f, 0f, 0f);
            testSpline.AddControlPoint(a);
            float3 b = new float3(1f, 3f, 0f);
            testSpline.AddControlPoint(b);

            testSpline.UpdateControlPointWorld(0, new float3(1f, 0f, 0f), SplinePoint.Post);
            testSpline.UpdateControlPointWorld(1, new float3(0f, 3f, 0f), SplinePoint.Pre);

            float length = math.distance(a, b);
            Assert.Greater(testSpline.Length(), length);
        }
    }
}