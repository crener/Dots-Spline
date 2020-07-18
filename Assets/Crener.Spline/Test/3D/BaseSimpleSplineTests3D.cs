using System.IO;
using System.Security.Cryptography.X509Certificates;
using Crener.Spline.CatmullRom;
using Crener.Spline.Common;
using Crener.Spline.Test.Helpers;
using NUnit.Framework;
using Unity.Mathematics;

namespace Crener.Spline.Test._3D
{
    public abstract class BaseSimpleSplineTests3D : SelfCleanUpTestSet
    {
        private static readonly float3 c_xOne = new float3(1f, 0f, 0f);
        private static readonly float3 c_xTwo = new float3(2f, 0f, 0f);


        /// <summary>
        /// Create a new instance of the spline
        /// </summary>
        protected abstract ISimpleTestSpline3D CreateNewSpline();

        /// <summary>
        /// Create a new spline and validates that it is ready for testing
        /// </summary>
        protected ISimpleTestSpline3D PrepareSpline()
        {
            ISimpleTestSpline3D spline = CreateNewSpline();
            Assert.IsNotNull(spline);

            TestHelpers.ClearSpline(spline);
            m_disposables.Add(spline);

            return spline;
        }

        [Test]
        public void Basic()
        {
            ISimpleTestSpline3D spline = PrepareSpline();
            Assert.NotNull(spline);
        }

        [Test]
        public void Add1()
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

            float3 a = float3.zero;
            testSpline.AddControlPoint(a);

            Assert.AreEqual(1, testSpline.ControlPointCount);
            Assert.AreEqual(testSpline.ExpectedControlPointCount(1), testSpline.ControlPoints.Count);
            Assert.AreEqual(1, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(0f, testSpline.Length());

            TestHelpers.CheckFloat3(a, testSpline.GetControlPoint(0, SplinePoint.Point));
        }

        [Test]
        public void Add2()
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

            float3 a = float3.zero;
            testSpline.AddControlPoint(a);
            float3 b = c_xOne;
            testSpline.AddControlPoint(b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(testSpline.ExpectedControlPointCount(2), testSpline.ControlPoints.Count);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            Assert.AreEqual(1f, testSpline.Length());

            TestHelpers.CheckFloat3(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat3(b, testSpline.GetControlPoint(1, SplinePoint.Point));
        }

        [Test]
        public void Add3()
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

            float3 a = float3.zero;
            testSpline.AddControlPoint(float3.zero);
            float3 b = c_xOne;
            testSpline.AddControlPoint(b);
            float3 c = c_xTwo;
            testSpline.AddControlPoint(c);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(testSpline.ExpectedControlPointCount(3), testSpline.ControlPoints.Count);
            Assert.AreEqual(3, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            Assert.AreEqual(2f, testSpline.Length());

            TestHelpers.CheckFloat3(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat3(b, testSpline.GetControlPoint(1, SplinePoint.Point));
            TestHelpers.CheckFloat3(c, testSpline.GetControlPoint(2, SplinePoint.Point));
        }

        [Test]
        public void Remove()
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

            float3 a = float3.zero;
            testSpline.AddControlPoint(float3.zero);
            float3 b = c_xOne;
            testSpline.AddControlPoint(b);
            float3 c = c_xTwo;
            testSpline.AddControlPoint(c);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(3, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedControlPointCount(3), testSpline.ControlPoints.Count);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            Assert.AreEqual(2f, testSpline.Length());

            TestHelpers.CheckFloat3(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat3(b, testSpline.GetControlPoint(1, SplinePoint.Point));
            TestHelpers.CheckFloat3(c, testSpline.GetControlPoint(2, SplinePoint.Point));

            //Remove a point
            testSpline.RemoveControlPoint(1);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedControlPointCount(2), testSpline.ControlPoints.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(2f, testSpline.Length());

            TestHelpers.CheckFloat3(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat3(c, testSpline.GetControlPoint(1, SplinePoint.Point));
        }

        [Test]
        public void RemoveFromStart()
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

            float3 a = float3.zero;
            testSpline.AddControlPoint(float3.zero);
            float3 b = c_xOne;
            testSpline.AddControlPoint(b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedControlPointCount(2), testSpline.ControlPoints.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(1f, testSpline.Length());

            TestHelpers.CheckFloat3(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat3(b, testSpline.GetControlPoint(1, SplinePoint.Point));

            //Remove a point
            testSpline.RemoveControlPoint(0);

            Assert.AreEqual(1, testSpline.ControlPointCount);
            Assert.AreEqual(1, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedControlPointCount(1), testSpline.ControlPoints.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(0f, testSpline.Length());

            TestHelpers.CheckFloat3(b, testSpline.GetControlPoint(0, SplinePoint.Point));
        }

        [Test]
        public void RemoveFromEnd2Points()
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

            float3 a = float3.zero;
            testSpline.AddControlPoint(float3.zero);
            float3 b = c_xOne;
            testSpline.AddControlPoint(b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedControlPointCount(2), testSpline.ControlPoints.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(1f, testSpline.Length());

            TestHelpers.CheckFloat3(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat3(b, testSpline.GetControlPoint(1, SplinePoint.Point));

            //Remove a point
            testSpline.RemoveControlPoint(1);

            Assert.AreEqual(1, testSpline.ControlPointCount);
            Assert.AreEqual(1, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedControlPointCount(1), testSpline.ControlPoints.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(0f, testSpline.Length());

            TestHelpers.CheckFloat3(a, testSpline.GetControlPoint(0, SplinePoint.Point));
        }

        [Test]
        public void RemoveFromEnd3Points()
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

            float3 a = float3.zero;
            float3 b = c_xOne;
            float3 c = new float3(10f, 0f, 0f);
            testSpline.AddControlPoint(a);
            testSpline.AddControlPoint(b);
            testSpline.AddControlPoint(c);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(3, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedControlPointCount(3), testSpline.ControlPoints.Count);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            Assert.AreEqual(10f, testSpline.Length());

            TestHelpers.CheckFloat3(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat3(b, testSpline.GetControlPoint(1, SplinePoint.Point));
            TestHelpers.CheckFloat3(c, testSpline.GetControlPoint(2, SplinePoint.Point));

            //Remove a point
            testSpline.RemoveControlPoint(2);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedControlPointCount(2), testSpline.ControlPoints.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(1f, testSpline.Length());

            TestHelpers.CheckFloat3(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat3(b, testSpline.GetControlPoint(1, SplinePoint.Point));
        }

        [Test]
        public void RemoveFromOutOfRange()
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

            float3 a = float3.zero;
            testSpline.AddControlPoint(float3.zero);
            float3 b = c_xOne;
            testSpline.AddControlPoint(b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedControlPointCount(2), testSpline.ControlPoints.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(1f, testSpline.Length());

            TestHelpers.CheckFloat3(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat3(b, testSpline.GetControlPoint(1, SplinePoint.Point));

            //Remove a point
            testSpline.RemoveControlPoint(300);

            Assert.AreEqual(1, testSpline.ControlPointCount);
            Assert.AreEqual(1, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedControlPointCount(1), testSpline.ControlPoints.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(0f, testSpline.Length());

            TestHelpers.CheckFloat3(a, testSpline.GetControlPoint(0, SplinePoint.Point));
        }

        [Test]
        public void Add3Remove2()
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

            float3 a = float3.zero;
            testSpline.AddControlPoint(float3.zero);
            float3 b = c_xOne;
            testSpline.AddControlPoint(b);
            float3 c = c_xTwo;
            testSpline.AddControlPoint(c);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(3, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            Assert.AreEqual(2f, testSpline.Length());

            TestHelpers.CheckFloat3(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat3(b, testSpline.GetControlPoint(1, SplinePoint.Point));
            TestHelpers.CheckFloat3(c, testSpline.GetControlPoint(2, SplinePoint.Point));

            // Remove a point
            testSpline.RemoveControlPoint(2);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedControlPointCount(2), testSpline.ControlPoints.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(1f, testSpline.Length());

            TestHelpers.CheckFloat3(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat3(b, testSpline.GetControlPoint(1, SplinePoint.Point));

            // Remove another point
            testSpline.RemoveControlPoint(1);

            Assert.AreEqual(1, testSpline.ControlPointCount);
            Assert.AreEqual(1, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedControlPointCount(1), testSpline.ControlPoints.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(0f, testSpline.Length());

            TestHelpers.CheckFloat3(a, testSpline.GetControlPoint(0, SplinePoint.Point));
        }

        [Test]
        public void RemoveFromEndOutOfRangeUnder()
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

            float3 a = float3.zero;
            testSpline.AddControlPoint(float3.zero);
            float3 b = c_xOne;
            testSpline.AddControlPoint(b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedControlPointCount(2), testSpline.ControlPoints.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(1f, testSpline.Length());

            TestHelpers.CheckFloat3(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat3(b, testSpline.GetControlPoint(1, SplinePoint.Point));

            // Remove a point less than 0
            testSpline.RemoveControlPoint(-3);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedControlPointCount(2), testSpline.ControlPoints.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(1f, testSpline.Length());

            TestHelpers.CheckFloat3(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat3(b, testSpline.GetControlPoint(1, SplinePoint.Point));
        }

        [Test]
        public void RemoveWhenEmpty()
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

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
            ISimpleTestSpline3D testSpline = PrepareSpline();

            Assert.AreEqual(0, testSpline.ControlPointCount);
            Assert.AreEqual(0f, testSpline.Length());

            TestHelpers.CheckFloat3(float3.zero, testSpline.GetPoint(0.5f));
        }

        [Test]
        public void Point()
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

            float3 a = float3.zero;
            testSpline.AddControlPoint(a);
            float3 b = c_xOne;
            testSpline.AddControlPoint(b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(1f, testSpline.Length());

            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(1f, testSpline.Times[0]);

            TestHelpers.CheckFloat3(new float3(0.5f, 0f, 0f), testSpline.GetPoint(0.5f));
        }

        [Test]
        public void Point2()
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

            float3 a = float3.zero;
            testSpline.AddControlPoint(a);
            float3 b = c_xOne;
            testSpline.AddControlPoint(b);
            float3 c = c_xTwo;
            testSpline.AddControlPoint(c);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(2f, testSpline.Length());

            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);

            TestHelpers.CheckFloat3(a, testSpline.GetPoint(0f));
            TestHelpers.CheckFloat3(new float3(1f, 0f, 0f), testSpline.GetPoint(0.5f));
            //TestHelpers.CheckFloat3(c * 0.77f, spline.GetPoint(0.77f)); // fails due to bezier point bunching issues
        }

        [Test]
        public void Point4()
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

            float3 a = new float3(3f, 3f, 1f);
            testSpline.AddControlPoint(a);

            Assert.AreEqual(1, testSpline.ControlPointCount);
            Assert.AreEqual(0f, testSpline.Length());

            TestHelpers.CheckFloat3(new float3(3f, 3f, 1f), testSpline.GetPoint(0.5f));
        }

        [Test]
        public void Update()
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

            float3 a = float3.zero;
            testSpline.AddControlPoint(float3.zero);
            float3 b = c_xOne;
            testSpline.AddControlPoint(b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            TestHelpers.CheckFloat3(a, testSpline.GetPoint(0f));
            TestHelpers.CheckFloat3(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat3(b, testSpline.GetPoint(1f));
            TestHelpers.CheckFloat3(b, testSpline.GetControlPoint(1, SplinePoint.Point));

            //update 0 point position
            float3 a2 = new float3(-1f, -1f, 1f);
            testSpline.UpdateControlPoint(0, a2, SplinePoint.Point);

            TestHelpers.CheckFloat3(a2, testSpline.GetPoint(0f));
            TestHelpers.CheckFloat3(a2, testSpline.GetControlPoint(0, SplinePoint.Point));

            //update 1 point position
            float3 b2 = new float3(2f, 2f, 1f);
            testSpline.UpdateControlPoint(1, b2, SplinePoint.Point);

            TestHelpers.CheckFloat3(b2, testSpline.GetPoint(1f));
            TestHelpers.CheckFloat3(b2, testSpline.GetControlPoint(1, SplinePoint.Point));
        }

        [Test]
        public void Update2()
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

            float3 a = float3.zero;
            testSpline.AddControlPoint(float3.zero);
            float3 b = c_xOne;
            testSpline.AddControlPoint(b);
            float3 c = c_xTwo;
            testSpline.AddControlPoint(c);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(3, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            TestHelpers.CheckFloat3(a, testSpline.GetPoint(0f));
            TestHelpers.CheckFloat3(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat3(b, testSpline.GetPoint(0.5f));
            TestHelpers.CheckFloat3(b, testSpline.GetControlPoint(1, SplinePoint.Point));
            TestHelpers.CheckFloat3(c, testSpline.GetPoint(1f));
            TestHelpers.CheckFloat3(c, testSpline.GetControlPoint(2, SplinePoint.Point));

            //update 0 point position
            float3 a2 = new float3(0f, 1f, 1f);
            testSpline.UpdateControlPoint(0, a2, SplinePoint.Point);
            TestHelpers.CheckFloat3(a2, testSpline.GetPoint(0f));
            TestHelpers.CheckFloat3(a2, testSpline.GetControlPoint(0, SplinePoint.Point));

            //update 1 point position
            float3 b2 = new float3(1f, 1f, 1f);
            testSpline.UpdateControlPoint(1, b2, SplinePoint.Point);
            TestHelpers.CheckFloat3(b2, testSpline.GetControlPoint(1, SplinePoint.Point));

            //update 2 point position
            float3 c2 = new float3(2f, 1f, 1f);
            testSpline.UpdateControlPoint(2, c2, SplinePoint.Point);
            TestHelpers.CheckFloat3(c2, testSpline.GetPoint(1f));
            TestHelpers.CheckFloat3(c2, testSpline.GetControlPoint(2, SplinePoint.Point));
        }

        [Test]
        public void Update3()
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

            float3 a = float3.zero;
            testSpline.AddControlPoint(a);
            float3 b = c_xOne;
            testSpline.AddControlPoint(b);
            float3 c = c_xTwo;
            testSpline.AddControlPoint(c);

            Assert.AreEqual(2f, testSpline.Length());

            //update 1 point position
            float3 b2 = new float3(1f, 2f, 1f);
            testSpline.UpdateControlPoint(1, b2, SplinePoint.Point);
            Assert.GreaterOrEqual(testSpline.Length(), 2f);
        }

        [Test]
        public void Insert()
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

            float3 a = float3.zero;
            testSpline.AddControlPoint(float3.zero);
            float3 b = new float3(10f, 0f, 1f);
            testSpline.AddControlPoint(b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            TestHelpers.CheckFloat3(a, testSpline.GetPoint(0f));
            TestHelpers.CheckFloat3(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat3(b, testSpline.GetPoint(1f));
            TestHelpers.CheckFloat3(b, testSpline.GetControlPoint(1, SplinePoint.Point));

            //insert point
            float3 c = new float3(20f, 0f, 1f);
            testSpline.InsertControlPoint(1, c);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(3, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            TestHelpers.CheckFloat3(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat3(c, testSpline.GetControlPoint(1, SplinePoint.Point));
            TestHelpers.CheckFloat3(b, testSpline.GetControlPoint(2, SplinePoint.Point));
        }

        [Test]
        public void InsertSecondFirst()
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

            float3 a = float3.zero;
            testSpline.AddControlPoint(a);
            Assert.AreEqual(1, testSpline.ControlPointCount);
            TestHelpers.CheckFloat3(a, testSpline.GetControlPoint(0, SplinePoint.Point), (float.Epsilon * 2f));

            float3 b = new float3(4);
            testSpline.InsertControlPoint(0, b);
            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            TestHelpers.CheckFloat3(b, testSpline.GetControlPoint(0, SplinePoint.Point), (float.Epsilon * 2f));
            TestHelpers.CheckFloat3(a, testSpline.GetControlPoint(1, SplinePoint.Point), (float.Epsilon * 2f));
        }

        [Test]
        public void InsertWithOne()
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

            float3 a = float3.zero;
            testSpline.AddControlPoint(float3.zero);

            Assert.AreEqual(1, testSpline.ControlPointCount);
            Assert.AreEqual(1, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            TestHelpers.CheckFloat3(a, testSpline.GetPoint(0f));
            TestHelpers.CheckFloat3(a, testSpline.GetControlPoint(0, SplinePoint.Point));

            float3 b = new float3(10f, 0f, 1f);
            testSpline.InsertControlPoint(1000, b);

            TestHelpers.CheckFloat3(b, testSpline.GetPoint(1f));
            TestHelpers.CheckFloat3(b, testSpline.GetControlPoint(1, SplinePoint.Point));

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
        }

        [Test]
        public void InsertEmpty()
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

            Assert.AreEqual(0, testSpline.ControlPointCount);
            Assert.AreEqual(0, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.Times.Count);

            float3 a = float3.zero;
            testSpline.InsertControlPoint(12, a);

            Assert.AreEqual(1, testSpline.ControlPointCount);
            Assert.AreEqual(1, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            TestHelpers.CheckFloat3(a, testSpline.GetPoint(0f));
            TestHelpers.CheckFloat3(a, testSpline.GetControlPoint(0, SplinePoint.Point));
        }

        [Test]
        public void InsertAtEnd()
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

            float3 a = float3.zero;
            float3 b = new float3(10f, 0f, 1f);
            float3 c = new float3(20f, 0f, 1f);

            testSpline.AddControlPoint(a);
            testSpline.AddControlPoint(b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            TestHelpers.CheckFloat3(a, testSpline.GetPoint(0f));
            TestHelpers.CheckFloat3(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat3(b, testSpline.GetPoint(1f));
            TestHelpers.CheckFloat3(b, testSpline.GetControlPoint(1, SplinePoint.Point));

            //insert point
            testSpline.InsertControlPoint(2, c);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(3, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            TestHelpers.CheckFloat3(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat3(b, testSpline.GetControlPoint(1, SplinePoint.Point));
            TestHelpers.CheckFloat3(c, testSpline.GetControlPoint(2, SplinePoint.Point));
        }

        [Test]
        public void InsertAtStart()
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

            float3 a = float3.zero;
            testSpline.AddControlPoint(float3.zero);
            float3 b = c_xTwo;
            testSpline.AddControlPoint(b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);

            TestHelpers.CheckFloat3(a, testSpline.GetPoint(0f));
            TestHelpers.CheckFloat3(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat3(b, testSpline.GetPoint(1f));
            TestHelpers.CheckFloat3(b, testSpline.GetControlPoint(1, SplinePoint.Point));

            //insert point
            float3 c = new float3(-2f, 0f, 1f);
            testSpline.InsertControlPoint(0, c);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(3, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            TestHelpers.CheckFloat3(c, testSpline.GetControlPoint(0, SplinePoint.Point));
            TestHelpers.CheckFloat3(a, testSpline.GetControlPoint(1, SplinePoint.Point));
            TestHelpers.CheckFloat3(b, testSpline.GetControlPoint(2, SplinePoint.Point));
        }

        [Test]
        public void NoPoints()
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

            Assert.AreEqual(0, testSpline.ControlPointCount);

            TestHelpers.CheckFloat3(float3.zero, testSpline.GetPoint(0f));
            TestHelpers.CheckFloat3(float3.zero, testSpline.GetPoint(1f));
            TestHelpers.CheckFloat3(float3.zero, testSpline.GetPoint(0.5f));
        }

        [Test]
        public void SplineLengthRecalculation()
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

            Assert.AreEqual(0, testSpline.ControlPointCount);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);

            testSpline.AddControlPoint(new float3(0f, 0f, 1f));
            testSpline.AddControlPoint(new float3(1f, 1f, 1f));
            testSpline.AddControlPoint(new float3(2f, 2f, 1f));

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
        public void ProgressUnder([Range(1, 8)] int nodeAmount)
        {
            const float offsetX = 2f;
            const float offsetY = 2f;
            const float offsetZ = 2f;

            ISimpleTestSpline3D testSpline = PrepareSpline();

            float3 first = float3.zero;
            for (int i = 0; i < nodeAmount; i++)
            {
                float3 pos = new float3(offsetX * (i + 1), offsetY * (i + 1), offsetZ * (i + 1));
                if(i == 0) first = pos;
                testSpline.AddControlPoint(pos);
            }

            Assert.AreNotEqual(float3.zero, first, "Test likely misconfigured as the expects location has the default value");
            Assert.AreEqual(nodeAmount, testSpline.ControlPointCount);

            Assert.AreEqual(first, testSpline.GetPoint(0f));
            Assert.AreEqual(first, testSpline.GetPoint(-0.5f));
            Assert.AreEqual(first, testSpline.GetPoint(-1f));
        }

        /// <summary>
        /// Progress greater than 0 should return first point
        /// </summary>
        [Test]
        public void ProgressOver([Range(1, 8)] int nodeAmount)
        {
            const float offsetX = 2f;
            const float offsetY = 2f;
            const float offsetZ = 2f;

            ISimpleTestSpline3D testSpline = PrepareSpline();

            float3 last = float3.zero;
            for (int i = 0; i < nodeAmount; i++)
            {
                float3 pos = new float3(offsetX * (i + 1), offsetY * (i + 1), offsetZ * (i + 1));
                last = pos;
                testSpline.AddControlPoint(pos);
            }

            Assert.AreNotEqual(float3.zero, last, "Test likely misconfigured as the expects location has the default value");
            Assert.AreEqual(nodeAmount, testSpline.ControlPointCount);

            Assert.AreEqual(last, testSpline.GetPoint(1f));
            Assert.AreEqual(last, testSpline.GetPoint(1.5f));
            Assert.AreEqual(last, testSpline.GetPoint(2f));
        }

        [Test]
        public void MultiMidPoint([Range(1, 12)] int points)
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();

            for (int i = 0; i < points; i++)
            {
                testSpline.AddControlPoint(new float3(i));
            }

            // todo figure out how to get catmull to like this test set
            // catmull has issues with long straights
            Assume.That(testSpline.SplineDataType != SplineType.CatmullRom);

            Assert.AreEqual(points, testSpline.ControlPointCount);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);

            float3 point = testSpline.GetPoint(0.5f);
            TestHelpers.CheckFloat3(new float3((points - 1) / 2f), point);
        }

        [Test]
        public void MultiMidPointOffset([Range(1, 12)] int points)
        {
            ISimpleTestSpline3D testSpline = PrepareSpline();
            const float offset = 200f;

            for (int i = 0; i < points; i++)
            {
                testSpline.AddControlPoint(new float3(offset + i));
            }

            // todo figure out how to get catmull to like this test set
            // catmull has issues with long straights
            Assume.That(testSpline.SplineDataType != SplineType.CatmullRom);

            Assert.AreEqual(points, testSpline.ControlPointCount);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);

            float3 point = testSpline.GetPoint(0.5f);
            TestHelpers.CheckFloat3(new float3(offset + (points - 1) / 2f), point, 0.00005f);
        }
    }
}