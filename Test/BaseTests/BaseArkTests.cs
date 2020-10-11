using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Crener.Spline.Test.BaseTests.TransferableTestBases;
using Crener.Spline.Test.Helpers;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

namespace Crener.Spline.Test.BaseTests
{
    public abstract class BaseArkTests : TransferableTestSet<IArkableSpline>
    {
        protected void ChangeArking(IArkableSpline spline, bool newArkState)
        {
            spline.ArkParameterization = newArkState;
            Assert.AreEqual(newArkState, spline.ArkParameterization);

            if(newArkState)
            {
                if(spline.ControlPointCount == 0) Assert.AreEqual(SplineType.Empty, spline.SplineDataType);
                else if(spline.ControlPointCount == 1) Assert.AreEqual(SplineType.Single, spline.SplineDataType);
                else if(spline.ControlPointCount > 2) Assert.AreEqual(SplineType.Linear, spline.SplineDataType);
            }
        }

        /// <summary>
        /// The amount of points in the spline data computed from ark parameterized 
        /// </summary>
        protected abstract int SplineSegmentPointCount(ISpline spline);

        /// <summary>
        /// ensures that the segment count increases after ark parametrisation is enabled 
        /// </summary>
        [Test]
        public void SegmentChange([NUnit.Framework.Range(2, 9)] int points)
        {
            IArkableSpline testSpline = PrepareSpline();
            const int pointLength = 20;

            float3 first = new float3(0);
            AddControlPointLocalSpace(testSpline, first);

            for (int i = 1; i < points; i++)
            {
                float3 point = new float3(i * pointLength);
                AddControlPointLocalSpace(testSpline, point);
            }

            testSpline.ArkLength = pointLength / 10f;
            ChangeArking(testSpline, false);
            int normalCount = SplineSegmentPointCount(testSpline);

            ChangeArking(testSpline, true);
            int arkCount = SplineSegmentPointCount(testSpline);

            Assert.Greater(arkCount, normalCount);
        }

        /// <summary>
        /// ensures that the segment count decreases after ark parametrisation is disabled 
        /// </summary>
        [Test]
        public void LengthIncreaseSwitchBeforeFirstPoint([NUnit.Framework.Range(2, 9)] int points)
        {
            IArkableSpline testSpline = PrepareSpline();
            const int pointLength = 20;

            float3 first = new float3(0);
            AddControlPointLocalSpace(testSpline, first);

            for (int i = 1; i < points; i++)
            {
                float3 point = new float3(i * pointLength);
                AddControlPointLocalSpace(testSpline, point);
            }

            testSpline.ArkLength = pointLength / 10f;
            ChangeArking(testSpline, true);
            int arkCount = SplineSegmentPointCount(testSpline);

            ChangeArking(testSpline, false);
            int normalCount = SplineSegmentPointCount(testSpline);

            Assert.Greater(arkCount, normalCount);
        }

        [Test]
        public void NoPoint()
        {
            IArkableSpline testSpline = PrepareSpline();

            ChangeArking(testSpline, true);
            int count = SplineSegmentPointCount(testSpline);
            Assert.AreEqual(0, count);

            ChangeArking(testSpline, false);
            count = SplineSegmentPointCount(testSpline);
            Assert.AreEqual(0, count);
        }

        [Test]
        public void SinglePoint()
        {
            IArkableSpline testSpline = PrepareSpline();
            AddControlPointLocalSpace(testSpline, new float3(80));

            ChangeArking(testSpline, true);
            int count = SplineSegmentPointCount(testSpline);
            Assert.AreEqual(1, count);

            ChangeArking(testSpline, false);
            count = SplineSegmentPointCount(testSpline);
            Assert.AreEqual(1, count);
        }

        [Test]
        public void ArkDisableToEnabled()
        {
            IArkableSpline testSpline = PrepareSpline();

            float3 a = new float3(10f);
            float3 b = new float3(80f);
            AddControlPointLocalSpace(testSpline, a);
            AddControlPointLocalSpace(testSpline, b);

            testSpline.ArkLength = 5f;
            testSpline.ArkParameterization = false;
            int originalCount = SplineSegmentPointCount(testSpline);

            ChangeArking(testSpline, false);
            int count = SplineSegmentPointCount(testSpline);
            Assert.AreEqual(originalCount, count, "This value shouldn't have changed");

            ChangeArking(testSpline, true);
            count = SplineSegmentPointCount(testSpline);
            Assert.AreEqual((int) math.ceil(Length(a, b) / 5f) + 1, count);
        }

        [Test]
        public void ArkEnabledToDisabled()
        {
            IArkableSpline testSpline = PrepareSpline();

            float3 a = new float3(10f);
            float3 b = new float3(80f);
            AddControlPointLocalSpace(testSpline, a);
            AddControlPointLocalSpace(testSpline, b);

            int originalCount = SplineSegmentPointCount(testSpline);
            testSpline.ArkLength = 5f;
            testSpline.ArkParameterization = false;

            ChangeArking(testSpline, true);
            int count = SplineSegmentPointCount(testSpline);
            Assert.AreEqual((int) math.ceil(Length(a, b) / 5f) + 1, count);

            ChangeArking(testSpline, false);
            count = SplineSegmentPointCount(testSpline);
            Assert.AreEqual(originalCount, count);
        }

        [Test]
        public void ArkLengthChangeWhenEnabled()
        {
            IArkableSpline testSpline = PrepareSpline();

            float3 a = new float3(10f);
            float3 b = new float3(80f);
            AddControlPointLocalSpace(testSpline, a);
            AddControlPointLocalSpace(testSpline, b);

            testSpline.ArkLength = 5f;

            ChangeArking(testSpline, true);
            int count = SplineSegmentPointCount(testSpline);
            Assert.AreEqual((int) math.ceil(Length(a, b) / 5f) + 1, count);

            testSpline.ArkLength = 2.5f;
            count = SplineSegmentPointCount(testSpline);
            Assert.AreEqual((int) math.ceil(Length(a, b) / 2.5f) + 1, count);
        }

        /// <summary>
        /// amount of points is the same as ark length is modified when arking is disabled 
        /// </summary>
        [Test]
        public void ArkLengthChangeWhenDisabled()
        {
            IArkableSpline testSpline = PrepareSpline();

            float3 a = new float3(10f);
            float3 b = new float3(80f);
            AddControlPointLocalSpace(testSpline, a);
            AddControlPointLocalSpace(testSpline, b);

            testSpline.ArkLength = 5f;
            int splineCount = SplineSegmentPointCount(testSpline);

            ChangeArking(testSpline, false);
            int count = SplineSegmentPointCount(testSpline);
            Assert.AreEqual(splineCount, count);

            testSpline.ArkLength = 2.5f;
            count = SplineSegmentPointCount(testSpline);
            Assert.AreEqual(splineCount, count);
        }

        [Test]
        public void Translation()
        {
            IArkableSpline testSpline = PrepareSpline();
            float3 move = new float3(10f, 0f, 10f);
            ((MonoBehaviour) testSpline).transform.position = move;

            float3 a = new float3(80);
            AddControlPointLocalSpace(testSpline, a);

            ChangeArking(testSpline, true);
            int count = SplineSegmentPointCount(testSpline);
            Assert.AreEqual(1, count);

            CompareProgressEquals(testSpline, 0f, a + move);
        }
    }

    public abstract class BaseArkTests3D : BaseArkTests
    {
        private static SplineInteractionBase3D s_splineBase = new SplineInteractionBase3D();

        public override void AddControlPointLocalSpace(IArkableSpline spline, float3 point) =>
            s_splineBase.AddControlPointLocalSpace(spline as ISimpleSpline3D, point);

        public override void InsertControlPointWorldSpace(IArkableSpline spline, int index, float3 point) =>
            s_splineBase.InsertControlPointWorldSpace(spline as ISimpleSpline3D, index, point);

        public override void InsertControlPointLocalSpace(IArkableSpline spline, int index, float3 point) => 
            s_splineBase.InsertControlPointWorldSpace(spline as ISimpleSpline3D, index, point);

        public override float3 GetControlPoint(IArkableSpline spline, int index, SplinePoint pointType) =>
            s_splineBase.GetControlPoint(spline as ISimpleSpline3D, index, pointType);

        public override void UpdateControlPoint(IArkableSpline spline, int index, float3 newPoint, SplinePoint pointType) =>
            s_splineBase.UpdateControlPoint(spline as ISimpleSpline3D, index, newPoint, pointType);

        public override float3 GetProgress(IArkableSpline spline, float progress) =>
            s_splineBase.GetProgress(spline as ISimpleSpline3D, progress);

        public override void CompareProgressEquals(IArkableSpline spline, float progress, float3 expectedPoint,
            float tolerance = 0.00001f) =>
            s_splineBase.CompareProgressEquals(spline as ISimpleSpline3D, progress, expectedPoint, tolerance);

        public override void CompareProgressNotEquals(IArkableSpline spline, float progress, float3 expectedPoint) =>
            s_splineBase.CompareProgressNotEquals(spline as ISimpleSpline3D, progress, expectedPoint);

        public override void ComparePoint(float3 expected, float3 actual, float tolerance = 0.00001f) =>
            s_splineBase.ComparePoint(expected, actual, tolerance);

        public override float Length(float3 a, float3 b) => s_splineBase.Length(a, b);
        
        protected override int SplineSegmentPointCount(ISpline spline)
        {
            ISpline3D spline3D = (spline as ISpline3D);
            Assert.NotNull(spline3D, "Unable to convert spline");
            Assert.NotNull(spline3D.SplineEntityData3D, "spline failed to generate data");

            return spline3D.SplineEntityData3D.Value.Points.Length;
        }
    }

    public abstract class BaseArkTests3DPlane : BaseArkTests3D
    {
        private static SplineInteractionBase3DPlane s_splineBase = new SplineInteractionBase3DPlane();

        public override void AddControlPointLocalSpace(IArkableSpline spline, float3 point) =>
            s_splineBase.AddControlPointLocalSpace(spline as ISpline3DPlane, point);

        public override void InsertControlPointWorldSpace(IArkableSpline spline, int index, float3 point) =>
            s_splineBase.InsertControlPointWorldSpace(spline as ISpline3DPlane, index, point);

        public override float3 GetControlPoint(IArkableSpline spline, int index, SplinePoint pointType) =>
            s_splineBase.GetControlPoint(spline as ISpline3DPlane, index, pointType);

        public override void UpdateControlPoint(IArkableSpline spline, int index, float3 newPoint, SplinePoint pointType) =>
            s_splineBase.UpdateControlPoint(spline as ISpline3DPlane, index, newPoint, pointType);

        public override float3 GetProgress(IArkableSpline spline, float progress) =>
            s_splineBase.GetProgress(spline as ISpline3DPlane, progress);

        public override void CompareProgressEquals(IArkableSpline spline, float progress, float3 expectedPoint,
            float tolerance = 0.00001f) =>
            s_splineBase.CompareProgressEquals(spline as ISpline3DPlane, progress, expectedPoint, tolerance);

        public override void CompareProgressNotEquals(IArkableSpline spline, float progress, float3 expectedPoint) =>
            s_splineBase.CompareProgressNotEquals(spline as ISpline3DPlane, progress, expectedPoint);

        public override void ComparePoint(float3 expected, float3 actual, float tolerance = 0.00001f) =>
            s_splineBase.ComparePoint(actual, expected, tolerance);

        public override float Length(float3 a, float3 b) => s_splineBase.Length(a, b);
    }

    public abstract class BaseArkTests2D : BaseArkTests
    {
        private static SplineInteractionBase2D s_splineBase = new SplineInteractionBase2D();

        public override void AddControlPointLocalSpace(IArkableSpline spline, float3 point) =>
            s_splineBase.AddControlPointLocalSpace(spline as ISpline2D, point);

        public override void InsertControlPointWorldSpace(IArkableSpline spline, int index, float3 point) =>
            s_splineBase.InsertControlPointWorldSpace(spline as ISpline2D, index, point);

        public override void InsertControlPointLocalSpace(IArkableSpline spline, int index, float3 point) => 
            s_splineBase.InsertControlPointWorldSpace(spline as ISpline2D, index, point);

        public override float3 GetControlPoint(IArkableSpline spline, int index, SplinePoint pointType) =>
            s_splineBase.GetControlPoint(spline as ISpline2D, index, pointType);

        public override void UpdateControlPoint(IArkableSpline spline, int index, float3 newPoint, SplinePoint pointType) =>
            s_splineBase.UpdateControlPoint(spline as ISpline2D, index, newPoint, pointType);

        public override float3 GetProgress(IArkableSpline spline, float progress) =>
            s_splineBase.GetProgress(spline as ISpline2D, progress);

        public override void CompareProgressEquals(IArkableSpline spline, float progress, float3 expectedPoint,
            float tolerance = 0.00001f) =>
            s_splineBase.CompareProgressEquals(spline as ISpline2D, progress, expectedPoint, tolerance);

        public override void CompareProgressNotEquals(IArkableSpline spline, float progress, float3 expectedPoint) =>
            s_splineBase.CompareProgressNotEquals(spline as ISpline2D, progress, expectedPoint);

        public override void ComparePoint(float3 expected, float3 actual, float tolerance = 0.00001f) =>
            s_splineBase.ComparePoint(expected, actual, tolerance);

        public override float Length(float3 a, float3 b) => s_splineBase.Length(a, b);

        protected override int SplineSegmentPointCount(ISpline spline)
        {
            ISpline2D spline3D = (spline as ISpline2D);
            Assert.NotNull(spline3D, "Unable to convert spline");
            Assert.NotNull(spline3D.SplineEntityData2D, "spline failed to generate data");

            return spline3D.SplineEntityData2D.Value.Points.Length;
        }
    }
}