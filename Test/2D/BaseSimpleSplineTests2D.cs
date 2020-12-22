using Crener.Spline.Common;
using Crener.Spline.Test.Helpers;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

namespace Crener.Spline.Test._2D
{
    public abstract class BaseSimpleSplineTests2D : SelfCleanUpTestSet
    {
        private static readonly float2 c_xOne = new float2(1f, 0f);
        private static readonly float2 c_xTwo = new float2(2f, 0f);
        
        /// <summary>
        /// Create a new instance of the spline
        /// </summary>
        protected abstract ISimpleTestSpline2D CreateNewSpline();

        /// <summary>
        /// Create a new spline and validates that it is ready for testing
        /// </summary>
        protected ISimpleTestSpline2D PrepareSpline()
        {
            ISimpleTestSpline2D spline2D = CreateNewSpline();
            Assert.IsNotNull(spline2D);

            TestHelpers.ClearSpline(spline2D);
            m_disposables.Add(spline2D);

            return spline2D;
        }

        [Test]
        public void Basic()
        {
            ISimpleTestSpline2D spline2D = PrepareSpline();
            Assert.NotNull(spline2D);
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
            Assert.AreEqual(1, testSpline2D.Times.Count);
            Assert.AreEqual(0f, testSpline2D.Length());

            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
        }

        [Test]
        public void Add2()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            testSpline2D.AddControlPoint(a);
            float2 b = c_xOne;
            testSpline2D.AddControlPoint(b);

            Assert.AreEqual(2, testSpline2D.ControlPointCount);
            Assert.AreEqual(testSpline2D.ExpectedControlPointCount(2), testSpline2D.ControlPoints.Count);
            Assert.AreEqual(2, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            Assert.AreEqual(1f, testSpline2D.Length());

            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(1, SplinePoint.Point));
        }

        [Test]
        public void Add3()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            testSpline2D.AddControlPoint(float2.zero);
            float2 b = c_xOne;
            testSpline2D.AddControlPoint(b);
            float2 c = c_xTwo;
            testSpline2D.AddControlPoint(c);

            Assert.AreEqual(3, testSpline2D.ControlPointCount);
            Assert.AreEqual(testSpline2D.ExpectedControlPointCount(3), testSpline2D.ControlPoints.Count);
            Assert.AreEqual(3, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            Assert.AreEqual(2f, testSpline2D.Length());

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
            float2 b = c_xOne;
            testSpline2D.AddControlPoint(b);
            float2 c = c_xTwo;
            testSpline2D.AddControlPoint(c);

            Assert.AreEqual(3, testSpline2D.ControlPointCount);
            Assert.AreEqual(3, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedControlPointCount(3), testSpline2D.ControlPoints.Count);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            Assert.AreEqual(2f, testSpline2D.Length());

            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(1, SplinePoint.Point));
            TestHelpers.CheckFloat2(c, testSpline2D.GetControlPoint(2, SplinePoint.Point));

            //Remove a point
            testSpline2D.RemoveControlPoint(1);

            Assert.AreEqual(2, testSpline2D.ControlPointCount);
            Assert.AreEqual(2, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedControlPointCount(2), testSpline2D.ControlPoints.Count);
            Assert.AreEqual(1, testSpline2D.Times.Count);
            Assert.AreEqual(2f, testSpline2D.Length());

            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(c, testSpline2D.GetControlPoint(1, SplinePoint.Point));
        }
        
        [Test]
        public void RemoveFromStart()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();
            
            float2 a = float2.zero;
            testSpline2D.AddControlPoint(float2.zero);
            float2 b = c_xOne;
            testSpline2D.AddControlPoint(b);

            Assert.AreEqual(2, testSpline2D.ControlPointCount);
            Assert.AreEqual(2, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedControlPointCount(2), testSpline2D.ControlPoints.Count);
            Assert.AreEqual(1, testSpline2D.Times.Count);
            Assert.AreEqual(1f, testSpline2D.Length());

            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(1, SplinePoint.Point));

            //Remove a point
            testSpline2D.RemoveControlPoint(0);

            Assert.AreEqual(1, testSpline2D.ControlPointCount);
            Assert.AreEqual(1, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedControlPointCount(1), testSpline2D.ControlPoints.Count);
            Assert.AreEqual(1, testSpline2D.Times.Count);
            Assert.AreEqual(0f, testSpline2D.Length());

            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(0, SplinePoint.Point));
        }

        [Test]
        public void RemoveFromEnd2Points()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            testSpline2D.AddControlPoint(float2.zero);
            float2 b = c_xOne;
            testSpline2D.AddControlPoint(b);

            Assert.AreEqual(2, testSpline2D.ControlPointCount);
            Assert.AreEqual(2, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedControlPointCount(2), testSpline2D.ControlPoints.Count);
            Assert.AreEqual(1, testSpline2D.Times.Count);
            Assert.AreEqual(1f, testSpline2D.Length());

            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(1, SplinePoint.Point));

            //Remove a point
            testSpline2D.RemoveControlPoint(1);

            Assert.AreEqual(1, testSpline2D.ControlPointCount);
            Assert.AreEqual(1, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedControlPointCount(1), testSpline2D.ControlPoints.Count);
            Assert.AreEqual(1, testSpline2D.Times.Count);
            Assert.AreEqual(0f, testSpline2D.Length());

            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
        }

        [Test]
        public void RemoveFromEnd3Points()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            float2 b = c_xOne;
            float2 c = new float2(10f, 0f);
            testSpline2D.AddControlPoint(a);
            testSpline2D.AddControlPoint(b);
            testSpline2D.AddControlPoint(c);

            Assert.AreEqual(3, testSpline2D.ControlPointCount);
            Assert.AreEqual(3, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedControlPointCount(3), testSpline2D.ControlPoints.Count);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            Assert.AreEqual(10f, testSpline2D.Length());

            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(1, SplinePoint.Point));
            TestHelpers.CheckFloat2(c, testSpline2D.GetControlPoint(2, SplinePoint.Point));

            //Remove a point
            testSpline2D.RemoveControlPoint(2);

            Assert.AreEqual(2, testSpline2D.ControlPointCount);
            Assert.AreEqual(2, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedControlPointCount(2), testSpline2D.ControlPoints.Count);
            Assert.AreEqual(1, testSpline2D.Times.Count);
            Assert.AreEqual(1f, testSpline2D.Length());

            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(1, SplinePoint.Point));
        }

        [Test]
        public void RemoveFromOutOfRange()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            testSpline2D.AddControlPoint(float2.zero);
            float2 b = c_xOne;
            testSpline2D.AddControlPoint(b);

            Assert.AreEqual(2, testSpline2D.ControlPointCount);
            Assert.AreEqual(2, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedControlPointCount(2), testSpline2D.ControlPoints.Count);
            Assert.AreEqual(1, testSpline2D.Times.Count);
            Assert.AreEqual(1f, testSpline2D.Length());

            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(1, SplinePoint.Point));

            //Remove a point
            testSpline2D.RemoveControlPoint(300);

            Assert.AreEqual(1, testSpline2D.ControlPointCount);
            Assert.AreEqual(1, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedControlPointCount(1), testSpline2D.ControlPoints.Count);
            Assert.AreEqual(1, testSpline2D.Times.Count);
            Assert.AreEqual(0f, testSpline2D.Length());

            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
        }

        [Test]
        public void Add3Remove2()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            testSpline2D.AddControlPoint(float2.zero);
            float2 b = c_xOne;
            testSpline2D.AddControlPoint(b);
            float2 c = c_xTwo;
            testSpline2D.AddControlPoint(c);

            Assert.AreEqual(3, testSpline2D.ControlPointCount);
            Assert.AreEqual(3, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            Assert.AreEqual(2f, testSpline2D.Length());

            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(1, SplinePoint.Point));
            TestHelpers.CheckFloat2(c, testSpline2D.GetControlPoint(2, SplinePoint.Point));

            // Remove a point
            testSpline2D.RemoveControlPoint(2);

            Assert.AreEqual(2, testSpline2D.ControlPointCount);
            Assert.AreEqual(2, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedControlPointCount(2), testSpline2D.ControlPoints.Count);
            Assert.AreEqual(1, testSpline2D.Times.Count);
            Assert.AreEqual(1f, testSpline2D.Length());

            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(1, SplinePoint.Point));
            
            // Remove another point
            testSpline2D.RemoveControlPoint(1);

            Assert.AreEqual(1, testSpline2D.ControlPointCount);
            Assert.AreEqual(1, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedControlPointCount(1), testSpline2D.ControlPoints.Count);
            Assert.AreEqual(1, testSpline2D.Times.Count);
            Assert.AreEqual(0f, testSpline2D.Length());

            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
        }

        [Test]
        public void RemoveFromEndOutOfRangeUnder()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            testSpline2D.AddControlPoint(float2.zero);
            float2 b = c_xOne;
            testSpline2D.AddControlPoint(b);

            Assert.AreEqual(2, testSpline2D.ControlPointCount);
            Assert.AreEqual(2, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedControlPointCount(2), testSpline2D.ControlPoints.Count);
            Assert.AreEqual(1, testSpline2D.Times.Count);
            Assert.AreEqual(1f, testSpline2D.Length());

            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(1, SplinePoint.Point));

            // Remove a point less than 0
            testSpline2D.RemoveControlPoint(-3);

            Assert.AreEqual(2, testSpline2D.ControlPointCount);
            Assert.AreEqual(2, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedControlPointCount(2), testSpline2D.ControlPoints.Count);
            Assert.AreEqual(1, testSpline2D.Times.Count);
            Assert.AreEqual(1f, testSpline2D.Length());

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
            float2 b = c_xOne;
            testSpline2D.AddControlPoint(b);

            Assert.AreEqual(2, testSpline2D.ControlPointCount);
            Assert.AreEqual(1f, testSpline2D.Length());

            Assert.AreEqual(1, testSpline2D.Times.Count);
            Assert.AreEqual(1f, testSpline2D.Times[0]);

            TestHelpers.CheckFloat2(new float2(0.5f, 0f), testSpline2D.Get2DPointWorld(0.5f));
        }

        [Test]
        public void Point2()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            testSpline2D.AddControlPoint(float2.zero);
            float2 b = c_xOne;
            testSpline2D.AddControlPoint(b);
            float2 c = c_xTwo;
            testSpline2D.AddControlPoint(c);

            Assert.AreEqual(3, testSpline2D.ControlPointCount);
            Assert.AreEqual(2f, testSpline2D.Length());

            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            //Assert.AreEqual(0.5f, testSpline.Times[0]);
            //Assert.AreEqual(1f, testSpline.Times[1]);

            TestHelpers.CheckFloat2(a, testSpline2D.Get2DPointWorld(0f));
            TestHelpers.CheckFloat2(new float2(1f, 0f), testSpline2D.Get2DPointWorld(0.5f));
            //TestHelpers.CheckFloat2(c * 0.77f, spline.GetPoint(0.77f)); // fails due to bezier point bunching issues
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
            float2 b = c_xOne;
            testSpline2D.AddControlPoint(b);

            Assert.AreEqual(2, testSpline2D.ControlPointCount);
            Assert.AreEqual(2, testSpline2D.Modes.Count);
            Assert.AreEqual(1, testSpline2D.Times.Count);
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
            float2 b = c_xOne;
            testSpline2D.AddControlPoint(b);
            float2 c = c_xTwo;
            testSpline2D.AddControlPoint(c);

            Assert.AreEqual(3, testSpline2D.ControlPointCount);
            Assert.AreEqual(3, testSpline2D.Modes.Count);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            TestHelpers.CheckFloat2(a, testSpline2D.Get2DPointWorld(0f));
            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline2D.Get2DPointWorld(0.5f));
            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(1, SplinePoint.Point));
            TestHelpers.CheckFloat2(c, testSpline2D.Get2DPointWorld(1f));
            TestHelpers.CheckFloat2(c, testSpline2D.GetControlPoint(2, SplinePoint.Point));

            //update 0 point position
            float2 a2 = new float2(0f, 1f);
            testSpline2D.UpdateControlPointLocal(0, a2, SplinePoint.Point);
            TestHelpers.CheckFloat2(a2, testSpline2D.Get2DPointWorld(0f));
            TestHelpers.CheckFloat2(a2, testSpline2D.GetControlPoint(0, SplinePoint.Point));

            //update 1 point position
            float2 b2 = new float2(1f, 1f);
            testSpline2D.UpdateControlPointLocal(1, b2, SplinePoint.Point);
            TestHelpers.CheckFloat2(b2, testSpline2D.GetControlPoint(1, SplinePoint.Point));

            //update 2 point position
            float2 c2 = new float2(2f, 1f);
            testSpline2D.UpdateControlPointLocal(2, c2, SplinePoint.Point);
            TestHelpers.CheckFloat2(c2, testSpline2D.Get2DPointWorld(1f));
            TestHelpers.CheckFloat2(c2, testSpline2D.GetControlPoint(2, SplinePoint.Point));
        }

        [Test]
        public void Update3()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            testSpline2D.AddControlPoint(a);
            float2 b = c_xOne;
            testSpline2D.AddControlPoint(b);
            float2 c = c_xTwo;
            testSpline2D.AddControlPoint(c);

            Assert.AreEqual(2f, testSpline2D.Length());

            //update 1 point position
            float2 b2 = new float2(1f, 2f);
            testSpline2D.UpdateControlPointLocal(1, b2, SplinePoint.Point);
            Assert.GreaterOrEqual(testSpline2D.Length(), 2f);
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
        public void InsertSecondFirst()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            testSpline2D.AddControlPoint(a);
            Assert.AreEqual(1, testSpline2D.ControlPointCount);
            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point), (float.Epsilon * 2f));
            
            float2 b = new float2(4);
            testSpline2D.InsertControlPoint(0, b);
            Assert.AreEqual(2, testSpline2D.ControlPointCount);
            Assert.AreEqual(2, testSpline2D.Modes.Count);
            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(0, SplinePoint.Point), (float.Epsilon * 2f));
            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(1, SplinePoint.Point), (float.Epsilon * 2f));
        }

        [Test]
        public void InsertWithOne()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            testSpline2D.AddControlPoint(float2.zero);

            Assert.AreEqual(1, testSpline2D.ControlPointCount);
            Assert.AreEqual(1, testSpline2D.Modes.Count);
            Assert.AreEqual(1, testSpline2D.Times.Count);
            TestHelpers.CheckFloat2(a, testSpline2D.Get2DPointWorld(0f));
            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));

            float2 b = new float2(10f, 0f);
            testSpline2D.InsertControlPoint(1000, b);

            TestHelpers.CheckFloat2(b, testSpline2D.Get2DPointWorld(1f));
            TestHelpers.CheckFloat2(b, testSpline2D.GetControlPoint(1, SplinePoint.Point));

            Assert.AreEqual(2, testSpline2D.ControlPointCount);
            Assert.AreEqual(2, testSpline2D.Modes.Count);
            Assert.AreEqual(1, testSpline2D.Times.Count);
        }

        [Test]
        public void InsertEmpty()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            Assert.AreEqual(0, testSpline2D.ControlPointCount);
            Assert.AreEqual(0, testSpline2D.Modes.Count);
            Assert.AreEqual(1, testSpline2D.Times.Count);

            float2 a = float2.zero;
            testSpline2D.InsertControlPoint(12, a);

            Assert.AreEqual(1, testSpline2D.ControlPointCount);
            Assert.AreEqual(1, testSpline2D.Modes.Count);
            Assert.AreEqual(1, testSpline2D.Times.Count);
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
            float2 b = c_xTwo;
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
            
            testSpline2D.AddControlPoint(new float2(0f,0f));
            testSpline2D.AddControlPoint(new float2(1f,1f));
            testSpline2D.AddControlPoint(new float2(2f,2f));
            
            Assert.AreEqual(3, testSpline2D.ControlPointCount);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(3), testSpline2D.Times.Count);
            
            testSpline2D.RemoveControlPoint(0);
            testSpline2D.RemoveControlPoint(0);
            testSpline2D.RemoveControlPoint(0);

            Assert.AreEqual(0, testSpline2D.ControlPointCount);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(0), testSpline2D.Times.Count);
        }

        /// <summary>
        /// Progress less than 0 should return first point
        /// </summary>
        [Test]
        public void ProgressUnder([NUnit.Framework.Range(0, 8)] int nodeAmount)
        {
            const float offsetX = 2f;
            const float offsetY = 2f;
            
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 first = float2.zero;
            for (int i = 0; i < nodeAmount; i++)
            {
                float2 pos = new float2(offsetX * (i + 1), offsetY * (i + 1));
                if(i == 0) first = pos;
                testSpline2D.AddControlPoint(pos);
            }
            
            Assert.AreEqual(nodeAmount, testSpline2D.ControlPointCount);
            
            Assert.AreEqual(first, testSpline2D.Get2DPointWorld(0f));
            Assert.AreEqual(first, testSpline2D.Get2DPointWorld(-0.5f));
            Assert.AreEqual(first, testSpline2D.Get2DPointWorld(-1f));
        }

        /// <summary>
        /// Progress greater than 0 should return first point
        /// </summary>
        [Test]
        public void ProgressOver([NUnit.Framework.Range(0, 8)] int nodeAmount)
        {
            const float offsetX = 2f;
            const float offsetY = 2f;
            
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 last = float2.zero;
            for (int i = 0; i < nodeAmount; i++)
            {
                float2 pos = new float2(offsetX * (i + 1), offsetY * (i + 1));
                last = pos;
                testSpline2D.AddControlPoint(pos);
            }
            
            Assert.AreEqual(nodeAmount, testSpline2D.ControlPointCount);
            
            Assert.AreEqual(last, testSpline2D.Get2DPointWorld(1f));
            Assert.AreEqual(last, testSpline2D.Get2DPointWorld(1.5f));
            Assert.AreEqual(last, testSpline2D.Get2DPointWorld(2f));
        }

        [Test]
        public void Translation()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();
            float3 move = new float3(10f, 0f, 10f);
            ((MonoBehaviour) testSpline2D).transform.position = move;

            Assert.AreEqual(0, testSpline2D.ControlPointCount);
            Assert.AreEqual(0, testSpline2D.Modes.Count);
            Assert.AreEqual(1, testSpline2D.Times.Count);

            float2 a = float2.zero;
            testSpline2D.InsertControlPoint(12, a);

            Assert.AreEqual(1, testSpline2D.ControlPointCount);
            Assert.AreEqual(1, testSpline2D.Modes.Count);
            Assert.AreEqual(1, testSpline2D.Times.Count);
            TestHelpers.CheckFloat2(a, testSpline2D.Get2DPointLocal(0f));
            TestHelpers.CheckFloat2(move.xy + a, testSpline2D.Get2DPointWorld(0f));
            TestHelpers.CheckFloat2(a, testSpline2D.GetControlPoint(0, SplinePoint.Point));
        }

        [Test]
        public void MultiMidPoint([NUnit.Framework.Range(1, 12)] int points)
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();
            
            for (int i = 0; i < points; i++)
            {
                testSpline2D.AddControlPoint(new float2(i));
            }
            
            // todo figure out how to get catmull to like this test set
            // catmull has issues with long straights
            Assume.That(testSpline2D.SplineDataType != SplineType.CatmullRom, "Catmull rom spline currently not supported by this test");

            Assert.AreEqual(points, testSpline2D.ControlPointCount);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);

            float2 point = testSpline2D.Get2DPointWorld(0.5f);
            TestHelpers.CheckFloat2(new float2((points - 1) / 2f), point);
        }

        [Test]
        public void MultiMidPointOffset([NUnit.Framework.Range(1, 12)] int points)
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();
            const float offset = 200f;

            for (int i = 0; i < points; i++)
            {
                testSpline2D.AddControlPoint(new float2(offset + i));
            }
            
            // todo figure out how to get catmull to like this test set
            // catmull has issues with long straights
            Assume.That(testSpline2D.SplineDataType != SplineType.CatmullRom, "Catmull rom spline currently not supported by this test");

            Assert.AreEqual(points, testSpline2D.ControlPointCount);
            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);

            float2 point = testSpline2D.Get2DPointWorld(0.5f);
            TestHelpers.CheckFloat2(new float2(offset + (points - 1) / 2f), point, 0.00005f);
        }
    }
}