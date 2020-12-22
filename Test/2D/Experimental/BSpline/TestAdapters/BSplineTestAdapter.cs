using Crener.Spline.Common;
using Crener.Spline.Test.Helpers;
using NUnit.Framework;
using Unity.Mathematics;

namespace Crener.Spline.Test._2D.Experimental.BSpline.TestAdapters
{
    public abstract class BSplineTestAdapter : SelfCleanUpTestSet
    {
        /// <summary>
        /// Create a new instance of the spline
        /// </summary>
        protected abstract ISimpleTestSpline2D PrepareSpline();

        #region Original tests
        [Test]
        public void Basic()
        {
            ISimpleTestSpline2D spline2D = PrepareSpline();
            Assert.NotNull(spline2D);
        }

        [Test]
        public void NoEditModeChange([Values] SplineEditMode mode)
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            testSpline2D.AddControlPoint(a);

            testSpline2D.ChangeEditMode(0, mode);
            Assert.AreEqual(SplineEditMode.Standard, testSpline2D.GetEditMode(0));
        }

        [Test]
        public void Add1()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            testSpline2D.AddControlPoint(a);

            Assert.AreEqual(1, testSpline2D.ControlPointCount);
            Assert.AreEqual(testSpline2D.ExpectedControlPointCount(1), testSpline2D.ControlPoints.Count);
            Assert.AreEqual(1, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            Assert.AreEqual(0f, testSpline2D.Length());

            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
        }

        [Test]
        public void Add2()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            testSpline2D.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            testSpline2D.AddControlPoint(b);

            Assert.AreEqual(2, testSpline2D.ControlPointCount);
            Assert.AreEqual(testSpline2D.ExpectedControlPointCount(2), testSpline2D.ControlPoints.Count);
            Assert.AreEqual(2, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            Assert.AreEqual(0.5f, testSpline2D.Length());

            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(1, SplinePoint.Point));
        }

        [Test]
        public void Add3()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            testSpline2D.AddControlPoint(float2.zero);
            float2 b = new float2(1f, 0f);
            testSpline2D.AddControlPoint(b);
            float2 c = new float2(2f, 0f);
            testSpline2D.AddControlPoint(c);

            Assert.AreEqual(3, testSpline2D.ControlPointCount);
            Assert.AreEqual(testSpline2D.ExpectedControlPointCount(3), testSpline2D.ControlPoints.Count);
            Assert.AreEqual(3, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            Assert.AreEqual(1f, testSpline2D.Length());

            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(1, SplinePoint.Point));
            TestHelpers.CheckFloat2(c, testSpline2D.GetControlPoint(2, SplinePoint.Point));
        }

        [Test]
        public void Remove()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            testSpline2D.AddControlPoint(float2.zero);
            float2 b = new float2(1f, 0f);
            testSpline2D.AddControlPoint(b);
            float2 c = new float2(2f, 0f);
            testSpline2D.AddControlPoint(c);

            Assert.AreEqual(3, testSpline2D.ControlPointCount);
            Assert.AreEqual(3, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedControlPointCount(3), testSpline2D.ControlPoints.Count);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            Assert.AreEqual(1f, testSpline2D.Length());

            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(1, SplinePoint.Point));
            TestHelpers.CheckFloat2(c, testSpline2D.GetControlPoint(2, SplinePoint.Point));

            //Remove a point
            testSpline2D.RemoveControlPoint(1);

            Assert.AreEqual(2, testSpline2D.ControlPointCount);
            Assert.AreEqual(2, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedControlPointCount(2), testSpline2D.ControlPoints.Count);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            Assert.AreEqual(1f, testSpline2D.Length());

            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(c, testSpline2D.GetControlPoint(1, SplinePoint.Point));
        }

        [Test]
        public void RemoveFromStart()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            testSpline2D.AddControlPoint(float2.zero);
            float2 b = new float2(1f, 0f);
            testSpline2D.AddControlPoint(b);

            Assert.AreEqual(2, testSpline2D.ControlPointCount);
            Assert.AreEqual(2, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedControlPointCount(2), testSpline2D.ControlPoints.Count);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            Assert.AreEqual(0.5f, testSpline2D.Length());

            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(1, SplinePoint.Point));

            //Remove a point
            testSpline2D.RemoveControlPoint(0);

            Assert.AreEqual(1, testSpline2D.ControlPointCount);
            Assert.AreEqual(1, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedControlPointCount(1), testSpline2D.ControlPoints.Count);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            Assert.AreEqual(0f, testSpline2D.Length());

            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(0, SplinePoint.Point));
        }

        [Test]
        public void RemoveFromEnd2Points()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            testSpline2D.AddControlPoint(float2.zero);
            float2 b = new float2(1f, 0f);
            testSpline2D.AddControlPoint(b);

            Assert.AreEqual(2, testSpline2D.ControlPointCount);
            Assert.AreEqual(2, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedControlPointCount(2), testSpline2D.ControlPoints.Count);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            Assert.AreEqual(0.5f, testSpline2D.Length());

            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(1, SplinePoint.Point));

            //Remove a point
            testSpline2D.RemoveControlPoint(1);

            Assert.AreEqual(1, testSpline2D.ControlPointCount);
            Assert.AreEqual(1, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedControlPointCount(1), testSpline2D.ControlPoints.Count);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            Assert.AreEqual(0f, testSpline2D.Length());

            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
        }

        [Test]
        public void RemoveFromEnd3Points()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            float2 b = new float2(1f, 0f);
            float2 c = new float2(10f, 0f);
            testSpline2D.AddControlPoint(a);
            testSpline2D.AddControlPoint(b);
            testSpline2D.AddControlPoint(c);

            Assert.AreEqual(3, testSpline2D.ControlPointCount);
            Assert.AreEqual(3, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedControlPointCount(3), testSpline2D.ControlPoints.Count);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            Assert.AreEqual(5f, testSpline2D.Length());

            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(1, SplinePoint.Point));
            TestHelpers.CheckFloat2(c, testSpline2D.GetControlPoint(2, SplinePoint.Point));

            //Remove a point
            testSpline2D.RemoveControlPoint(2);

            Assert.AreEqual(2, testSpline2D.ControlPointCount);
            Assert.AreEqual(2, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedControlPointCount(2), testSpline2D.ControlPoints.Count);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            Assert.AreEqual(0.5f, testSpline2D.Length());

            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(1, SplinePoint.Point));
        }

        [Test]
        public void RemoveFromOutOfRange()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            testSpline2D.AddControlPoint(float2.zero);
            float2 b = new float2(1f, 0f);
            testSpline2D.AddControlPoint(b);

            Assert.AreEqual(2, testSpline2D.ControlPointCount);
            Assert.AreEqual(2, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedControlPointCount(2), testSpline2D.ControlPoints.Count);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            Assert.AreEqual(0.5f, testSpline2D.Length());

            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(1, SplinePoint.Point));

            //Remove a point
            testSpline2D.RemoveControlPoint(300);

            Assert.AreEqual(1, testSpline2D.ControlPointCount);
            Assert.AreEqual(1, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedControlPointCount(1), testSpline2D.ControlPoints.Count);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            Assert.AreEqual(0f, testSpline2D.Length());

            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
        }

        [Test]
        public void Add3Remove2()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            testSpline2D.AddControlPoint(float2.zero);
            float2 b = new float2(1f, 0f);
            testSpline2D.AddControlPoint(b);
            float2 c = new float2(2f, 0f);
            testSpline2D.AddControlPoint(c);

            Assert.AreEqual(3, testSpline2D.ControlPointCount);
            Assert.AreEqual(3, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            Assert.AreEqual(1f, testSpline2D.Length());

            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(1, SplinePoint.Point));
            TestHelpers.CheckFloat2(c, testSpline2D.GetControlPoint(2, SplinePoint.Point));

            // Remove a point
            testSpline2D.RemoveControlPoint(2);

            Assert.AreEqual(2, testSpline2D.ControlPointCount);
            Assert.AreEqual(2, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedControlPointCount(2), testSpline2D.ControlPoints.Count);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            Assert.AreEqual(0.5f, testSpline2D.Length());

            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(1, SplinePoint.Point));

            // Remove another point
            testSpline2D.RemoveControlPoint(1);

            Assert.AreEqual(1, testSpline2D.ControlPointCount);
            Assert.AreEqual(1, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedControlPointCount(1), testSpline2D.ControlPoints.Count);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            Assert.AreEqual(0f, testSpline2D.Length());

            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
        }

        [Test]
        public void RemoveFromEndOutOfRangeUnder()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            testSpline2D.AddControlPoint(float2.zero);
            float2 b = new float2(1f, 0f);
            testSpline2D.AddControlPoint(b);

            Assert.AreEqual(2, testSpline2D.ControlPointCount);
            Assert.AreEqual(2, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedControlPointCount(2), testSpline2D.ControlPoints.Count);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            Assert.AreEqual(0.5f, testSpline2D.Length());

            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(1, SplinePoint.Point));

            // Remove a point less than 0
            testSpline2D.RemoveControlPoint(-3);

            Assert.AreEqual(2, testSpline2D.ControlPointCount);
            Assert.AreEqual(2, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedControlPointCount(2), testSpline2D.ControlPoints.Count);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            Assert.AreEqual(0.5f, testSpline2D.Length());

            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(1, SplinePoint.Point));
        }

        [Test]
        public void RemoveWhenEmpty()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            Assert.AreEqual(0, testSpline2D.ControlPointCount, "spline should be empty");

            //Remove a point
            testSpline2D.RemoveControlPoint(0);
            Assert.AreEqual(0, testSpline2D.ControlPointCount, "spline should be empty");

            //Remove a point
            testSpline2D.RemoveControlPoint(1000);
            Assert.AreEqual(0, testSpline2D.ControlPointCount, "spline should be empty");
        }

        [Test]
        public void NoPoint()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            Assert.AreEqual(0, testSpline2D.ControlPointCount);
            Assert.AreEqual(0f, testSpline2D.Length());

            TestHelpers.CheckFloat2(float2.zero, testSpline2D.Get2DPointWorld(0.5f));
        }

        [Test]
        public void Point()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            testSpline2D.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            testSpline2D.AddControlPoint(b);

            Assert.AreEqual(2, testSpline2D.ControlPointCount);
            Assert.AreEqual(0.5f, testSpline2D.Length());

            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            Assert.AreEqual(1f, testSpline2D.Times[0]);

            TestHelpers.CheckFloat2(new float2(0.5f, 0f), testSpline2D.Get2DPointWorld(0.5f));
        }

        [Test]
        public void Point2()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            testSpline2D.AddControlPoint(float2.zero);
            float2 b = new float2(1f, 0f);
            testSpline2D.AddControlPoint(b);
            float2 c = new float2(2f, 0f);
            testSpline2D.AddControlPoint(c);

            Assert.AreEqual(3, testSpline2D.ControlPointCount);
            Assert.AreEqual(1f, testSpline2D.Length());

            Assert.AreEqual(1, testSpline2D.Times.Count);
            Assert.AreEqual(1f, testSpline2D.Times[0]);

            TestHelpers.CheckFloat2(new float2(0.5f, 0f), testSpline2D.Get2DPointWorld(0f));
            TestHelpers.CheckFloat2(new float2(1f, 0f), testSpline2D.Get2DPointWorld(0.5f));
        }

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
            Assert.AreEqual(7.5f, testSpline2D.Length());

            Assert.AreEqual(2, testSpline2D.Times.Count);
            Assert.AreEqual(0.5f, testSpline2D.Times[0]);
            Assert.AreEqual(1f, testSpline2D.Times[1]);

            TestHelpers.CheckFloat2(new float2(1.25f, 0f), testSpline2D.Get2DPointWorld(0f));
            TestHelpers.CheckFloat2(new float2(2.8125f, 0f), testSpline2D.Get2DPointWorld(0.25f));
            TestHelpers.CheckFloat2(new float2(5f, 0f), testSpline2D.Get2DPointWorld(0.5f));
            float2 max = new float2(8.75f, 0f);
            TestHelpers.CheckFloat2(max, testSpline2D.Get2DPointWorld(1f));
            TestHelpers.CheckFloat2(max, testSpline2D.Get2DPointWorld(1.5f));
            TestHelpers.CheckFloat2(max, testSpline2D.Get2DPointWorld(5f));
        }

        [Test]
        public void Point4()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = new float2(3f, 3f);
            testSpline2D.AddControlPoint(a);

            Assert.AreEqual(1, testSpline2D.ControlPointCount);
            Assert.AreEqual(0f, testSpline2D.Length());

            TestHelpers.CheckFloat2(new float2(3f, 3f), testSpline2D.Get2DPointWorld(0.5f));
        }

        [Test]
        public void Update()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            testSpline2D.AddControlPoint(float2.zero);
            float2 b = new float2(1f, 0f);
            testSpline2D.AddControlPoint(b);

            Assert.AreEqual(2, testSpline2D.ControlPointCount);
            Assert.AreEqual(2, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            TestHelpers.CheckFloat2(a, testSpline2D.Get2DPointWorld(0f));
            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline2D.Get2DPointWorld(1f));
            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(1, SplinePoint.Point));

            //update 0 point position
            float2 a2 = new float2(-1f, -1f);
            testSpline2D.UpdateControlPointLocal(0, a2, SplinePoint.Point);

            TestHelpers.CheckFloat2(a2, testSpline2D.Get2DPointWorld(0f));
            TestHelpers.CheckFloat2(a2, testSpline2D.GetControlPoint(0, SplinePoint.Point));

            //update 1 point position
            float2 b2 = new float2(2f, 2f);
            testSpline2D.UpdateControlPointLocal(1, b2, SplinePoint.Point);

            TestHelpers.CheckFloat2(b2, testSpline2D.Get2DPointWorld(1f));
            TestHelpers.CheckFloat2(b2, testSpline2D.GetControlPoint(1, SplinePoint.Point));
        }

        [Test]
        public void Update2()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            testSpline2D.AddControlPoint(float2.zero);
            float2 b = new float2(1f, 0f);
            testSpline2D.AddControlPoint(b);
            float2 c = new float2(2f, 0f);
            testSpline2D.AddControlPoint(c);

            Assert.AreEqual(3, testSpline2D.ControlPointCount);
            Assert.AreEqual(3, testSpline2D.Modes.Count);
            Assert.AreEqual(1, testSpline2D.Times.Count);
            TestHelpers.CheckFloat2(new float2(0.5f, 0f), testSpline2D.Get2DPointWorld(0f));
            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline2D.Get2DPointWorld(0.5f));
            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(1, SplinePoint.Point));
            TestHelpers.CheckFloat2(new float2(1.5f, 0f), testSpline2D.Get2DPointWorld(1f));
            TestHelpers.CheckFloat2(c, testSpline2D.GetControlPoint(2, SplinePoint.Point));

            //update 0 point position
            testSpline2D.UpdateControlPointLocal(0, new float2(0.5f, 1f), SplinePoint.Point);
            TestHelpers.CheckFloat2(new float2(0.75f, 0.5f), testSpline2D.Get2DPointWorld(0f));
            TestHelpers.CheckFloat2(new float2(0.5f, 1f), testSpline2D.GetControlPoint(0, SplinePoint.Point));

            //update 1 point position
            float2 b2 = new float2(1f, 1f);
            testSpline2D.UpdateControlPointLocal(1, b2, SplinePoint.Point);
            TestHelpers.CheckFloat2(b2, testSpline2D.GetControlPoint(1, SplinePoint.Point));

            //update 2 point position
            testSpline2D.UpdateControlPointLocal(2, new float2(2f, 1f), SplinePoint.Point);
            TestHelpers.CheckFloat2(new float2(1.5f, 1f), testSpline2D.Get2DPointWorld(1f));
            TestHelpers.CheckFloat2(new float2(2f, 1f), testSpline2D.GetControlPoint(2, SplinePoint.Point));
        }

        [Test]
        public void Update3()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            testSpline2D.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            testSpline2D.AddControlPoint(b);
            float2 c = new float2(2f, 0f);
            testSpline2D.AddControlPoint(c);

            Assert.AreEqual(1f, testSpline2D.Length());

            // update 1 point position
            float2 b2 = new float2(1f, 2f);
            testSpline2D.UpdateControlPointLocal(1, b2, SplinePoint.Point);
            Assert.GreaterOrEqual(testSpline2D.Length(), 1f);
        }

        [Test]
        public void Insert()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            testSpline2D.AddControlPoint(float2.zero);
            float2 b = new float2(10f, 0f);
            testSpline2D.AddControlPoint(b);

            Assert.AreEqual(2, testSpline2D.ControlPointCount);
            Assert.AreEqual(2, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            TestHelpers.CheckFloat2(a, testSpline2D.Get2DPointWorld(0f));
            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline2D.Get2DPointWorld(1f));
            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(1, SplinePoint.Point));

            //insert point
            float2 c = new float2(20f, 0f);
            testSpline2D.InsertControlPoint(1, c);

            Assert.AreEqual(3, testSpline2D.ControlPointCount);
            Assert.AreEqual(3, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(c, testSpline2D.GetControlPoint(1, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(2, SplinePoint.Point));
        }

        [Test]
        public void InsertWithOne()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            testSpline2D.AddControlPoint(float2.zero);

            Assert.AreEqual(1, testSpline2D.ControlPointCount);
            Assert.AreEqual(1, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            TestHelpers.CheckFloat2(a, testSpline2D.Get2DPointWorld(0f));
            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));

            float2 b = new float2(10f, 0f);
            testSpline2D.InsertControlPoint(1000, b);

            TestHelpers.CheckFloat2(b, testSpline2D.Get2DPointWorld(1f));
            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(1, SplinePoint.Point));

            Assert.AreEqual(2, testSpline2D.ControlPointCount);
            Assert.AreEqual(2, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
        }

        [Test]
        public void InsertEmpty()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            Assert.AreEqual(0, testSpline2D.ControlPointCount);
            Assert.AreEqual(0, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);

            float2 a = float2.zero;
            testSpline2D.InsertControlPoint(12, a);

            Assert.AreEqual(1, testSpline2D.ControlPointCount);
            Assert.AreEqual(1, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            TestHelpers.CheckFloat2(a, testSpline2D.Get2DPointWorld(0f));
            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
        }

        [Test]
        public void InsertAtEnd()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            float2 b = new float2(10f, 0f);
            float2 c = new float2(20f, 0f);

            testSpline2D.AddControlPoint(a);
            testSpline2D.AddControlPoint(b);

            Assert.AreEqual(2, testSpline2D.ControlPointCount);
            Assert.AreEqual(2, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            TestHelpers.CheckFloat2(a, testSpline2D.Get2DPointWorld(0f));
            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline2D.Get2DPointWorld(1f));
            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(1, SplinePoint.Point));

            //insert point
            testSpline2D.InsertControlPoint(2, c);

            Assert.AreEqual(3, testSpline2D.ControlPointCount);
            Assert.AreEqual(3, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(1, SplinePoint.Point));
            TestHelpers.CheckFloat2(c, testSpline2D.GetControlPoint(2, SplinePoint.Point));
        }

        [Test]
        public void InsertAtStart()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            testSpline2D.AddControlPoint(float2.zero);
            float2 b = new float2(2f, 0f);
            testSpline2D.AddControlPoint(b);

            Assert.AreEqual(2, testSpline2D.ControlPointCount);
            Assert.AreEqual(2, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);

            TestHelpers.CheckFloat2(a, testSpline2D.Get2DPointWorld(0f));
            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline2D.Get2DPointWorld(1f));
            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(1, SplinePoint.Point));

            //insert point
            float2 c = new float2(-2f, 0f);
            testSpline2D.InsertControlPoint(0, c);

            Assert.AreEqual(3, testSpline2D.ControlPointCount);
            Assert.AreEqual(3, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            TestHelpers.CheckFloat2(c, testSpline2D.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(1, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(2, SplinePoint.Point));
        }

        [Test]
        public void NoPoints()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            Assert.AreEqual(0, testSpline2D.ControlPointCount);

            TestHelpers.CheckFloat2(float2.zero, testSpline2D.Get2DPointWorld(0f));
            TestHelpers.CheckFloat2(float2.zero, testSpline2D.Get2DPointWorld(1f));
            TestHelpers.CheckFloat2(float2.zero, testSpline2D.Get2DPointWorld(0.5f));
        }

        [Test]
        public void SplineLengthRecalculation()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            Assert.AreEqual(0, testSpline2D.ControlPointCount);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);

            testSpline2D.AddControlPoint(new float2(0f, 0f));
            testSpline2D.AddControlPoint(new float2(1f, 1f));
            testSpline2D.AddControlPoint(new float2(2f, 2f));

            Assert.AreEqual(3, testSpline2D.ControlPointCount);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(3), testSpline2D.Times.Count);

            testSpline2D.RemoveControlPoint(0);
            testSpline2D.RemoveControlPoint(0);
            testSpline2D.RemoveControlPoint(0);

            Assert.AreEqual(0, testSpline2D.ControlPointCount);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(0), testSpline2D.Times.Count);
        }
        #endregion // Original tests


        [Test]
        public void ZigZagLength()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = new float2(0f, 0f);
            testSpline2D.AddControlPoint(a);
            float2 b = new float2(10f, 10f);
            testSpline2D.AddControlPoint(b);
            float2 c = new float2(0f, 20f);
            testSpline2D.AddControlPoint(c);
            float2 d = new float2(20f, 30f);
            testSpline2D.AddControlPoint(d);

            float aLength = math.distance(a, b) / 2f;
            float bLength = math.distance(b, c);
            float cLength = math.distance(c, d) / 2f;

            float minLength = (aLength + bLength + cLength) / 2f;
            Assert.Greater(testSpline2D.Length(), minLength);
        }

        [Test]
        public void ZigZagLength2()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = new float2(2f, 2f);
            testSpline2D.AddControlPoint(a);
            float2 b = new float2(3f, 0f);
            testSpline2D.AddControlPoint(b);
            float2 c = new float2(4f, 2f);
            testSpline2D.AddControlPoint(c);
            float2 d = new float2(5f, 0f);
            testSpline2D.AddControlPoint(d);

            float aLength = math.distance(a, b) / 2f;
            float bLength = math.distance(b, c);
            float cLength = math.distance(c, d) / 2f;

            float length = (aLength + bLength + cLength) / 2f;
            Assert.Greater(testSpline2D.Length(), length);
        }

        [Test]
        public void bSplineLength()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = new float2(0f, 0f);
            testSpline2D.AddControlPoint(a);
            float2 b = new float2(0f, 0.5f);
            testSpline2D.AddControlPoint(b);
            float2 c = new float2(0f, 1f);
            testSpline2D.AddControlPoint(c);

            Assert.AreEqual(0.5f, testSpline2D.Length());
        }
    }
}