using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Crener.Spline.Test.Helpers;
using NUnit.Framework;
using Unity.Mathematics;

namespace Crener.Spline.Test._2D.LinearCubic.TestAdapters
{
    public abstract class LinearCubicBaseTestAdapter : BaseSimpleSplineTests
    {
        /// <summary>
        /// test that 3 point spline doesn't touch 2nd point when in L shape
        /// </summary>
        [Test]
        public void PointL()
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 topLeft = new float2(1f, 2f);
            float2 bottomLeft = new float2(1f, 1f);
            float2 bottomRight = new float2(2f, 1f);
            testSpline.AddControlPoint(topLeft);
            testSpline.AddControlPoint(bottomLeft);
            testSpline.AddControlPoint(bottomRight);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.SegmentPointCount);

            Assert.AreNotEqual(bottomLeft, testSpline.GetPoint(0.5f));
        }

        /// <summary>
        /// test that 3 point spline without looping doesn't end at the first point
        /// </summary>
        [Test]
        public void Loopback()
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 topLeft = new float2(1f, 2f);
            float2 bottomLeft = new float2(1f, 1f);
            float2 bottomRight = new float2(2f, 1f);
            testSpline.AddControlPoint(topLeft);
            testSpline.AddControlPoint(bottomLeft);
            testSpline.AddControlPoint(bottomRight);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.SegmentPointCount);

            Assert.AreEqual(topLeft, testSpline.GetPoint(0f));
            Assert.AreNotEqual(topLeft, testSpline.GetPoint(1f));
            Assert.AreEqual(bottomRight, testSpline.GetPoint(1f));
        }

        /// <summary>
        /// test that 4 point spline doesn't touch 2nd and 3rd point
        /// </summary>
        [Test]
        public void NoTouch4Point()
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 topLeft = new float2(1f, 2f);
            float2 bottomLeft = new float2(1f, 1f);
            float2 bottomRight = new float2(2f, 1f);
            float2 topRight = new float2(2f, 2f);
            testSpline.AddControlPoint(topLeft);
            testSpline.AddControlPoint(bottomLeft);
            testSpline.AddControlPoint(bottomRight);
            testSpline.AddControlPoint(topRight);

            Assert.AreEqual(4, testSpline.ControlPointCount);
            Assert.AreEqual(3, testSpline.SegmentPointCount);

            TestHelpers.CheckFloat2(topLeft, testSpline.GetPoint(0f));
            TestHelpers.CheckFloat2(topLeft, testSpline.GetPoint(-0.1f));
            TestHelpers.CheckFloat2(topRight, testSpline.GetPoint(1f));
            TestHelpers.CheckFloat2(topRight, testSpline.GetPoint(1.1f), 0.025f);

            Assert.AreNotEqual(bottomLeft, testSpline.GetPoint(0.33f));
            Assert.AreNotEqual(topRight, testSpline.GetPoint(0.66f));
        }

        /// <summary>
        /// test that 4 point spline doesn't touch 2nd and 3rd point
        /// </summary>
        [Test]
        public void NoTouch3Point()
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 topLeft = new float2(1f, 2f);
            float2 bottomLeft = new float2(1f, 1f);
            float2 bottomRight = new float2(2f, 1f);
            testSpline.AddControlPoint(topLeft);
            testSpline.AddControlPoint(bottomLeft);
            testSpline.AddControlPoint(bottomRight);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.SegmentPointCount);

            TestHelpers.CheckFloat2(topLeft, testSpline.GetPoint(0f));
            TestHelpers.CheckFloat2(topLeft, testSpline.GetPoint(-0.1f));
            TestHelpers.CheckFloat2(bottomRight, testSpline.GetPoint(1f));
            TestHelpers.CheckFloat2(bottomRight, testSpline.GetPoint(1.1f));

            Assert.AreNotEqual(bottomLeft, testSpline.GetPoint(0.5f));
        }

        // todo add tests which check the error range in the first segment and last segment
        // once the weight has been removed from those first points
    }
}