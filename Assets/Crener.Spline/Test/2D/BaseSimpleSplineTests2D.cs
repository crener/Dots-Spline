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
        protected abstract ISimpleTestSpline CreateNewSpline();

        /// <summary>
        /// Create a new spline and validates that it is ready for testing
        /// </summary>
        protected ISimpleTestSpline PrepareSpline()
        {
            ISimpleTestSpline spline = CreateNewSpline();
            Assert.IsNotNull(spline);

            TestHelpers.ClearSpline(spline);
            m_disposables.Add(spline);

            return spline;
        }

        [Test]
        public void Basic()
        {
            ISimpleTestSpline spline = PrepareSpline();
            Assert.NotNull(spline);
        }

        [Test]
        public void Add1()
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 a = float2.zero;
            testSpline.AddControlPoint(a);

            Assert.AreEqual(1, testSpline.ControlPointCount);
            Assert.AreEqual(testSpline.ExpectedControlPointCount(1), testSpline.ControlPoints.Count);
            Assert.AreEqual(1, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(0f, testSpline.Length());

            TestHelpers.CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
        }

        [Test]
        public void Add2()
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 a = float2.zero;
            testSpline.AddControlPoint(a);
            float2 b = c_xOne;
            testSpline.AddControlPoint(b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(testSpline.ExpectedControlPointCount(2), testSpline.ControlPoints.Count);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            Assert.AreEqual(1f, testSpline.Length());

            TestHelpers.CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline.GetControlPoint(1, SplinePoint.Point));
        }

        [Test]
        public void Add3()
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 a = float2.zero;
            testSpline.AddControlPoint(float2.zero);
            float2 b = c_xOne;
            testSpline.AddControlPoint(b);
            float2 c = c_xTwo;
            testSpline.AddControlPoint(c);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(testSpline.ExpectedControlPointCount(3), testSpline.ControlPoints.Count);
            Assert.AreEqual(3, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            Assert.AreEqual(2f, testSpline.Length());

            TestHelpers.CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline.GetControlPoint(1, SplinePoint.Point));
            TestHelpers.CheckFloat2(c, testSpline.GetControlPoint(2, SplinePoint.Point));
        }

        [Test]
        public void Remove()
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 a = float2.zero;
            testSpline.AddControlPoint(float2.zero);
            float2 b = c_xOne;
            testSpline.AddControlPoint(b);
            float2 c = c_xTwo;
            testSpline.AddControlPoint(c);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(3, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedControlPointCount(3), testSpline.ControlPoints.Count);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            Assert.AreEqual(2f, testSpline.Length());

            TestHelpers.CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline.GetControlPoint(1, SplinePoint.Point));
            TestHelpers.CheckFloat2(c, testSpline.GetControlPoint(2, SplinePoint.Point));

            //Remove a point
            testSpline.RemoveControlPoint(1);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedControlPointCount(2), testSpline.ControlPoints.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(2f, testSpline.Length());

            TestHelpers.CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(c, testSpline.GetControlPoint(1, SplinePoint.Point));
        }
        
        [Test]
        public void RemoveFromStart()
        {
            ISimpleTestSpline testSpline = PrepareSpline();
            
            float2 a = float2.zero;
            testSpline.AddControlPoint(float2.zero);
            float2 b = c_xOne;
            testSpline.AddControlPoint(b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedControlPointCount(2), testSpline.ControlPoints.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(1f, testSpline.Length());

            TestHelpers.CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline.GetControlPoint(1, SplinePoint.Point));

            //Remove a point
            testSpline.RemoveControlPoint(0);

            Assert.AreEqual(1, testSpline.ControlPointCount);
            Assert.AreEqual(1, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedControlPointCount(1), testSpline.ControlPoints.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(0f, testSpline.Length());

            TestHelpers.CheckFloat2(b, testSpline.GetControlPoint(0, SplinePoint.Point));
        }

        [Test]
        public void RemoveFromEnd2Points()
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 a = float2.zero;
            testSpline.AddControlPoint(float2.zero);
            float2 b = c_xOne;
            testSpline.AddControlPoint(b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedControlPointCount(2), testSpline.ControlPoints.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(1f, testSpline.Length());

            TestHelpers.CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline.GetControlPoint(1, SplinePoint.Point));

            //Remove a point
            testSpline.RemoveControlPoint(1);

            Assert.AreEqual(1, testSpline.ControlPointCount);
            Assert.AreEqual(1, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedControlPointCount(1), testSpline.ControlPoints.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(0f, testSpline.Length());

            TestHelpers.CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
        }

        [Test]
        public void RemoveFromEnd3Points()
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 a = float2.zero;
            float2 b = c_xOne;
            float2 c = new float2(10f, 0f);
            testSpline.AddControlPoint(a);
            testSpline.AddControlPoint(b);
            testSpline.AddControlPoint(c);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(3, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedControlPointCount(3), testSpline.ControlPoints.Count);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            Assert.AreEqual(10f, testSpline.Length());

            TestHelpers.CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline.GetControlPoint(1, SplinePoint.Point));
            TestHelpers.CheckFloat2(c, testSpline.GetControlPoint(2, SplinePoint.Point));

            //Remove a point
            testSpline.RemoveControlPoint(2);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedControlPointCount(2), testSpline.ControlPoints.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(1f, testSpline.Length());

            TestHelpers.CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline.GetControlPoint(1, SplinePoint.Point));
        }

        [Test]
        public void RemoveFromOutOfRange()
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 a = float2.zero;
            testSpline.AddControlPoint(float2.zero);
            float2 b = c_xOne;
            testSpline.AddControlPoint(b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedControlPointCount(2), testSpline.ControlPoints.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(1f, testSpline.Length());

            TestHelpers.CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline.GetControlPoint(1, SplinePoint.Point));

            //Remove a point
            testSpline.RemoveControlPoint(300);

            Assert.AreEqual(1, testSpline.ControlPointCount);
            Assert.AreEqual(1, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedControlPointCount(1), testSpline.ControlPoints.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(0f, testSpline.Length());

            TestHelpers.CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
        }

        [Test]
        public void Add3Remove2()
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 a = float2.zero;
            testSpline.AddControlPoint(float2.zero);
            float2 b = c_xOne;
            testSpline.AddControlPoint(b);
            float2 c = c_xTwo;
            testSpline.AddControlPoint(c);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(3, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            Assert.AreEqual(2f, testSpline.Length());

            TestHelpers.CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline.GetControlPoint(1, SplinePoint.Point));
            TestHelpers.CheckFloat2(c, testSpline.GetControlPoint(2, SplinePoint.Point));

            // Remove a point
            testSpline.RemoveControlPoint(2);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedControlPointCount(2), testSpline.ControlPoints.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(1f, testSpline.Length());

            TestHelpers.CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline.GetControlPoint(1, SplinePoint.Point));
            
            // Remove another point
            testSpline.RemoveControlPoint(1);

            Assert.AreEqual(1, testSpline.ControlPointCount);
            Assert.AreEqual(1, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedControlPointCount(1), testSpline.ControlPoints.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(0f, testSpline.Length());

            TestHelpers.CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
        }

        [Test]
        public void RemoveFromEndOutOfRangeUnder()
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 a = float2.zero;
            testSpline.AddControlPoint(float2.zero);
            float2 b = c_xOne;
            testSpline.AddControlPoint(b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedControlPointCount(2), testSpline.ControlPoints.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(1f, testSpline.Length());

            TestHelpers.CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline.GetControlPoint(1, SplinePoint.Point));

            // Remove a point less than 0
            testSpline.RemoveControlPoint(-3);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedControlPointCount(2), testSpline.ControlPoints.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(1f, testSpline.Length());

            TestHelpers.CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline.GetControlPoint(1, SplinePoint.Point));
        }

        [Test]
        public void RemoveWhenEmpty()
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            Assert.AreEqual(0, testSpline.ControlPointCount, "spline should be empty");

            //Remove a point
            testSpline.RemoveControlPoint(0);
            Assert.AreEqual(0, testSpline.ControlPointCount, "spline should be empty");

            //Remove a point
            testSpline.RemoveControlPoint(1000);
            Assert.AreEqual(0, testSpline.ControlPointCount, "spline should be empty");
        }

        [Test]
        public void NoPoint()
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            Assert.AreEqual(0, testSpline.ControlPointCount);
            Assert.AreEqual(0f, testSpline.Length());

            TestHelpers.CheckFloat2(float2.zero, testSpline.Get2DPoint(0.5f));
        }

        [Test]
        public void Point()
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 a = float2.zero;
            testSpline.AddControlPoint(a);
            float2 b = c_xOne;
            testSpline.AddControlPoint(b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(1f, testSpline.Length());

            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(1f, testSpline.Times[0]);

            TestHelpers.CheckFloat2(new float2(0.5f, 0f), testSpline.Get2DPoint(0.5f));
        }

        [Test]
        public void Point2()
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 a = float2.zero;
            testSpline.AddControlPoint(float2.zero);
            float2 b = c_xOne;
            testSpline.AddControlPoint(b);
            float2 c = c_xTwo;
            testSpline.AddControlPoint(c);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(2f, testSpline.Length());

            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            //Assert.AreEqual(0.5f, testSpline.Times[0]);
            //Assert.AreEqual(1f, testSpline.Times[1]);

            TestHelpers.CheckFloat2(a, testSpline.Get2DPoint(0f));
            TestHelpers.CheckFloat2(new float2(1f, 0f), testSpline.Get2DPoint(0.5f));
            //TestHelpers.CheckFloat2(c * 0.77f, spline.GetPoint(0.77f)); // fails due to bezier point bunching issues
        }

        [Test]
        public void Point4()
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 a = new float2(3f, 3f);
            testSpline.AddControlPoint(a);

            Assert.AreEqual(1, testSpline.ControlPointCount);
            Assert.AreEqual(0f, testSpline.Length());

            TestHelpers.CheckFloat2(new float2(3f, 3f), testSpline.Get2DPoint(0.5f));
        }

        [Test]
        public void Update()
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 a = float2.zero;
            testSpline.AddControlPoint(float2.zero);
            float2 b = c_xOne;
            testSpline.AddControlPoint(b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            TestHelpers.CheckFloat2(a, testSpline.Get2DPoint(0f));
            TestHelpers.CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline.Get2DPoint(1f));
            TestHelpers.CheckFloat2(b, testSpline.GetControlPoint(1, SplinePoint.Point));

            //update 0 point position
            float2 a2 = new float2(-1f, -1f);
            testSpline.UpdateControlPoint(0, a2, SplinePoint.Point);

            TestHelpers.CheckFloat2(a2, testSpline.Get2DPoint(0f));
            TestHelpers.CheckFloat2(a2, testSpline.GetControlPoint(0, SplinePoint.Point));

            //update 1 point position
            float2 b2 = new float2(2f, 2f);
            testSpline.UpdateControlPoint(1, b2, SplinePoint.Point);

            TestHelpers.CheckFloat2(b2, testSpline.Get2DPoint(1f));
            TestHelpers.CheckFloat2(b2, testSpline.GetControlPoint(1, SplinePoint.Point));
        }

        [Test]
        public void Update2()
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 a = float2.zero;
            testSpline.AddControlPoint(float2.zero);
            float2 b = c_xOne;
            testSpline.AddControlPoint(b);
            float2 c = c_xTwo;
            testSpline.AddControlPoint(c);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(3, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            TestHelpers.CheckFloat2(a, testSpline.Get2DPoint(0f));
            TestHelpers.CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline.Get2DPoint(0.5f));
            TestHelpers.CheckFloat2(b, testSpline.GetControlPoint(1, SplinePoint.Point));
            TestHelpers.CheckFloat2(c, testSpline.Get2DPoint(1f));
            TestHelpers.CheckFloat2(c, testSpline.GetControlPoint(2, SplinePoint.Point));

            //update 0 point position
            float2 a2 = new float2(0f, 1f);
            testSpline.UpdateControlPoint(0, a2, SplinePoint.Point);
            TestHelpers.CheckFloat2(a2, testSpline.Get2DPoint(0f));
            TestHelpers.CheckFloat2(a2, testSpline.GetControlPoint(0, SplinePoint.Point));

            //update 1 point position
            float2 b2 = new float2(1f, 1f);
            testSpline.UpdateControlPoint(1, b2, SplinePoint.Point);
            TestHelpers.CheckFloat2(b2, testSpline.GetControlPoint(1, SplinePoint.Point));

            //update 2 point position
            float2 c2 = new float2(2f, 1f);
            testSpline.UpdateControlPoint(2, c2, SplinePoint.Point);
            TestHelpers.CheckFloat2(c2, testSpline.Get2DPoint(1f));
            TestHelpers.CheckFloat2(c2, testSpline.GetControlPoint(2, SplinePoint.Point));
        }

        [Test]
        public void Update3()
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 a = float2.zero;
            testSpline.AddControlPoint(a);
            float2 b = c_xOne;
            testSpline.AddControlPoint(b);
            float2 c = c_xTwo;
            testSpline.AddControlPoint(c);

            Assert.AreEqual(2f, testSpline.Length());

            //update 1 point position
            float2 b2 = new float2(1f, 2f);
            testSpline.UpdateControlPoint(1, b2, SplinePoint.Point);
            Assert.GreaterOrEqual(testSpline.Length(), 2f);
        }

        [Test]
        public void Insert()
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 a = float2.zero;
            testSpline.AddControlPoint(float2.zero);
            float2 b = new float2(10f, 0f);
            testSpline.AddControlPoint(b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            TestHelpers.CheckFloat2(a, testSpline.Get2DPoint(0f));
            TestHelpers.CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline.Get2DPoint(1f));
            TestHelpers.CheckFloat2(b, testSpline.GetControlPoint(1, SplinePoint.Point));

            //insert point
            float2 c = new float2(20f, 0f);
            testSpline.InsertControlPoint(1, c);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(3, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            TestHelpers.CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(c, testSpline.GetControlPoint(1, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline.GetControlPoint(2, SplinePoint.Point));
        }

        [Test]
        public void InsertSecondFirst()
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 a = float2.zero;
            testSpline.AddControlPoint(a);
            Assert.AreEqual(1, testSpline.ControlPointCount);
            TestHelpers.CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point), (float.Epsilon * 2f));
            
            float2 b = new float2(4);
            testSpline.InsertControlPoint(0, b);
            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            TestHelpers.CheckFloat2(b, testSpline.GetControlPoint(0, SplinePoint.Point), (float.Epsilon * 2f));
            TestHelpers.CheckFloat2(a, testSpline.GetControlPoint(1, SplinePoint.Point), (float.Epsilon * 2f));
        }

        [Test]
        public void InsertWithOne()
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 a = float2.zero;
            testSpline.AddControlPoint(float2.zero);

            Assert.AreEqual(1, testSpline.ControlPointCount);
            Assert.AreEqual(1, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            TestHelpers.CheckFloat2(a, testSpline.Get2DPoint(0f));
            TestHelpers.CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));

            float2 b = new float2(10f, 0f);
            testSpline.InsertControlPoint(1000, b);

            TestHelpers.CheckFloat2(b, testSpline.Get2DPoint(1f));
            TestHelpers.CheckFloat2(b, testSpline.GetControlPoint(1, SplinePoint.Point));

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
        }

        [Test]
        public void InsertEmpty()
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            Assert.AreEqual(0, testSpline.ControlPointCount);
            Assert.AreEqual(0, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.Times.Count);

            float2 a = float2.zero;
            testSpline.InsertControlPoint(12, a);

            Assert.AreEqual(1, testSpline.ControlPointCount);
            Assert.AreEqual(1, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            TestHelpers.CheckFloat2(a, testSpline.Get2DPoint(0f));
            TestHelpers.CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
        }

        [Test]
        public void InsertAtEnd()
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 a = float2.zero;
            float2 b = new float2(10f, 0f);
            float2 c = new float2(20f, 0f);

            testSpline.AddControlPoint(a);
            testSpline.AddControlPoint(b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            TestHelpers.CheckFloat2(a, testSpline.Get2DPoint(0f));
            TestHelpers.CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline.Get2DPoint(1f));
            TestHelpers.CheckFloat2(b, testSpline.GetControlPoint(1, SplinePoint.Point));

            //insert point
            testSpline.InsertControlPoint(2, c);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(3, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            TestHelpers.CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline.GetControlPoint(1, SplinePoint.Point));
            TestHelpers.CheckFloat2(c, testSpline.GetControlPoint(2, SplinePoint.Point));
        }

        [Test]
        public void InsertAtStart()
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 a = float2.zero;
            testSpline.AddControlPoint(float2.zero);
            float2 b = c_xTwo;
            testSpline.AddControlPoint(b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);

            TestHelpers.CheckFloat2(a, testSpline.Get2DPoint(0f));
            TestHelpers.CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline.Get2DPoint(1f));
            TestHelpers.CheckFloat2(b, testSpline.GetControlPoint(1, SplinePoint.Point));

            //insert point
            float2 c = new float2(-2f, 0f);
            testSpline.InsertControlPoint(0, c);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(3, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            TestHelpers.CheckFloat2(c, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat2(a, testSpline.GetControlPoint(1, SplinePoint.Point));
            TestHelpers.CheckFloat2(b, testSpline.GetControlPoint(2, SplinePoint.Point));
        }

        [Test]
        public void NoPoints()
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            Assert.AreEqual(0, testSpline.ControlPointCount);

            TestHelpers.CheckFloat2(float2.zero, testSpline.Get2DPoint(0f));
            TestHelpers.CheckFloat2(float2.zero, testSpline.Get2DPoint(1f));
            TestHelpers.CheckFloat2(float2.zero, testSpline.Get2DPoint(0.5f));
        }

        [Test]
        public void SplineLengthRecalculation()
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            Assert.AreEqual(0, testSpline.ControlPointCount);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            
            testSpline.AddControlPoint(new float2(0f,0f));
            testSpline.AddControlPoint(new float2(1f,1f));
            testSpline.AddControlPoint(new float2(2f,2f));
            
            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(testSpline.ExpectedTimeCount(3), testSpline.Times.Count);
            
            testSpline.RemoveControlPoint(0);
            testSpline.RemoveControlPoint(0);
            testSpline.RemoveControlPoint(0);

            Assert.AreEqual(0, testSpline.ControlPointCount);
            Assert.AreEqual(testSpline.ExpectedTimeCount(0), testSpline.Times.Count);
        }

        /// <summary>
        /// Progress less than 0 should return first point
        /// </summary>
        [Test]
        public void ProgressUnder([NUnit.Framework.Range(1, 8)] int nodeAmount)
        {
            const float offsetX = 2f;
            const float offsetY = 2f;
            
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 first = float2.zero;
            for (int i = 0; i < nodeAmount; i++)
            {
                float2 pos = new float2(offsetX * (i + 1), offsetY * (i + 1));
                if(i == 0) first = pos;
                testSpline.AddControlPoint(pos);
            }
            
            Assert.AreNotEqual(float2.zero, first, "Test likely misconfigured as the expects location has the default value");
            Assert.AreEqual(nodeAmount, testSpline.ControlPointCount);
            
            Assert.AreEqual(first, testSpline.Get2DPoint(0f));
            Assert.AreEqual(first, testSpline.Get2DPoint(-0.5f));
            Assert.AreEqual(first, testSpline.Get2DPoint(-1f));
        }

        /// <summary>
        /// Progress greater than 0 should return first point
        /// </summary>
        [Test]
        public void ProgressOver([NUnit.Framework.Range(1, 8)] int nodeAmount)
        {
            const float offsetX = 2f;
            const float offsetY = 2f;
            
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 last = float2.zero;
            for (int i = 0; i < nodeAmount; i++)
            {
                float2 pos = new float2(offsetX * (i + 1), offsetY * (i + 1));
                last = pos;
                testSpline.AddControlPoint(pos);
            }
            
            Assert.AreNotEqual(float2.zero, last, "Test likely misconfigured as the expects location has the default value");
            Assert.AreEqual(nodeAmount, testSpline.ControlPointCount);
            
            Assert.AreEqual(last, testSpline.Get2DPoint(1f));
            Assert.AreEqual(last, testSpline.Get2DPoint(1.5f));
            Assert.AreEqual(last, testSpline.Get2DPoint(2f));
        }

        [Test]
        public void Translation()
        {
            ISimpleTestSpline testSpline = PrepareSpline();
            float3 move = new float3(10f, 0f, 10f);
            ((MonoBehaviour) testSpline).transform.position = move;

            Assert.AreEqual(0, testSpline.ControlPointCount);
            Assert.AreEqual(0, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.Times.Count);

            float2 a = float2.zero;
            testSpline.InsertControlPoint(12, a);

            Assert.AreEqual(1, testSpline.ControlPointCount);
            Assert.AreEqual(1, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            TestHelpers.CheckFloat2(new float2(10f, 0f), testSpline.Get2DPoint(0f));
            TestHelpers.CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
        }

        [Test]
        public void MultiMidPoint([NUnit.Framework.Range(1, 12)] int points)
        {
            ISimpleTestSpline testSpline = PrepareSpline();
            
            for (int i = 0; i < points; i++)
            {
                testSpline.AddControlPoint(new float2(i));
            }
            
            // todo figure out how to get catmull to like this test set
            // catmull has issues with long straights
            Assume.That(testSpline.SplineDataType != SplineType.CatmullRom);

            Assert.AreEqual(points, testSpline.ControlPointCount);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);

            float2 point = testSpline.Get2DPoint(0.5f);
            TestHelpers.CheckFloat2(new float2((points - 1) / 2f), point);
        }

        [Test]
        public void MultiMidPointOffset([NUnit.Framework.Range(1, 12)] int points)
        {
            ISimpleTestSpline testSpline = PrepareSpline();
            const float offset = 200f;

            for (int i = 0; i < points; i++)
            {
                testSpline.AddControlPoint(new float2(offset + i));
            }
            
            // todo figure out how to get catmull to like this test set
            // catmull has issues with long straights
            Assume.That(testSpline.SplineDataType != SplineType.CatmullRom);

            Assert.AreEqual(points, testSpline.ControlPointCount);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);

            float2 point = testSpline.Get2DPoint(0.5f);
            TestHelpers.CheckFloat2(new float2(offset + (points - 1) / 2f), point, 0.00005f);
        }
    }
}