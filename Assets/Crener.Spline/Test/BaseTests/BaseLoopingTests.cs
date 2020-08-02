using Crener.Spline.Common.Interfaces;
using Crener.Spline.Test.Helpers;
using NUnit.Framework;
using Unity.Mathematics;

namespace Crener.Spline.Test.BaseTests
{
    public abstract class BaseLoopingTests : TransferableTestSet<ILoopingSpline>
    {
        protected void ChangeLooped(ILoopingSpline spline, bool newLoopedState)
        {
            int segmentCount = spline.SegmentPointCount;
            bool preLooped = spline.Looped;

            spline.Looped = newLoopedState;
            Assert.AreEqual(spline.Looped, newLoopedState);

            if(preLooped != newLoopedState)
            {
                if(newLoopedState)
                    Assert.Greater(spline.SegmentPointCount, segmentCount);
                else
                    Assert.Less(spline.SegmentPointCount, segmentCount);
            }
        }

        /// <summary>
        /// Create a new spline and validates that it is ready for testing
        /// </summary>
        protected virtual ILoopingSpline PrepareSpline()
        {
            ILoopingSpline spline = base.PrepareSpline();
            spline.Looped = false;
            return spline;
        }
        
        [Test]
        public void LengthIncrease([Range(2, 9)] int points)
        {
            ILoopingSpline testSpline = PrepareSpline();

            float3 first = new float3(0);
            AddControlPoint(testSpline, first);

            for (int i = 1; i < points; i++)
            {
                float3 point = new float3(i);
                AddControlPoint(testSpline, point);
            }

            Assert.IsFalse(testSpline.Looped);
            float length = testSpline.Length();

            // loop the spline
            ChangeLooped(testSpline, true);
            Assert.Greater(testSpline.Length(), length, "Looping the spline did not result in an increase the spline length");

            // remove the spline looping
            ChangeLooped(testSpline, false);
            Assert.AreEqual(length, testSpline.Length(), "Loop should return to original length");
        }

        [Test]
        public void LengthIncreaseSwitchBeforeFirstPoint([Range(2, 9)] int points)
        {
            ILoopingSpline testSpline = PrepareSpline();
            ChangeLooped(testSpline, true);

            float3 first = new float3(0);
            AddControlPoint(testSpline, first);

            for (int i = 1; i < points; i++)
            {
                float3 point = new float3(i);
                AddControlPoint(testSpline, point);
            }

            float length = testSpline.Length();

            // remove loop
            ChangeLooped(testSpline, false);
            Assert.Less(testSpline.Length(), length, "spline should have reduced in size due to no longer being looped");

            // back to original loop size
            ChangeLooped(testSpline, true);
            Assert.AreEqual(testSpline.Length(), length, "spline is back to original state, so it should match the original length");
        }

        [Test]
        public void NoPoint()
        {
            ILoopingSpline testSpline = PrepareSpline();

            Assert.IsFalse(testSpline.Looped);
            Assert.AreEqual(0f, testSpline.Length());

            ChangeLooped(testSpline, true);
            Assert.AreEqual(0f, testSpline.Length());
        }

        [Test]
        public void SinglePoint()
        {
            ILoopingSpline testSpline = PrepareSpline();
            AddControlPoint(testSpline, new float3(80));

            Assert.IsFalse(testSpline.Looped);
            Assert.AreEqual(0f, testSpline.Length());

            ChangeLooped(testSpline, true);
            Assert.AreEqual(0f, testSpline.Length());

            ChangeLooped(testSpline, false);
            Assert.AreEqual(0f, testSpline.Length());
        }

        [Test]
        public void LineLoopback()
        {
            ILoopingSpline testSpline = PrepareSpline();

            float3 a = new float3(10f);
            float3 b = new float3(20f);

            AddControlPoint(testSpline, a);
            AddControlPoint(testSpline, b);

            Assert.IsFalse(testSpline.Looped);
            CompareProgressEquals(testSpline, 0f, a);
            CompareProgressEquals(testSpline, 1f, b);
            
            ChangeLooped(testSpline, true);
            float3 point = GetProgress(testSpline, 0f);
            CompareProgressEquals(testSpline, 0f, point);
            CompareProgressEquals(testSpline, 1f, point);
        }

        [Test]
        public void TriangleLoopback()
        {
            ILoopingSpline testSpline = PrepareSpline();

            float3 a = new float3(10f, 10f, 3f);
            float3 b = new float3(12f, 10f, 3f);
            float3 c = new float3(11f, 11f, 3f);

            AddControlPoint(testSpline, a);
            AddControlPoint(testSpline, b);
            AddControlPoint(testSpline, c);

            Assert.IsFalse(testSpline.Looped);
            CompareProgressEquals(testSpline, 0f, a);
            CompareProgressEquals(testSpline, 1f, c);
            
            ChangeLooped(testSpline, true);
            float3 point = GetProgress(testSpline, 0f);
            CompareProgressEquals(testSpline, 0f, point);
            CompareProgressEquals(testSpline, 1f, point);
        }

        [Test]
        public void SquareLoopback()
        {
            ILoopingSpline testSpline = PrepareSpline();

            float3 a = new float3(10f, 10f, 3f);
            float3 b = new float3(12f, 10f, 3f);
            float3 c = new float3(12f, 12f, 3f);
            float3 d = new float3(10f, 12f, 3f);

            AddControlPoint(testSpline, a);
            AddControlPoint(testSpline, b);
            AddControlPoint(testSpline, c);
            AddControlPoint(testSpline, d);

            Assert.IsFalse(testSpline.Looped);
            CompareProgressEquals(testSpline, 0f, a);
            CompareProgressEquals(testSpline, 1f, d);
            
            ChangeLooped(testSpline, true);
            float3 point = GetProgress(testSpline, 0f);
            CompareProgressEquals(testSpline, 0f, point);
            CompareProgressEquals(testSpline, 1f, point);
        }
    }

    public abstract class BaseLoopingTests3D : BaseLoopingTests
    {
        protected override void AddControlPoint(ILoopingSpline spline, float3 point)
        {
            Assert.NotNull(spline);

            ISpline3D spline3D = spline as ISpline3D;
            Assert.NotNull(spline3D, $"Failed to convert to 3D spline! Spline type was: {spline.GetType().Name}");

            int before = spline3D.ControlPointCount;
            spline3D.AddControlPoint(point);

            Assert.Greater(spline3D.ControlPointCount, before, "Adding a point did not increase the control point count");
        }

        protected override float Length(float3 a, float3 b) => math.distance(a, b);
        
        protected override float3 GetProgress(ILoopingSpline spline, float progress)
        {
            ISpline3D spline3D = spline as ISpline3D;
            Assert.NotNull(spline3D);
            return spline3D.GetPoint(progress);
        }

        protected override void CompareProgressEquals(ILoopingSpline spline, float progress, float3 expectedPoint, float tolerance = 0.00001f)
        {
            ISpline3D spline3D = spline as ISpline3D;
            Assert.NotNull(spline3D);

            float3 point = spline3D.GetPoint(progress);
            TestHelpers.CheckFloat3(point, expectedPoint, tolerance);
        }

        protected override void CompareProgress(ILoopingSpline spline, float progress, float3 unexpectedPoint)
        {
            ISpline3D spline3D = spline as ISpline3D;
            Assert.NotNull(spline3D);

            float3 point = spline3D.GetPoint(progress);
            Assert.AreNotEqual(point, unexpectedPoint);
        }
    }

    public abstract class BaseLoopingTests2D : BaseLoopingTests
    {
        protected override void AddControlPoint(ILoopingSpline spline, float3 point)
        {
            Assert.NotNull(spline);

            ISpline2D spline2D = spline as ISpline2D;
            Assert.NotNull(spline2D, $"Failed to convert to 2D spline! Spline type was: {spline.GetType().Name}");

            int before = spline2D.ControlPointCount;
            spline2D.AddControlPoint(point.xy);

            Assert.Greater(spline2D.ControlPointCount, before, "Adding a point did not increase the control point count");
        }

        protected override float Length(float3 a, float3 b) => math.distance(a.xy, b.xy);

        protected override float3 GetProgress(ILoopingSpline spline, float progress)
        {
            ISpline2D spline2D = spline as ISpline2D;
            Assert.NotNull(spline2D);

            float2 point = spline2D.GetPoint(progress);
            return new float3(point.x, point.y, 0f);
        }

        protected override void CompareProgressEquals(ILoopingSpline spline, float progress, float3 expectedPoint, float tolerance = 0.00001f)
        {
            ISpline2D spline2D = spline as ISpline2D;
            Assert.NotNull(spline2D);

            float2 point = spline2D.GetPoint(progress);
            TestHelpers.CheckFloat2(point, expectedPoint.xy, tolerance);
        }

        protected override void CompareProgress(ILoopingSpline spline, float progress, float3 unexpectedPoint)
        {
            ISpline2D spline2D = spline as ISpline2D;
            Assert.NotNull(spline2D);

            float2 point = spline2D.GetPoint(progress);
            Assert.AreNotEqual(point, unexpectedPoint.xy);
        }
    }
}