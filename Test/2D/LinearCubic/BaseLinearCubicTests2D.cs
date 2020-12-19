using Crener.Spline.Common.Interfaces;
using Crener.Spline.Test._2D.LinearCubic.TestAdapters;
using Crener.Spline.Test._2D.LinearCubic.TestTypes;
using Crener.Spline.Test.BaseTests;
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
            ISimpleTestSpline spline = game.AddComponent<MeaninglessTestWrapper2.TestLinearCubic2DSplineSimple>();

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

            TestHelpers.CheckFloat2(topLeft, testSpline.Get2DPointWorld(0f));
            TestHelpers.CheckFloat2(topLeft, testSpline.Get2DPointWorld(-0.1f));
            TestHelpers.CheckFloat2(topRight, testSpline.Get2DPointWorld(1f));
            TestHelpers.CheckFloat2(topRight, testSpline.Get2DPointWorld(1.1f));

            (testSpline as ILoopingSpline).Looped = true;
            Assert.AreEqual(4, testSpline.ControlPointCount);
            Assert.AreEqual(5, testSpline.SegmentPointCount);

            Assert.AreNotEqual(topLeft, testSpline.Get2DPointWorld(-1f));
            Assert.AreNotEqual(topRight, testSpline.Get2DPointWorld(2f));
        }

        //todo test that 3 or 4 point looped spline doesn't touch any point directly
    }
    
    public class LoopingLinearCubicTests2D : BaseLoopingTests2D
    {
        public override ILoopingSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ILoopingSpline spline = game.AddComponent<MeaninglessTestWrapper2.TestLinearCubic2DSplineSimple>();

            return spline;
        }
    }
    
    public class ArkLinearCubicTests2D : BaseArkTests2D
    {
        public override IArkableSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            IArkableSpline spline = game.AddComponent<MeaninglessTestWrapper.TestLinearCubic2DSplineSimpleJob>();

            return spline;
        }
    }
}