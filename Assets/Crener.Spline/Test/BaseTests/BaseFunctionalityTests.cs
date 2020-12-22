using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Crener.Spline.Test.BaseTests.TransferableTestBases;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

namespace Crener.Spline.Test.BaseTests
{
    public abstract class BaseFunctionalityTests : TransferableTestSet<ITestSpline>
    {
        private static readonly float3 s_xOne = new float3(1f, 0f, 0f);
        private static readonly float3 s_xTwo = new float3(2f, 0f, 0f);

        [Test]
        public void Basic()
        {
            ITestSpline spline = PrepareSpline();
            Assert.NotNull(spline);
        }

        [Test]
        public void Add1()
        {
            ITestSpline testSpline = PrepareSpline();

            float3 a = float3.zero;
            AddControlPointLocalSpace(testSpline, a);

            Assert.AreEqual(testSpline.ExpectedControlPointCount(1), testSpline.ControlPointCount);
            Assert.AreEqual(1, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(0f, testSpline.Length());

            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
        }

        [Test]
        public void Add2()
        {
            ITestSpline testSpline = PrepareSpline();

            float3 a = float3.zero;
            AddControlPointLocalSpace(testSpline, a);
            float3 b = s_xOne;
            AddControlPointLocalSpace(testSpline, b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedTimeCount(2), testSpline.Times.Count);
            Assert.AreEqual(1f, testSpline.Length());

            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
            ComparePoint(b, GetControlPoint(testSpline, 1, SplinePoint.Point));
        }

        [Test]
        public void Add3()
        {
            ITestSpline testSpline = PrepareSpline();

            float3 a = float3.zero;
            AddControlPointLocalSpace(testSpline, a);
            float3 b = s_xOne;
            AddControlPointLocalSpace(testSpline, b);
            float3 c = s_xTwo;
            AddControlPointLocalSpace(testSpline, c);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(3, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedTimeCount(3), testSpline.Times.Count);
            Assert.AreEqual(2f, testSpline.Length());

            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
            ComparePoint(b, GetControlPoint(testSpline, 1, SplinePoint.Point));
            ComparePoint(c, GetControlPoint(testSpline, 2, SplinePoint.Point));
        }

        [Test]
        public void Remove()
        {
            ITestSpline testSpline = PrepareSpline();

            float3 a = float3.zero;
            AddControlPointLocalSpace(testSpline, a);
            float3 b = s_xOne;
            AddControlPointLocalSpace(testSpline, b);
            float3 c = s_xTwo;
            AddControlPointLocalSpace(testSpline, c);

            Assert.AreEqual(3, testSpline.Modes.Count);
            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            Assert.AreEqual(2f, testSpline.Length());

            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
            ComparePoint(b, GetControlPoint(testSpline, 1, SplinePoint.Point));
            ComparePoint(c, GetControlPoint(testSpline, 2, SplinePoint.Point));

            //Remove a point
            testSpline.RemoveControlPoint(1);

            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(2f, testSpline.Length());

            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
            ComparePoint(c, GetControlPoint(testSpline, 1, SplinePoint.Point));
        }

        [Test]
        public void RemoveFromStart()
        {
            ITestSpline testSpline = PrepareSpline();

            float3 a = float3.zero;
            AddControlPointLocalSpace(testSpline, a);
            float3 b = s_xOne;
            AddControlPointLocalSpace(testSpline, b);

            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(1f, testSpline.Length());

            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
            ComparePoint(b, GetControlPoint(testSpline, 1, SplinePoint.Point));

            //Remove a point
            testSpline.RemoveControlPoint(0);

            Assert.AreEqual(1, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.ControlPointCount);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(0f, testSpline.Length());

            ComparePoint(b, GetControlPoint(testSpline, 0, SplinePoint.Point));
        }

        [Test]
        public void RemoveFromEnd2Points()
        {
            ITestSpline testSpline = PrepareSpline();

            float3 a = float3.zero;
            AddControlPointLocalSpace(testSpline, a);
            float3 b = s_xOne;
            AddControlPointLocalSpace(testSpline, b);

            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(1f, testSpline.Length());

            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
            ComparePoint(b, GetControlPoint(testSpline, 1, SplinePoint.Point));

            //Remove a point
            testSpline.RemoveControlPoint(1);

            Assert.AreEqual(1, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.ControlPointCount);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(0f, testSpline.Length());

            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
        }

        [Test]
        public void RemoveFromEnd3Points()
        {
            ITestSpline testSpline = PrepareSpline();

            float3 a = float3.zero;
            float3 b = s_xOne;
            float3 c = new float3(10f, 0f, 0f);
            AddControlPointLocalSpace(testSpline, a);
            AddControlPointLocalSpace(testSpline, b);
            AddControlPointLocalSpace(testSpline, c);

            Assert.AreEqual(3, testSpline.Modes.Count);
            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            Assert.AreEqual(10f, testSpline.Length());

            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
            ComparePoint(b, GetControlPoint(testSpline, 1, SplinePoint.Point));
            ComparePoint(c, GetControlPoint(testSpline, 2, SplinePoint.Point));

            //Remove a point
            testSpline.RemoveControlPoint(2);

            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(1f, testSpline.Length());

            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
            ComparePoint(b, GetControlPoint(testSpline, 1, SplinePoint.Point));
        }

        [Test]
        public void RemoveFromOutOfRange()
        {
            ITestSpline testSpline = PrepareSpline();

            float3 a = float3.zero;
            AddControlPointLocalSpace(testSpline, a);
            float3 b = s_xOne;
            AddControlPointLocalSpace(testSpline, b);

            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(1f, testSpline.Length());

            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
            ComparePoint(b, GetControlPoint(testSpline, 1, SplinePoint.Point));

            //Remove a point
            testSpline.RemoveControlPoint(300);

            Assert.AreEqual(1, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.ControlPointCount);
            Assert.AreEqual(testSpline.ExpectedTimeCount(1), testSpline.Times.Count);
            Assert.AreEqual(0f, testSpline.Length());

            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
        }

        [Test]
        public void Add3Remove2()
        {
            ITestSpline testSpline = PrepareSpline();

            float3 a = float3.zero;
            AddControlPointLocalSpace(testSpline, a);
            float3 b = s_xOne;
            AddControlPointLocalSpace(testSpline, b);
            float3 c = s_xTwo;
            AddControlPointLocalSpace(testSpline, c);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(3, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            Assert.AreEqual(2f, testSpline.Length());

            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
            ComparePoint(b, GetControlPoint(testSpline, 1, SplinePoint.Point));
            ComparePoint(c, GetControlPoint(testSpline, 2, SplinePoint.Point));

            // Remove a point
            testSpline.RemoveControlPoint(2);

            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(1f, testSpline.Length());

            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
            ComparePoint(b, GetControlPoint(testSpline, 1, SplinePoint.Point));

            // Remove another point
            testSpline.RemoveControlPoint(1);

            Assert.AreEqual(1, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.ControlPointCount);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(0f, testSpline.Length());

            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
        }

        [Test]
        public void RemoveFromEndOutOfRangeUnder()
        {
            ITestSpline testSpline = PrepareSpline();

            float3 a = float3.zero;
            AddControlPointLocalSpace(testSpline, a);
            float3 b = s_xOne;
            AddControlPointLocalSpace(testSpline, b);

            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(1f, testSpline.Length());

            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
            ComparePoint(b, GetControlPoint(testSpline, 1, SplinePoint.Point));

            // Remove a point less than 0
            testSpline.RemoveControlPoint(-3);

            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(1f, testSpline.Length());

            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
            ComparePoint(b, GetControlPoint(testSpline, 1, SplinePoint.Point));
        }

        [Test]
        public void RemoveWhenEmpty()
        {
            ITestSpline testSpline = PrepareSpline();

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
            ITestSpline testSpline = PrepareSpline();

            Assert.AreEqual(0, testSpline.ControlPointCount);
            Assert.AreEqual(0f, testSpline.Length());

            CompareProgressEquals(testSpline, 0.5f, float3.zero);
        }

        [Test]
        public void Point()
        {
            ITestSpline testSpline = PrepareSpline();

            float3 a = float3.zero;
            AddControlPointLocalSpace(testSpline, a);
            float3 b = s_xOne;
            AddControlPointLocalSpace(testSpline, b);
            
            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(1f, testSpline.Length());
            
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(1f, testSpline.Times[0]);

            CompareProgressEquals(testSpline, 0.5f, new float3(0.5f, 0f, 0f));
        }

        [Test]
        public void Point2()
        {
            ITestSpline testSpline = PrepareSpline();

            float3 a = float3.zero;
            AddControlPointLocalSpace(testSpline, a);
            float3 b = s_xOne;
            AddControlPointLocalSpace(testSpline, b);
            float3 c = s_xTwo;
            AddControlPointLocalSpace(testSpline, c);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(2f, testSpline.Length());

            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);

            CompareProgressEquals(testSpline, 0f, a);
            CompareProgressEquals(testSpline, 0.5f, new float3(1f, 0f, 0f));
            //TestHelpers.CheckFloat3(c * 0.77f, spline.GetPoint(0.77f)); // fails due to bezier point bunching issues
        }

        [Test]
        public void Point4()
        {
            ITestSpline testSpline = PrepareSpline();

            float3 a = new float3(3f, 3f, 1f);
            AddControlPointLocalSpace(testSpline, a);

            Assert.AreEqual(testSpline.ExpectedControlPointCount(1), testSpline.ControlPointCount);
            Assert.AreEqual(0f, testSpline.Length());

            CompareProgressEquals(testSpline, 0.5f, new float3(3f, 3f, 1f));
        }

        [Test]
        public void Update()
        {
            ITestSpline testSpline = PrepareSpline();

            float3 a = float3.zero;
            AddControlPointLocalSpace(testSpline, a);
            float3 b = s_xOne;
            AddControlPointLocalSpace(testSpline, b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedTimeCount(1), testSpline.Times.Count);
            
            CompareProgressEquals(testSpline, 0f, a);
            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
            CompareProgressEquals(testSpline, 1f, b);
            ComparePoint(b, GetControlPoint(testSpline, 1, SplinePoint.Point));

            //update 0 point position
            float3 a2 = new float3(-1f, -1f, 1f);
            UpdateControlPoint(testSpline, 0, a2, SplinePoint.Point);

            CompareProgressEquals(testSpline, 0f, a2);
            ComparePoint(a2, GetControlPoint(testSpline, 0, SplinePoint.Point));

            //update 1 point position
            float3 b2 = new float3(2f, 2f, 1f);
            UpdateControlPoint(testSpline, 1, b2, SplinePoint.Point);

            CompareProgressEquals(testSpline, 1f, b2);
            ComparePoint(b2, GetControlPoint(testSpline, 1, SplinePoint.Point));
        }

        [Test]
        public void Update2()
        {
            ITestSpline testSpline = PrepareSpline();

            float3 a = float3.zero;
            AddControlPointLocalSpace(testSpline, a);
            float3 b = s_xOne;
            AddControlPointLocalSpace(testSpline, b);
            float3 c = s_xTwo;
            AddControlPointLocalSpace(testSpline, c);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(3, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            
            CompareProgressEquals(testSpline, 0f, a);
            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
            CompareProgressEquals(testSpline, 0.5f, b);
            ComparePoint(b, GetControlPoint(testSpline, 1, SplinePoint.Point));
            CompareProgressEquals(testSpline, 1f, c);
            ComparePoint(c, GetControlPoint(testSpline, 2, SplinePoint.Point));

            //update 0 point position
            float3 a2 = new float3(0f, 1f, 1f);
            UpdateControlPoint(testSpline, 0, a2, SplinePoint.Point);
            CompareProgressEquals(testSpline, 0f, a2);
            ComparePoint(a2, GetControlPoint(testSpline, 0, SplinePoint.Point));

            //update 1 point position
            float3 b2 = new float3(1f, 1f, 1f);
            UpdateControlPoint(testSpline, 1, b2, SplinePoint.Point);
            ComparePoint(b2, GetControlPoint(testSpline, 1, SplinePoint.Point));

            //update 2 point position
            float3 c2 = new float3(2f, 1f, 1f);
            UpdateControlPoint(testSpline, 2, c2, SplinePoint.Point);
            CompareProgressEquals(testSpline, 1f, c2);
            ComparePoint(c2, GetControlPoint(testSpline, 2, SplinePoint.Point));
        }

        [Test]
        public void Update3()
        {
            ITestSpline testSpline = PrepareSpline();

            float3 a = float3.zero;
            AddControlPointLocalSpace(testSpline, a);
            float3 b = s_xOne;
            AddControlPointLocalSpace(testSpline, b);
            float3 c = s_xTwo;
            AddControlPointLocalSpace(testSpline, c);

            Assert.AreEqual(2f, testSpline.Length());

            //update 1 point position
            float3 b2 = new float3(1f, 2f, 1f);
            UpdateControlPoint(testSpline, 1, b2, SplinePoint.Point);
            Assert.GreaterOrEqual(testSpline.Length(), 2f);
        }

        [Test]
        public void Insert()
        {
            ITestSpline testSpline = PrepareSpline();

            float3 a = float3.zero;
            AddControlPointLocalSpace(testSpline, a);
            float3 b = new float3(10f, 0f, 1f);
            AddControlPointLocalSpace(testSpline, b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            CompareProgressEquals(testSpline, 0f, a);
            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
            CompareProgressEquals(testSpline, 1f, b);
            ComparePoint(b, GetControlPoint(testSpline, 1, SplinePoint.Point));

            //insert point
            float3 c = new float3(20f, 0f, 1f);
            InsertControlPointWorldSpace(testSpline, 1, c);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(3, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
            ComparePoint(c, GetControlPoint(testSpline, 1, SplinePoint.Point));
            ComparePoint(b, GetControlPoint(testSpline, 2, SplinePoint.Point));
        }

        [Test]
        public void InsertSecondFirst()
        {
            ITestSpline testSpline = PrepareSpline();

            float3 a = float3.zero;
            AddControlPointLocalSpace(testSpline, a);
            Assert.AreEqual(1, testSpline.ControlPointCount);
            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point), (float.Epsilon * 2f));

            float3 b = new float3(4);
            InsertControlPointWorldSpace(testSpline, 0, b);
            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            ComparePoint(b, GetControlPoint(testSpline, 0, SplinePoint.Point), (float.Epsilon * 2f));
            ComparePoint(a, GetControlPoint(testSpline, 1, SplinePoint.Point), (float.Epsilon * 2f));
        }

        [Test]
        public void InsertWithOne()
        {
            ITestSpline testSpline = PrepareSpline();

            float3 a = float3.zero;
            AddControlPointLocalSpace(testSpline, a);

            Assert.AreEqual(1, testSpline.ControlPointCount);
            Assert.AreEqual(1, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            CompareProgressEquals(testSpline, 0f, a);
            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));

            float3 b = new float3(10f, 0f, 1f);
            InsertControlPointWorldSpace(testSpline, 1000, b);

            CompareProgressEquals(testSpline, 1f, b);
            ComparePoint(b, GetControlPoint(testSpline, 1, SplinePoint.Point));

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
        }

        [Test]
        public void InsertEmpty()
        {
            ITestSpline testSpline = PrepareSpline();

            Assert.AreEqual(0, testSpline.ControlPointCount);
            Assert.AreEqual(0, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.Times.Count);

            float3 a = float3.zero;
            InsertControlPointWorldSpace(testSpline, 12, a);

            Assert.AreEqual(1, testSpline.ControlPointCount);
            Assert.AreEqual(1, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            CompareProgressEquals(testSpline, 0f, a);
            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
        }

        [Test]
        public void InsertAtEnd()
        {
            ITestSpline testSpline = PrepareSpline();

            float3 a = float3.zero;
            float3 b = new float3(10f, 0f, 1f);
            float3 c = new float3(20f, 0f, 1f);

            AddControlPointLocalSpace(testSpline, a);
            AddControlPointLocalSpace(testSpline, b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            CompareProgressEquals(testSpline, 0f, a);
            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
            CompareProgressEquals(testSpline, 1f, b);
            ComparePoint(b, GetControlPoint(testSpline, 1, SplinePoint.Point));

            //insert point
            InsertControlPointWorldSpace(testSpline, 2, c);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(3, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
            ComparePoint(b, GetControlPoint(testSpline, 1, SplinePoint.Point));
            ComparePoint(c, GetControlPoint(testSpline, 2, SplinePoint.Point));
        }

        [Test]
        public void InsertAtStart()
        {
            ITestSpline testSpline = PrepareSpline();

            float3 a = float3.zero;
            AddControlPointLocalSpace(testSpline, a);
            float3 b = s_xTwo;
            AddControlPointLocalSpace(testSpline, b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);

            CompareProgressEquals(testSpline, 0f, a);
            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
            CompareProgressEquals(testSpline, 1f, b);
            ComparePoint(b, GetControlPoint(testSpline, 1, SplinePoint.Point));

            //insert point
            float3 c = new float3(-2f, 0f, 1f);
            InsertControlPointWorldSpace(testSpline, 0, c);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(3, testSpline.Modes.Count);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            ComparePoint(c, GetControlPoint(testSpline, 0, SplinePoint.Point));
            ComparePoint(a, GetControlPoint(testSpline, 1, SplinePoint.Point));
            ComparePoint(b, GetControlPoint(testSpline, 2, SplinePoint.Point));
        }

        [Test]
        public void NoPoints()
        {
            ITestSpline testSpline = PrepareSpline();

            Assert.AreEqual(0, testSpline.ControlPointCount);

            CompareProgressEquals(testSpline, 0f, float3.zero);
            CompareProgressEquals(testSpline, 1f, float3.zero);
            CompareProgressEquals(testSpline, 0.5f, float3.zero);
        }

        [Test]
        public void SplineLengthRecalculation()
        {
            ITestSpline testSpline = PrepareSpline();

            Assert.AreEqual(0, testSpline.ControlPointCount);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);

            AddControlPointLocalSpace(testSpline, new float3(0f, 0f, 1f));
            AddControlPointLocalSpace(testSpline, new float3(1f, 1f, 1f));
            AddControlPointLocalSpace(testSpline, new float3(2f, 2f, 1f));

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
            const float offsetZ = 2f;

            ITestSpline testSpline = PrepareSpline();

            float3 first = float3.zero;
            for (int i = 0; i < nodeAmount; i++)
            {
                float3 pos = new float3(offsetX * (i + 1), offsetY * (i + 1), offsetZ * (i + 1));
                if(i == 0) first = pos;
                AddControlPointLocalSpace(testSpline, pos);
            }

            Assert.AreNotEqual(float3.zero, first, "Test likely misconfigured as the expects location has the default value");
            Assert.AreEqual(nodeAmount, testSpline.ControlPointCount);

            ComparePoint(first, GetProgressWorld(testSpline, 0f));
            ComparePoint(first, GetProgressWorld(testSpline, -0.5f));
            ComparePoint(first, GetProgressWorld(testSpline, -1f));
        }

        /// <summary>
        /// Progress greater than 0 should return first point
        /// </summary>
        [Test]
        public void ProgressOver([NUnit.Framework.Range(1, 8)] int nodeAmount)
        {
            const float offsetX = 2f;
            const float offsetY = 2f;
            const float offsetZ = 2f;

            ITestSpline testSpline = PrepareSpline();

            float3 last = float3.zero;
            for (int i = 0; i < nodeAmount; i++)
            {
                float3 pos = new float3(offsetX * (i + 1), offsetY * (i + 1), offsetZ * (i + 1));
                last = pos;
                AddControlPointLocalSpace(testSpline, pos);
            }

            Assert.AreNotEqual(float3.zero, last, "Test likely misconfigured as the expects location has the default value");
            Assert.AreEqual(nodeAmount, testSpline.ControlPointCount);

            ComparePoint(last, GetProgressWorld(testSpline, 1f));
            ComparePoint(last, GetProgressWorld(testSpline, 1.5f));
            ComparePoint(last, GetProgressWorld(testSpline, 2f));
        }

        [Test]
        public void Translation()
        {
            ITestSpline testSpline = PrepareSpline();
            float3 move = new float3(10f, 0f, 10f);
            ((MonoBehaviour) testSpline).transform.position = move;

            float3 a = float3.zero;
            InsertControlPointLocalSpace(testSpline, 12, a);
            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
            ComparePoint(move, GetProgressWorld(testSpline, 0f));
            ComparePoint(a, GetProgressLocal(testSpline, 0f));
        }

        [Test]
        public void TranslationPointUpdate()
        {
            ITestSpline testSpline = PrepareSpline();
            
            float3 a = float3.zero;
            InsertControlPointWorldSpace(testSpline, 12, a);
            Assert.AreEqual(1, testSpline.ControlPointCount);
            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
            
            float3 move = new float3(10f, 0f, 10f);
            ((MonoBehaviour) testSpline).transform.position = move;
            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
            ComparePoint(a, GetProgressLocal(testSpline, 0.5f));
            ComparePoint(move + a, GetProgressWorld(testSpline, 0.5f));

            a = new float3(10f);
            UpdateControlPoint(testSpline, 0, a + move, SplinePoint.Point);
            Assert.AreEqual(1, testSpline.ControlPointCount);
            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
            ComparePoint(move + a, GetProgressWorld(testSpline, 0f));
            ComparePoint(a, GetProgressLocal(testSpline, 0f));
        }

        [Test]
        public void TranslationAddLocal()
        {
            ITestSpline testSpline = PrepareSpline();
            
            // move spline
            float3 move = new float3(10f, 0f, 10f);
            ((MonoBehaviour) testSpline).transform.position = move;

            // add to spline
            float3 a = new float3(20f, 3f, 4f);
            AddControlPointLocalSpace(testSpline, a);
            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
        }

        [Test]
        public void TranslationInsertLocal()
        {
            ITestSpline testSpline = PrepareSpline();
            
            // move spline
            float3 move = new float3(10f, 0f, 10f);
            ((MonoBehaviour) testSpline).transform.position = move;

            // add to spline
            float3 a = new float3(20f, 3f, 4f);
            InsertControlPointLocalSpace(testSpline, 12, a);
            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
        }

        [Test]
        public void TranslationInsertWorld()
        {
            ITestSpline testSpline = PrepareSpline();
            
            // move spline
            float3 move = new float3(10f, 0f, 10f);
            ((MonoBehaviour) testSpline).transform.position = move;

            // add to spline
            float3 a = new float3(20f, 3f, 4f);
            InsertControlPointWorldSpace(testSpline, 12, a);
            ComparePoint(a - move, GetControlPoint(testSpline, 0, SplinePoint.Point));
        }

        [Test]
        public void TranslationClearOnMove()
        {
            ITestSpline testSpline = PrepareSpline();
            
            // move spline
            float3 move = new float3(10f, 0f, 10f);
            ((MonoBehaviour) testSpline).transform.position = move;

            // add to spline
            float3 a = new float3(20f, 3f, 4f);
            AddControlPointLocalSpace(testSpline, a);
            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
        }

        [Test]
        public void MultiMidPoint([NUnit.Framework.Range(1, 12)] int points)
        {
            ITestSpline testSpline = PrepareSpline();

            for (int i = 0; i < points; i++)
            {
                AddControlPointLocalSpace(testSpline, new float3(i));
            }

            // todo figure out how to get catmull to like this test set
            // catmull has issues with long straights
            Assume.That(testSpline.SplineDataType != SplineType.CatmullRom);

            Assert.AreEqual(points, testSpline.ControlPointCount);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);

            CompareProgressEquals(testSpline, 0.5f, new float3((points - 1) / 2f));
        }

        [Test]
        public void MultiMidPointOffset([NUnit.Framework.Range(1, 12)] int points)
        {
            ITestSpline testSpline = PrepareSpline();
            const float offset = 200f;

            for (int i = 0; i < points; i++)
            {
                AddControlPointLocalSpace(testSpline, new float3(offset + i));
            }

            // todo figure out how to get catmull to like this test set
            // catmull has issues with long straights
            Assume.That(testSpline.SplineDataType != SplineType.CatmullRom);

            Assert.AreEqual(points, testSpline.ControlPointCount);
            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);

            CompareProgressEquals(testSpline, 0.5f, new float3(offset + (points - 1) / 2f), 0.00005f);
        }
    }

    public abstract class BaseFunctionalityRotationTests : BaseFunctionalityTests
    {
        [Test]
        public void RotationPrePoint()
        {
            ITestSpline testSpline = PrepareSpline();
            
            // rotate
            Quaternion targetRotation = Quaternion.Euler(0f, 90f, 0f);
            ((MonoBehaviour) testSpline).transform.rotation = targetRotation;

            // add point
            float3 a = new float3(10f, 10f, 0f);
            InsertControlPointLocalSpace(testSpline, 12, a);
            ComparePoint(targetRotation * a, GetProgressWorld(testSpline, 0f));
            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
        }

        [Test]
        public void RotationPostPoint()
        {
            ITestSpline testSpline = PrepareSpline();

            // add point
            float3 a = new float3(10f, 10f, 0f);
            InsertControlPointWorldSpace(testSpline, 12, a);
            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
            ComparePoint(a, GetProgressWorld(testSpline, 0f));
            
            // rotate
            Quaternion targetRotation = Quaternion.Euler(0f, 90f, 0f);
            ((MonoBehaviour) testSpline).transform.rotation = targetRotation;
            ComparePoint(targetRotation * a, GetProgressWorld(testSpline, 0f));
            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));
        }

        /* // todo figure out how to fix this!!!
        [Test]
        public void TranslationRotation()
        {
            ITestSpline testSpline = PrepareSpline();
            
            // move spline
            float3 move = new float3(10f, 0f, 10f);
            ((MonoBehaviour) testSpline).transform.position = move;
            
            // rotate spline
            Quaternion targetRotation = Quaternion.Euler(0f, 90f, 0f);
            ((MonoBehaviour) testSpline).transform.rotation = targetRotation;

            // add to spline
            float3 a = new float3(20f, 3f, 4f);
            InsertControlPointLocalSpace(testSpline, 12, a);
            ComparePoint(a, GetControlPoint(testSpline, 0, SplinePoint.Point));

            if(testSpline is ISpline3DPlane)
            {
                float3 rotation = targetRotation * new float3(a.xy, 0);
                Debug.Log($"GameObject Rotation: {rotation:N5}");
                Debug.Log("");

                float3 world = GetProgressWorld(testSpline, 0f);
                Debug.Log($"WorldPos: {world:N5}");
                ComparePoint(move + rotation, world);
                Debug.Log("");

                float3 local = GetProgressLocal(testSpline, 0f);
                Debug.Log($"LocalPos: {local:N5}");
                ComparePoint(rotation, local);
                Debug.Log("");
            }
            else
            {
                ComparePoint(a, GetProgressLocal(testSpline, 0f));
                ComparePoint(move + (float3)(targetRotation * a), GetProgressWorld(testSpline, 0f));
            }
        }*/
    }

    public abstract class BaseFunctionalityTests3D : BaseFunctionalityRotationTests
    {
        private static SplineInteractionBase3D s_splineBase = new SplineInteractionBase3D();

        public override void AddControlPointLocalSpace(ITestSpline spline, float3 point) =>
            s_splineBase.AddControlPointLocalSpace(spline as ISimpleSpline3D, point);

        public override void InsertControlPointWorldSpace(ITestSpline spline, int index, float3 point) =>
            s_splineBase.InsertControlPointWorldSpace(spline as ISimpleSpline3D, index, point);

        public override void InsertControlPointLocalSpace(ITestSpline spline, int index, float3 point) => 
            s_splineBase.InsertControlPointLocalSpace(spline as ISimpleSpline3D, index, point);

        public override float3 GetControlPoint(ITestSpline spline, int index, SplinePoint pointType) =>
            s_splineBase.GetControlPoint(spline as ISimpleSpline3D, index, pointType);

        public override void UpdateControlPoint(ITestSpline spline, int index, float3 newPoint, SplinePoint pointType) =>
            s_splineBase.UpdateControlPoint(spline as ISimpleSpline3D, index, newPoint, pointType);

        public override float3 GetProgressWorld(ITestSpline spline, float progress) =>
            s_splineBase.GetProgressWorld(spline as ISimpleSpline3D, progress);
        
        public override float3 GetProgressLocal(ITestSpline spline, float progress) =>
            s_splineBase.GetProgressLocal(spline as ISimpleSpline3D, progress);

        public override void CompareProgressEquals(ITestSpline spline, float progress, float3 expectedPoint,
            float tolerance = 0.00001f) =>
            s_splineBase.CompareProgressEquals(spline as ISimpleSpline3D, progress, expectedPoint, tolerance);

        public override void CompareProgressNotEquals(ITestSpline spline, float progress, float3 expectedPoint) =>
            s_splineBase.CompareProgressNotEquals(spline as ISimpleSpline3D, progress, expectedPoint);

        public override void ComparePoint(float3 expected, float3 actual, float tolerance = 0.00001f) =>
            s_splineBase.ComparePoint(expected, actual, tolerance);

        public override float Length(float3 a, float3 b) => s_splineBase.Length(a, b);
    }

    public abstract class BaseFunctionalityTests3DPlane : BaseFunctionalityRotationTests
    {
        private static SplineInteractionBase3DPlane s_splineBase = new SplineInteractionBase3DPlane();

        public override void AddControlPointLocalSpace(ITestSpline spline, float3 point) =>
            s_splineBase.AddControlPointLocalSpace(spline as ISpline3DPlane, point);

        public override void InsertControlPointWorldSpace(ITestSpline spline, int index, float3 point) =>
            s_splineBase.InsertControlPointWorldSpace(spline as ISpline3DPlane, index, point);

        public override void InsertControlPointLocalSpace(ITestSpline spline, int index, float3 point) => 
            s_splineBase.InsertControlPointLocalSpace(spline as ISpline3DPlane, index, point);

        public override float3 GetControlPoint(ITestSpline spline, int index, SplinePoint pointType) =>
            s_splineBase.GetControlPoint(spline as ISpline3DPlane, index, pointType);

        public override void UpdateControlPoint(ITestSpline spline, int index, float3 newPoint, SplinePoint pointType) =>
            s_splineBase.UpdateControlPoint(spline as ISpline3DPlane, index, newPoint, pointType);

        public override float3 GetProgressWorld(ITestSpline spline, float progress) =>
            s_splineBase.GetProgressWorld(spline as ISpline3DPlane, progress);
        
        public override float3 GetProgressLocal(ITestSpline spline, float progress) =>
            s_splineBase.GetProgressLocal(spline as ISpline3DPlane, progress);

        public override void CompareProgressEquals(ITestSpline spline, float progress, float3 expectedPoint,
            float tolerance = 0.00001f) =>
            s_splineBase.CompareProgressEquals(spline as ISpline3DPlane, progress, expectedPoint, tolerance);

        public override void CompareProgressNotEquals(ITestSpline spline, float progress, float3 expectedPoint) =>
            s_splineBase.CompareProgressNotEquals(spline as ISpline3DPlane, progress, expectedPoint);

        public override void ComparePoint(float3 expected, float3 actual, float tolerance = 0.00001f) =>
            s_splineBase.ComparePoint(expected, actual, tolerance);

        public override float Length(float3 a, float3 b) => s_splineBase.Length(a, b);
    }
    
    public abstract class BaseFunctionalityTests2DPlane : BaseFunctionalityTests
    {
        private static SplineInteractionBase3DPlane s_splineBase = new SplineInteractionBase3DPlane();

        public override void AddControlPointLocalSpace(ITestSpline spline, float3 point) =>
            s_splineBase.AddControlPointLocalSpace(spline as ISpline3DPlane, point);

        public override void InsertControlPointWorldSpace(ITestSpline spline, int index, float3 point) =>
            s_splineBase.InsertControlPointWorldSpace(spline as ISpline3DPlane, index, point);

        public override void InsertControlPointLocalSpace(ITestSpline spline, int index, float3 point) => 
            s_splineBase.InsertControlPointLocalSpace(spline as ISpline3DPlane, index, point);

        public override float3 GetControlPoint(ITestSpline spline, int index, SplinePoint pointType) =>
            s_splineBase.GetControlPoint(spline as ISpline3DPlane, index, pointType);

        public override void UpdateControlPoint(ITestSpline spline, int index, float3 newPoint, SplinePoint pointType) =>
            s_splineBase.UpdateControlPoint(spline as ISpline3DPlane, index, newPoint, pointType);

        public override float3 GetProgressWorld(ITestSpline spline, float progress) =>
            s_splineBase.GetProgressWorld(spline as ISpline3DPlane, progress);
        
        public override float3 GetProgressLocal(ITestSpline spline, float progress) =>
            s_splineBase.GetProgressLocal(spline as ISpline3DPlane, progress);

        public override void CompareProgressEquals(ITestSpline spline, float progress, float3 expectedPoint,
            float tolerance = 0.00001f) =>
            s_splineBase.CompareProgressEquals(spline as ISpline3DPlane, progress, expectedPoint, tolerance);

        public override void CompareProgressNotEquals(ITestSpline spline, float progress, float3 expectedPoint) =>
            s_splineBase.CompareProgressNotEquals(spline as ISpline3DPlane, progress, expectedPoint);

        public override void ComparePoint(float3 expected, float3 actual, float tolerance = 0.00001f) =>
            s_splineBase.ComparePoint(expected, actual, tolerance);

        public override float Length(float3 a, float3 b) => s_splineBase.Length(a, b);
    }

    public abstract class BaseFunctionalityTests2D : BaseFunctionalityTests
    {
        private static SplineInteractionBase2D s_splineBase = new SplineInteractionBase2D();

        public override void AddControlPointLocalSpace(ITestSpline spline, float3 point) =>
            s_splineBase.AddControlPointLocalSpace(spline as ISpline2D, point);

        public override void InsertControlPointWorldSpace(ITestSpline spline, int index, float3 point) =>
            s_splineBase.InsertControlPointWorldSpace(spline as ISpline2D, index, point);

        public override void InsertControlPointLocalSpace(ITestSpline spline, int index, float3 point) => 
            s_splineBase.InsertControlPointWorldSpace(spline as ISpline2D, index, point);

        public override float3 GetControlPoint(ITestSpline spline, int index, SplinePoint pointType) =>
            s_splineBase.GetControlPoint(spline as ISpline2D, index, pointType);

        public override void UpdateControlPoint(ITestSpline spline, int index, float3 newPoint, SplinePoint pointType) =>
            s_splineBase.UpdateControlPoint(spline as ISpline2D, index, newPoint, pointType);

        public override float3 GetProgressWorld(ITestSpline spline, float progress) =>
            s_splineBase.GetProgressWorld(spline as ISpline2D, progress);
        
        public override float3 GetProgressLocal(ITestSpline spline, float progress) =>
            s_splineBase.GetProgressLocal(spline as ISpline2D, progress);

        public override void CompareProgressEquals(ITestSpline spline, float progress, float3 expectedPoint,
            float tolerance = 0.00001f) =>
            s_splineBase.CompareProgressEquals(spline as ISpline2D, progress, expectedPoint, tolerance);

        public override void CompareProgressNotEquals(ITestSpline spline, float progress, float3 expectedPoint) =>
            s_splineBase.CompareProgressNotEquals(spline as ISpline2D, progress, expectedPoint);

        public override void ComparePoint(float3 expected, float3 actual, float tolerance = 0.00001f) =>
            s_splineBase.ComparePoint(actual, expected, tolerance);

        public override float Length(float3 a, float3 b) => s_splineBase.Length(a, b);
    }
}