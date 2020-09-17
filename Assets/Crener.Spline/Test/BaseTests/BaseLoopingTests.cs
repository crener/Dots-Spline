using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Crener.Spline.Test.BaseTests.TransferableTestBases;
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
        protected override ILoopingSpline PrepareSpline()
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
        private static SplineInteractionBase3D s_splineBase = new SplineInteractionBase3D();

        public override void AddControlPoint(ILoopingSpline spline, float3 point) =>
            s_splineBase.AddControlPoint(spline as ISimpleSpline3D, point);

        public override void InsertControlPoint(ILoopingSpline spline, int index, float3 point) =>
            s_splineBase.InsertControlPoint(spline as ISimpleSpline3D, index, point);

        public override float3 GetControlPoint(ILoopingSpline spline, int index, SplinePoint pointType) =>
            s_splineBase.GetControlPoint(spline as ISimpleSpline3D, index, pointType);

        public override void UpdateControlPoint(ILoopingSpline spline, int index, float3 newPoint, SplinePoint pointType) =>
            s_splineBase.UpdateControlPoint(spline as ISimpleSpline3D, index, newPoint, pointType);

        public override float3 GetProgress(ILoopingSpline spline, float progress) =>
            s_splineBase.GetProgress(spline as ISimpleSpline3D, progress);

        public override void CompareProgressEquals(ILoopingSpline spline, float progress, float3 expectedPoint,
            float tolerance = 0.00001f) =>
            s_splineBase.CompareProgressEquals(spline as ISimpleSpline3D, progress, expectedPoint, tolerance);

        public override void CompareProgressNotEquals(ILoopingSpline spline, float progress, float3 expectedPoint) =>
            s_splineBase.CompareProgressNotEquals(spline as ISimpleSpline3D, progress, expectedPoint);

        public override void ComparePoint(float3 expected, float3 actual, float tolerance = 0.00001f) =>
            s_splineBase.ComparePoint(expected, actual, tolerance);

        public override float Length(float3 a, float3 b) => s_splineBase.Length(a, b);
    }

    public abstract class BaseLoopingTests3DPlane : BaseLoopingTests
    {
        private static SplineInteractionBase3DPlane s_splineBase = new SplineInteractionBase3DPlane();

        public override void AddControlPoint(ILoopingSpline spline, float3 point) =>
            s_splineBase.AddControlPoint(spline as ISpline3DPlane, point);

        public override void InsertControlPoint(ILoopingSpline spline, int index, float3 point) =>
            s_splineBase.InsertControlPoint(spline as ISpline3DPlane, index, point);

        public override float3 GetControlPoint(ILoopingSpline spline, int index, SplinePoint pointType) =>
            s_splineBase.GetControlPoint(spline as ISpline3DPlane, index, pointType);

        public override void UpdateControlPoint(ILoopingSpline spline, int index, float3 newPoint, SplinePoint pointType) =>
            s_splineBase.UpdateControlPoint(spline as ISpline3DPlane, index, newPoint, pointType);

        public override float3 GetProgress(ILoopingSpline spline, float progress) =>
            s_splineBase.GetProgress(spline as ISpline3DPlane, progress);

        public override void CompareProgressEquals(ILoopingSpline spline, float progress, float3 expectedPoint,
            float tolerance = 0.00001f) =>
            s_splineBase.CompareProgressEquals(spline as ISpline3DPlane, progress, expectedPoint, tolerance);

        public override void CompareProgressNotEquals(ILoopingSpline spline, float progress, float3 expectedPoint) =>
            s_splineBase.CompareProgressNotEquals(spline as ISpline3DPlane, progress, expectedPoint);

        public override void ComparePoint(float3 expected, float3 actual, float tolerance = 0.00001f) =>
            s_splineBase.ComparePoint(actual, expected, tolerance);

        public override float Length(float3 a, float3 b) => s_splineBase.Length(a, b);
    }

    public abstract class BaseLoopingTests2D : BaseLoopingTests
    {
        private static SplineInteractionBase2D s_splineBase = new SplineInteractionBase2D();

        public override void AddControlPoint(ILoopingSpline spline, float3 point) =>
            s_splineBase.AddControlPoint(spline as ISpline2D, point);

        public override void InsertControlPoint(ILoopingSpline spline, int index, float3 point) =>
            s_splineBase.InsertControlPoint(spline as ISpline2D, index, point);

        public override float3 GetControlPoint(ILoopingSpline spline, int index, SplinePoint pointType) =>
            s_splineBase.GetControlPoint(spline as ISpline2D, index, pointType);

        public override void UpdateControlPoint(ILoopingSpline spline, int index, float3 newPoint, SplinePoint pointType) =>
            s_splineBase.UpdateControlPoint(spline as ISpline2D, index, newPoint, pointType);

        public override float3 GetProgress(ILoopingSpline spline, float progress) =>
            s_splineBase.GetProgress(spline as ISpline2D, progress);

        public override void CompareProgressEquals(ILoopingSpline spline, float progress, float3 expectedPoint,
            float tolerance = 0.00001f) =>
            s_splineBase.CompareProgressEquals(spline as ISpline2D, progress, expectedPoint, tolerance);

        public override void CompareProgressNotEquals(ILoopingSpline spline, float progress, float3 expectedPoint) =>
            s_splineBase.CompareProgressNotEquals(spline as ISpline2D, progress, expectedPoint);

        public override void ComparePoint(float3 expected, float3 actual, float tolerance = 0.00001f) =>
            s_splineBase.ComparePoint(expected, actual, tolerance);

        public override float Length(float3 a, float3 b) => s_splineBase.Length(a, b);
    }
}