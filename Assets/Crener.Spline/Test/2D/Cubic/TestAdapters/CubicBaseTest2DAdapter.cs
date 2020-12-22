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
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            testSpline2D.AddControlPoint(float2.zero);
            float2 b = new float2(2.5f, 0f);
            testSpline2D.AddControlPoint(b);
            float2 c = new float2(7.5f, 0f);
            testSpline2D.AddControlPoint(c);
            float2 d = new float2(10f, 0f);
            testSpline2D.AddControlPoint(d);

            Assert.AreEqual(4, testSpline2D.ControlPointCount);
            Assert.AreEqual(4, testSpline2D.Modes.Count);
            Assert.AreEqual(10f, testSpline2D.Length());

            Assert.AreEqual(testSpline2D.ExpectedTimeCount(testSpline2D.ControlPointCount), testSpline2D.Times.Count);
            Assert.AreEqual(0.25f, testSpline2D.Times[0]);
            Assert.AreEqual(0.75f, testSpline2D.Times[1]);
            Assert.AreEqual(1f, testSpline2D.Times[2]);

            TestHelpers.CheckFloat2(a, testSpline2D.Get2DPointWorld(0f));
            TestHelpers.CheckFloat2(new float2(2.5f, 0f), testSpline2D.Get2DPointWorld(0.25f));
            TestHelpers.CheckFloat2(new float2(5f, 0f), testSpline2D.Get2DPointWorld(0.5f));
            TestHelpers.CheckFloat2(new float2(10f, 0f), testSpline2D.Get2DPointWorld(1f));
            TestHelpers.CheckFloat2(new float2(10f, 0f), testSpline2D.Get2DPointWorld(1.5f));
            TestHelpers.CheckFloat2(new float2(10f, 0f), testSpline2D.Get2DPointWorld(5f));
        }

        [Test]
        public void NoEditModeChange([Values]SplineEditMode mode)
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = float2.zero;
            testSpline2D.AddControlPoint(a);

            testSpline2D.ChangeEditMode(0, mode);
            Assert.AreEqual(SplineEditMode.Standard, testSpline2D.GetEditMode(0));
        }

        [Test]
        public void ZigZagLength()
        {
            ISimpleTestSpline2D testSpline2D = PrepareSpline();

            float2 a = new float2(0f, 0f);
            testSpline2D.AddControlPoint(a);
            float2 b = new float2(1f, 3f);
            testSpline2D.AddControlPoint(b);

            testSpline2D.UpdateControlPointLocal(0, new float2(1f,0f),SplinePoint.Post );
            testSpline2D.UpdateControlPointLocal(1, new float2(0f,3f),SplinePoint.Pre );
            
            float length = math.distance(a, b);
            float spline = testSpline2D.Length();
            Assert.IsTrue(math.abs(length - spline) <= 0.00005f, $"Expected: {length}, but received: {spline}");
        }
    }
}