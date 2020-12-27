using Crener.Spline.Common;
using Crener.Spline.Test.Helpers;
using NUnit.Framework;
using Unity.Mathematics;

namespace Crener.Spline.Test._2D.Bezier.TestAdapters
{
    public abstract class BezierBaseTest2DAdapter : BaseSimpleSplineTests2D
    {
        [Test]
        public void Point3()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            testSpline2D.AddControlPoint(float2.zero);
            float2 b = new float2(2.5f, 0f);
            testSpline2D.AddControlPoint(b);
            float2 c = new float2(7.5f, 0f);
            testSpline2D.AddControlPoint(c);
            float2 d = new float2(10f, 0f);
            testSpline2D.AddControlPoint(d);

            Assert.AreEqual(4, testSpline2D.ControlPointCount);
            Assert.AreEqual(4, testSpline2D.Modes.Count);
            Assert.AreEqual(10f, testSpline2D.Length());

            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            Assert.AreEqual(0.25f, testSpline2D.Times[0]);
            Assert.AreEqual(0.75f, testSpline2D.Times[1]);
            Assert.AreEqual(1f, testSpline2D.Times[2]);

            TestHelpers.CheckFloat2(a, testSpline2D.Get2DPointWorld(0f));
            TestHelpers.CheckFloat2(new float2(2.5f, 0f), testSpline2D.Get2DPointWorld(0.25f));
            TestHelpers.CheckFloat2(new float2(5f, 0f), testSpline2D.Get2DPointWorld(0.5f));
            TestHelpers.CheckFloat2(new float2(10f, 0f), testSpline2D.Get2DPointWorld(1f));
            TestHelpers.CheckFloat2(new float2(10f, 0f), testSpline2D.Get2DPointWorld(1.5f));
            TestHelpers.CheckFloat2(new float2(10f, 0f), testSpline2D.Get2DPointWorld(5f));
        }

        [Test]
        public void PointCreation()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = new float2(0f, 0f);
            testSpline2D.AddControlPoint(float2.zero);

            Assert.AreEqual(1, testSpline2D.ControlPointCount);
            Assert.AreEqual(1, testSpline2D.Modes.Count);
            Assert.AreEqual(1, testSpline2D.Times.Count);

            Assert.AreEqual(1, testSpline2D.ControlPoints.Count);
            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));

            float2 b = new float2(10f, 0f);
            testSpline2D.AddControlPoint(b);

            Assert.AreEqual(2, testSpline2D.ControlPointCount);
            Assert.AreEqual(2, testSpline2D.Modes.Count);
            Assert.AreEqual(1, testSpline2D.Times.Count);
            Assert.AreEqual(10f, testSpline2D.Length());

            Assert.AreEqual(4, testSpline2D.ControlPoints.Count);
            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(new float2(1f, 0f), testSpline2D.GetControlPoint(0, SplinePoint.Post));
            TestHelpers.CheckFloat2(new float2(9f, 0f), testSpline2D.GetControlPoint(1, SplinePoint.Pre));
            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(1, SplinePoint.Point));
        }

        [Test]
        public void Point5()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = new float2(1f, 10f);
            testSpline2D.AddControlPoint(a);
            float2 b = new float2(2f, 10f);
            testSpline2D.AddControlPoint(b);
            float2 c = new float2(3f, 10f);
            testSpline2D.AddControlPoint(c);

            Assert.AreEqual(3, testSpline2D.ControlPointCount);
            Assert.AreEqual(2f, testSpline2D.Length());

            TestHelpers.CheckFloat2(new float2(2.5f, 10f), testSpline2D.Get2DPointWorld(0.7f), 0.01f);
        }

        [Test]
        public void NoEditModeChange([Values] SplineEditMode mode)
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            testSpline2D.AddControlPoint(a);

            testSpline2D.ChangeEditMode(0, mode);
            Assert.AreEqual(mode, testSpline2D.GetEditMode(0));
        }

        [Test]
        public void ZigZagLength()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = new float2(0f, 0f);
            float2 b = new float2(10f, 10f);
            float2 c = new float2(0f, 20f);
            float2 d = new float2(20f, 30f);
            testSpline2D.AddControlPoint(a);
            testSpline2D.AddControlPoint(d);
            testSpline2D.InsertControlPoint(1, b);
            testSpline2D.InsertControlPoint(2, c);

            float minLength = math.distance(a, b) + math.distance(b, c) + math.distance(c, d);
            Assert.Greater(testSpline2D.Length(), minLength);
        }

        [Test]
        public void ZigZagLength2()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = new float2(0f, 0f);
            testSpline2D.AddControlPoint(a);
            float2 b = new float2(1f, 3f);
            testSpline2D.AddControlPoint(b);

            testSpline2D.UpdateControlPointLocal(0, new float2(1f, 0f), SplinePoint.Post);
            testSpline2D.UpdateControlPointLocal(1, new float2(0f, 3f), SplinePoint.Pre);

            float length = math.distance(a, b);
            Assert.Greater(testSpline2D.Length(), length);
        }
    }
}