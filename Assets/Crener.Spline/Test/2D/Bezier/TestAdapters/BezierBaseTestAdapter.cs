using Crener.Spline.Common;
using NUnit.Framework;
using Unity.Mathematics;

namespace Crener.Spline.Test._2D.Bezier.TestAdapters
{
    public abstract class BezierBaseTestAdapter : BaseSimpleSplineTests
    {
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
            CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));

            float2 b = new float2(10f, 0f);
            testSpline.AddControlPoint(b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(10f, testSpline.Length());

            Assert.AreEqual(4, testSpline.ControlPoints.Count);
            CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            CheckFloat2(new float2(1f, 0f), testSpline.GetControlPoint(0, SplinePoint.Post));
            CheckFloat2(new float2(9f, 0f), testSpline.GetControlPoint(1, SplinePoint.Pre));
            CheckFloat2(b, testSpline.GetControlPoint(1, SplinePoint.Point));
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

            CheckFloat2(new float2(2.5f, 10f), testSpline.GetPoint(0.7f), 0.01f);
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
    }
}