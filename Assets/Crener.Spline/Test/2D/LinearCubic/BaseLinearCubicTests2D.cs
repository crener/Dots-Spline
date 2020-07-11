using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Crener.Spline.Test._2D.LinearCubic.TestAdapters;
using Crener.Spline.Test._2D.LinearCubic.TestTypes;
using Crener.Spline.Test.Helpers;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

namespace Crener.Spline.Test._2D.LinearCubic
{
    /// <summary>
    /// Tests Point to point implementation of basic 2D spline functionality
    /// </summary>
    public class BaseLinearCubicTests2D : LinearCubicBaseTest2DAdapter
    {
        protected override ISimpleTestSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ISimpleTestSpline spline = game.AddComponent<MeaninglessTestWrapper2.TestLinearCubicSpline2DSimple>();

            return spline;
        }


        /// <summary>
        /// 4 point square with a perfect circle 
        /// </summary>
        [Test]
        public void Circle()
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
            TestHelpers.CheckFloat2(topRight, testSpline.GetPoint(1.1f));

            (testSpline as ILoopingSpline).Looped = true;
            Assert.AreEqual(4, testSpline.ControlPointCount);
            Assert.AreEqual(5, testSpline.SegmentPointCount);

            Assert.AreNotEqual(topLeft, testSpline.GetPoint(-1f));
            Assert.AreNotEqual(topRight, testSpline.GetPoint(2f));
        }

        //todo test that 3 or 4 point looped spline doesn't touch any point directly
    }
}