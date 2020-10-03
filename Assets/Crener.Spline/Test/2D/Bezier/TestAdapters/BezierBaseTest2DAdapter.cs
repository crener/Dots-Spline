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
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 a = float2.zero;
            testSpline.AddControlPoint(float2.zero);
            float2 b = new float2(2.5f, 0f);
            testSpline.AddControlPoint(b);
            float2 c = new float2(7.5f, 0f);
            testSpline.AddControlPoint(c);
            float2 d = new float2(10f, 0f);
            testSpline.AddControlPoint(d);

            Assert.AreEqual(4, testSpline.ControlPointCount);
            Assert.AreEqual(4, testSpline.Modes.Count);
            Assert.AreEqual(10f, testSpline.Length());

            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            Assert.AreEqual(0.25f, testSpline.Times[0]);
            Assert.AreEqual(0.75f, testSpline.Times[1]);
            Assert.AreEqual(1f, testSpline.Times[2]);

            TestHelpers.CheckFloat2(a, testSpline.Get2DPoint(0f));
            TestHelpers.CheckFloat2(new float2(2.5f, 0f), testSpline.Get2DPoint(0.25f));
            TestHelpers.CheckFloat2(new float2(5f, 0f), testSpline.Get2DPoint(0.5f));
            TestHelpers.CheckFloat2(new float2(10f, 0f), testSpline.Get2DPoint(1f));
            TestHelpers.CheckFloat2(new float2(10f, 0f), testSpline.Get2DPoint(1.5f));
            TestHelpers.CheckFloat2(new float2(10f, 0f), testSpline.Get2DPoint(5f));
        }
        
        [Test]
        public void PointCreation()
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 a = new float2(0f, 0f);
            testSpline.AddControlPoint(float2.zero);

            Assert.AreEqual(1, testSpline.ControlPointCount);
            Assert.AreEqual(1, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.Times.Count);

            Assert.AreEqual(1, testSpline.ControlPoints.Count);
            TestHelpers.CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));

            float2 b = new float2(10f, 0f);
            testSpline.AddControlPoint(b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(10f, testSpline.Length());

            Assert.AreEqual(4, testSpline.ControlPoints.Count);
            TestHelpers.CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(new float2(1f, 0f), testSpline.GetControlPoint(0, SplinePoint.Post));
            TestHelpers.CheckFloat2(new float2(9f, 0f), testSpline.GetControlPoint(1, SplinePoint.Pre));
            TestHelpers.CheckFloat2(b, testSpline.GetControlPoint(1, SplinePoint.Point));
        }

        [Test]
        public void Point5()
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 a = new float2(1f, 10f);
            testSpline.AddControlPoint(a);
            float2 b = new float2(2f, 10f);
            testSpline.AddControlPoint(b);
            float2 c = new float2(3f, 10f);
            testSpline.AddControlPoint(c);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(2f, testSpline.Length());

            TestHelpers.CheckFloat2(new float2(2.5f, 10f), testSpline.Get2DPoint(0.7f), 0.01f);
        }

        [Test]
        public void NoEditModeChange([Values] SplineEditMode mode)
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 a = float2.zero;
            testSpline.AddControlPoint(a);

            testSpline.ChangeEditMode(0, mode);
            Assert.AreEqual(mode, testSpline.GetEditMode(0));
        }

        [Test]
        public void ZigZagLength()
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 a = new float2(0f, 0f);
            float2 b = new float2(10f, 10f);
            float2 c = new float2(0f, 20f);
            float2 d = new float2(20f, 30f);
            testSpline.AddControlPoint(a);
            testSpline.AddControlPoint(d);
            testSpline.InsertControlPoint(1, b);
            testSpline.InsertControlPoint(2, c);

            float minLength = math.distance(a, b) + math.distance(b, c) + math.distance(c, d);
            Assert.Greater(testSpline.Length(), minLength);
        }

        [Test]
        public void ZigZagLength2()
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 a = new float2(0f, 0f);
            testSpline.AddControlPoint(a);
            float2 b = new float2(1f, 3f);
            testSpline.AddControlPoint(b);

            testSpline.UpdateControlPointLocal(0, new float2(1f,0f),SplinePoint.Post );
            testSpline.UpdateControlPointLocal(1, new float2(0f,3f),SplinePoint.Pre );
            
            float length = math.distance(a, b);
            Assert.Greater(testSpline.Length(), length);
        }
    }
}