using Crener.Spline.Common;
using NUnit.Framework;
using Unity.Mathematics;

namespace Crener.Spline.Test._2D.P2P.TestAdapters
{
    public abstract class LinearBaseTestAdapter : BaseSimpleSplineTests
    {
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
            float2 b = new float2(10f, 10f);
            testSpline.AddControlPoint(b);
            float2 c = new float2(0f, 20f);
            testSpline.AddControlPoint(c);
            float2 d = new float2(20f, 30f);
            testSpline.AddControlPoint(d);

            float length = math.distance(a, b) + math.distance(b, c) + math.distance(c, d);
            float spline = testSpline.Length();
            Assert.IsTrue(math.abs(length - spline) <= 0.00005f, $"Expected: {length}, but received: {spline}");
        }

        [Test]
        public void ZigZagLength2()
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