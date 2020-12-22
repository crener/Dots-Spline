using Crener.Spline.Test.Helpers;
using NUnit.Framework;
using Unity.Mathematics;

namespace Crener.Spline.Test._2D.LinearCubic.TestAdapters
{
    public abstract class LinearCubicBaseTest2DAdapter : BaseSimpleSplineTests2D
    {
        /// <summary>
        /// test that 3 point spline doesn't touch 2nd point when in L shape
        /// </summary>
        [Test]
        public void PointL()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 topLeft = new float2(1f, 2f);
            float2 bottomLeft = new float2(1f, 1f);
            float2 bottomRight = new float2(2f, 1f);
            testSpline2D.AddControlPoint(topLeft);
            testSpline2D.AddControlPoint(bottomLeft);
            testSpline2D.AddControlPoint(bottomRight);

            Assert.AreEqual(3, testSpline2D.ControlPointCount);
            Assert.AreEqual(2, testSpline2D.SegmentPointCount);

            Assert.AreNotEqual(bottomLeft, testSpline2D.Get2DPointWorld(0.5f));
        }

        /// <summary>
        /// test that 3 point spline without looping doesn't end at the first point
        /// </summary>
        [Test]
        public void Loopback()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 topLeft = new float2(1f, 2f);
            float2 bottomLeft = new float2(1f, 1f);
            float2 bottomRight = new float2(2f, 1f);
            testSpline2D.AddControlPoint(topLeft);
            testSpline2D.AddControlPoint(bottomLeft);
            testSpline2D.AddControlPoint(bottomRight);

            Assert.AreEqual(3, testSpline2D.ControlPointCount);
            Assert.AreEqual(2, testSpline2D.SegmentPointCount);

            Assert.AreEqual(topLeft, testSpline2D.Get2DPointWorld(0f));
            Assert.AreNotEqual(topLeft, testSpline2D.Get2DPointWorld(1f));
            Assert.AreEqual(bottomRight, testSpline2D.Get2DPointWorld(1f));
        }

        /// <summary>
        /// test that 4 point spline doesn't touch 2nd and 3rd point
        /// </summary>
        [Test]
        public void NoTouch4Point()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 topLeft = new float2(1f, 2f);
            float2 bottomLeft = new float2(1f, 1f);
            float2 bottomRight = new float2(2f, 1f);
            float2 topRight = new float2(2f, 2f);
            testSpline2D.AddControlPoint(topLeft);
            testSpline2D.AddControlPoint(bottomLeft);
            testSpline2D.AddControlPoint(bottomRight);
            testSpline2D.AddControlPoint(topRight);

            Assert.AreEqual(4, testSpline2D.ControlPointCount);
            Assert.AreEqual(3, testSpline2D.SegmentPointCount);

            TestHelpers.CheckFloat2(topLeft, testSpline2D.Get2DPointWorld(0f));
            TestHelpers.CheckFloat2(topLeft, testSpline2D.Get2DPointWorld(-0.1f));
            TestHelpers.CheckFloat2(topRight, testSpline2D.Get2DPointWorld(1f));
            TestHelpers.CheckFloat2(topRight, testSpline2D.Get2DPointWorld(1.1f), 0.025f);

            Assert.AreNotEqual(bottomLeft, testSpline2D.Get2DPointWorld(0.33f));
            Assert.AreNotEqual(topRight, testSpline2D.Get2DPointWorld(0.66f));
        }

        /// <summary>
        /// test that 4 point spline doesn't touch 2nd and 3rd point
        /// </summary>
        [Test]
        public void NoTouch3Point()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 topLeft = new float2(1f, 2f);
            float2 bottomLeft = new float2(1f, 1f);
            float2 bottomRight = new float2(2f, 1f);
            testSpline2D.AddControlPoint(topLeft);
            testSpline2D.AddControlPoint(bottomLeft);
            testSpline2D.AddControlPoint(bottomRight);

            Assert.AreEqual(3, testSpline2D.ControlPointCount);
            Assert.AreEqual(2, testSpline2D.SegmentPointCount);

            TestHelpers.CheckFloat2(topLeft, testSpline2D.Get2DPointWorld(0f));
            TestHelpers.CheckFloat2(topLeft, testSpline2D.Get2DPointWorld(-0.1f));
            TestHelpers.CheckFloat2(bottomRight, testSpline2D.Get2DPointWorld(1f));
            TestHelpers.CheckFloat2(bottomRight, testSpline2D.Get2DPointWorld(1.1f));

            Assert.AreNotEqual(bottomLeft, testSpline2D.Get2DPointWorld(0.5f));
        }
        
        [Test]
        public void Point3()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            float2 b = new float2(2.5f, 0f);
            float2 c = new float2(7.5f, 0f);
            float2 d = new float2(10f, 0f);
            
            testSpline2D.AddControlPoint(a);
            testSpline2D.AddControlPoint(b);
            testSpline2D.AddControlPoint(c);
            testSpline2D.AddControlPoint(d);

            Assert.AreEqual(4, testSpline2D.ControlPointCount);
            Assert.AreEqual(4, testSpline2D.Modes.Count);
            Assert.AreEqual(10f, testSpline2D.Length());

            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            Assert.AreEqual(0.5f, testSpline2D.Times[0]);
            Assert.AreEqual(1f, testSpline2D.Times[1]);

            TestHelpers.CheckFloat2(a, testSpline2D.Get2DPointWorld(0f));
            TestHelpers.CheckFloat2(new float2(5f, 0f), testSpline2D.Get2DPointWorld(0.5f));
            TestHelpers.CheckFloat2(d, testSpline2D.Get2DPointWorld(1f));
            TestHelpers.CheckFloat2(d, testSpline2D.Get2DPointWorld(1.5f));
            TestHelpers.CheckFloat2(d, testSpline2D.Get2DPointWorld(5f));
        }

        // todo add tests which check the error range in the first segment and last segment
        // once the weight has been removed from those first points
    }
}