using Crener.Spline.Common;
using Crener.Spline.Test.BaseTests;
using Crener.Spline.Test.Helpers;
using NUnit.Framework;
using Unity.Mathematics;

namespace Crener.Spline.Test._3D.Bezier.TestAdapters
{
    public abstract class BezierBaseTest3DAdapter : BaseFunctionalityTests3D
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

            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            Assert.AreEqual(0.25f, testSpline.Times[0]);
            Assert.AreEqual(0.75f, testSpline.Times[1]);
            Assert.AreEqual(1f, testSpline.Times[2]);

            ComparePoint(a, GetProgressWorld(testSpline,0f));
            ComparePoint(new float3(2.5f, 0f, 0f), GetProgressWorld(testSpline, 0.25f));
            ComparePoint(new float3(5f, 0f, 0f), GetProgressWorld(testSpline, 0.5f));
            ComparePoint(new float3(10f, 0f, 0f), GetProgressWorld(testSpline, 1f));
            ComparePoint(new float3(10f, 0f, 0f), GetProgressWorld(testSpline, 1.5f));
            ComparePoint(new float3(10f, 0f, 0f), GetProgressWorld(testSpline, 5f));
        }

        [Test]
        public void PointCreation()
        {
            ITestSpline testSpline = PrepareSpline();

            float3 a = float3.zero;
            AddControlPointLocalSpace(testSpline, a);

            Assert.AreEqual(1, testSpline.ControlPointCount);
            Assert.AreEqual(1, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.Times.Count);

            Assert.AreEqual(1, testSpline.ControlPointCount);
            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));

            float3 b = new float3(10f, 0f, 0f);
            AddControlPointLocalSpace(testSpline, b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(10f, testSpline.Length());

            Assert.AreEqual(2, testSpline.ControlPointCount);
            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
            ComparePoint(new float3(1f, 0f, 0f), GetControlPoint(testSpline, 0, SplinePoint.Post));
            ComparePoint(new float3(9f, 0f, 0f), GetControlPoint(testSpline, 1, SplinePoint.Pre));
            ComparePoint(b, GetControlPoint(testSpline, 1, SplinePoint.Point));
        }

        [Test]
        public void Point5()
        {
            ITestSpline testSpline = PrepareSpline();

            float3 a = new float3(1f, 10f, 0f);
            AddControlPointLocalSpace(testSpline, a);
            float3 b = new float3(2f, 10f, 0f);
            AddControlPointLocalSpace(testSpline, b);
            float3 c = new float3(3f, 10f, 0f);
            AddControlPointLocalSpace(testSpline, c);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(2f, testSpline.Length());

            ComparePoint(new float3(2.5f, 10f, 0f), GetProgressWorld(testSpline, 0.7f), 0.01f);
        }

        [Test]
        public void NoEditModeChange([Values] SplineEditMode mode)
        {
            ITestSpline testSpline = PrepareSpline();

            float3 a = float3.zero;
            AddControlPointLocalSpace(testSpline, a);

            testSpline.ChangeEditMode(0, mode);
            Assert.AreEqual(mode, testSpline.GetEditMode(0));
        }

        [Test]
        public void ZigZagLength()
        {
            ITestSpline testSpline = PrepareSpline();

            float3 a = new float3(0f, 0f, 0f);
            float3 b = new float3(10f, 10f, 0f);
            float3 c = new float3(0f, 20f, 0f);
            float3 d = new float3(20f, 30f, 0f);
            AddControlPointLocalSpace(testSpline, a);
            AddControlPointLocalSpace(testSpline, d);
            InsertControlPointWorldSpace(testSpline, 1, b);
            InsertControlPointWorldSpace(testSpline, 2, c);

            float minLength = math.distance(a, b) + math.distance(b, c) + math.distance(c, d);
            Assert.Greater(testSpline.Length(), minLength);
        }

        [Test]
        public void ZigZagLength2()
        {
            ITestSpline testSpline = PrepareSpline();

            float3 a = new float3(0f, 0f, 0f);
            AddControlPointLocalSpace(testSpline, a);
            float3 b = new float3(1f, 3f, 0f);
            AddControlPointLocalSpace(testSpline, b);

            UpdateControlPoint(testSpline, 0, new float3(1f, 0f, 0f), SplinePoint.Post);
            UpdateControlPoint(testSpline, 1, new float3(0f, 3f, 0f), SplinePoint.Pre);

            float length = math.distance(a, b);
            Assert.Greater(testSpline.Length(), length);
        }
    }
}