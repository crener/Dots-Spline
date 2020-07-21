using Crener.Spline.Common;
using Crener.Spline.Test.Helpers;
using NUnit.Framework;
using Unity.Mathematics;

namespace Crener.Spline.Test._2D.Cubic.TestAdapters
{
    public abstract class CubicBaseTest2DAdapter : BaseSimpleSplineTests2D
    {
        [Test]
        public void Point3()
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 a = float2.zero;
            testSpline.AddControlPoint(float2.zero);
            float2 b = new float2(2.5f, 0f);
            testSpline.AddControlPoint(b);
            float2 c = new float2(7.5f, 0f);
            testSpline.AddControlPoint(c);
            float2 d = new float2(10f, 0f);
            testSpline.AddControlPoint(d);

            Assert.AreEqual(4, testSpline.ControlPointCount);
            Assert.AreEqual(4, testSpline.Modes.Count);
            Assert.AreEqual(10f, testSpline.Length());

            Assert.AreEqual(testSpline.ExpectedTimeCount(testSpline.ControlPointCount), testSpline.Times.Count);
            Assert.AreEqual(0.25f, testSpline.Times[0]);
            Assert.AreEqual(0.75f, testSpline.Times[1]);
            Assert.AreEqual(1f, testSpline.Times[2]);

            TestHelpers.CheckFloat2(a, testSpline.GetPoint(0f));
            TestHelpers.CheckFloat2(new float2(2.5f, 0f), testSpline.GetPoint(0.25f));
            TestHelpers.CheckFloat2(new float2(5f, 0f), testSpline.GetPoint(0.5f));
            TestHelpers.CheckFloat2(new float2(10f, 0f), testSpline.GetPoint(1f));
            TestHelpers.CheckFloat2(new float2(10f, 0f), testSpline.GetPoint(1.5f));
            TestHelpers.CheckFloat2(new float2(10f, 0f), testSpline.GetPoint(5f));
        }

        [Test]
        public void NoEditModeChange([Values]SplineEditMode mode)
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 a = float2.zero;
            testSpline.AddControlPoint(a);

            testSpline.ChangeEditMode(0, mode);
            Assert.AreEqual(SplineEditMode.Standard, testSpline.GetEditMode(0));
        }

        [Test]
        public void ZigZagLength()
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 a = new float2(0f, 0f);
            testSpline.AddControlPoint(a);
            float2 b = new float2(1f, 3f);
            testSpline.AddControlPoint(b);

            testSpline.UpdateControlPoint(0, new float2(1f,0f),SplinePoint.Post );
            testSpline.UpdateControlPoint(1, new float2(0f,3f),SplinePoint.Pre );
            
            float length = math.distance(a, b);
            float spline = testSpline.Length();
            Assert.IsTrue(math.abs(length - spline) <= 0.00005f, $"Expected: {length}, but received: {spline}");
        }
    }
}