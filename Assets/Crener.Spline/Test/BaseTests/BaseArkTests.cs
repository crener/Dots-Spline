using System.Collections.Generic;
using System.Linq;
using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Crener.Spline.Test.BaseTests.TransferableTestBases;
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
                else if(spline.ControlPointCount >= 2) Assert.AreEqual(SplineType.Linear, spline.SplineDataType);
            }
        }

        /// <summary>
        /// The amount of points in the spline data computed from ark parameterized 
        /// </summary>
        protected abstract int SplineSegmentPointCount(ISpline spline);

        /// <summary>
        /// Return an enumerator that goes over all points in the Arked spline
        /// </summary>
        /// <param name="spline">target spline</param>
        protected abstract IEnumerable<float3> PointData(ISpline spline);

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
        /// Make sure that the ark length setting is actually being respected
        /// This specific test ensures the minimum amount of points in a spline is valid
        /// </summary>
        [Test]
        public void ArkLengthConsistencyMin()
        {
            IArkableSpline testSpline = PrepareSpline();
            const float pointLength = 0.5f;

            float3 a = new float3(10);
            AddControlPointLocalSpace(testSpline, a);
            float3 b = new float3(20);
            AddControlPointLocalSpace(testSpline, b);

            testSpline.ArkLength = pointLength;
            ChangeArking(testSpline, true);

            ArkDistanceCheck(testSpline);

            float3[] pointData = PointData(testSpline).ToArray();
            ComparePoint(a, pointData[0]);
            ComparePoint(b, pointData[pointData.Length - 1]);
        }

        /// <summary>
        /// Make sure that the ark length setting is actually being respected
        /// This specific test ensures the Long splines are accurate
        /// </summary>
        [Test]
        public void ArkLengthConsistencyLong()
        {
            IArkableSpline testSpline = PrepareSpline();
            const float pointLength = 0.6f;

            float3 a = new float3(1);
            AddControlPointLocalSpace(testSpline, a);
            float3 b = new float3(5);
            AddControlPointLocalSpace(testSpline, b);
            float3 c = new float3(300);
            AddControlPointLocalSpace(testSpline, c);

            testSpline.ArkLength = pointLength;
            ChangeArking(testSpline, true);

            ArkDistanceCheck(testSpline);

            float3[] pointData = PointData(testSpline).ToArray();
            ComparePoint(a, pointData[0]);
            ComparePoint(c, pointData[pointData.Length - 1]);
        }

        /// <summary>
        /// Make sure that the ark length setting is actually being respected
        /// This specific test ensures the splines with many control points close together work correctly
        /// </summary>
        [Test]
        public void ArkLengthConsistencyManyPoints()
        {
            IArkableSpline testSpline = PrepareSpline();
            const float pointLength = 0.6f;

            float3 a = new float3(0);
            AddControlPointLocalSpace(testSpline, a);
            float3 b = new float3(5);
            AddControlPointLocalSpace(testSpline, b);
            float3 c = new float3(5.5);
            AddControlPointLocalSpace(testSpline, c);
            float3 d = new float3(6);
            AddControlPointLocalSpace(testSpline, d);
            float3 e = new float3(6.1);
            AddControlPointLocalSpace(testSpline, e);
            float3 f = new float3(7);
            AddControlPointLocalSpace(testSpline, f);
            float3 g = new float3(26);
            AddControlPointLocalSpace(testSpline, g);

            testSpline.ArkLength = pointLength;
            ChangeArking(testSpline, true);

            ArkDistanceCheck(testSpline);

            float3[] pointData = PointData(testSpline).ToArray();
            ComparePoint(a, pointData[0]);
            ComparePoint(g, pointData[pointData.Length - 1]);
        }

        private void ArkDistanceCheck(IArkableSpline testSpline)
        {
            float3[] pointData = PointData(testSpline).ToArray();
            Assert.GreaterOrEqual(pointData.Length, 2, "Need at least 2 points for this test to run!!");
            List<float> distances = new List<float>(pointData.Length - 1);

            // calculate averages
            float3 last = pointData[0];
            for (int i = 1; i < pointData.Length - 1; i++) // note last point is skipped as it's a buffer point with large splines
            {
                distances.Add(Length(pointData[i], last));
                last = pointData[i];
            }

            // make sure that the spline distance is roughly equal for all points, cause otherwise this setting is a bit fucked
            float average = distances.Sum() / distances.Count;
            float tolerance = testSpline.ArkLength / 10f;
            for (int i = 0; i < distances.Count; i++)
            {
                float pointDistance = distances[i];
                float averageDelta = math.abs(pointDistance - average);
                Assert.LessOrEqual(averageDelta, tolerance,
                    $"Points {i} => {i + 1} have a large distance between them compared to the average which means that the spline approximation " +
                    $"is inaccurate");
            }

            // last point is tested separately as it is used as the remainder in longer splines
            float lastPointLength = Length(pointData[pointData.Length - 2], pointData[pointData.Length - 1]);
            Assert.LessOrEqual(lastPointLength, average + tolerance);
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

            const float arkLength = 5f;
            testSpline.ArkLength = arkLength;
            testSpline.ArkParameterization = false;
            int originalCount = SplineSegmentPointCount(testSpline);

            ChangeArking(testSpline, false);
            int count = SplineSegmentPointCount(testSpline);
            Assert.AreEqual(originalCount, count, "This value shouldn't have changed");

            ChangeArking(testSpline, true);
            int newCount = SplineSegmentPointCount(testSpline);
            Assert.Greater(newCount, count);
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

            const float arkLength = 5f;
            testSpline.ArkLength = arkLength;
            testSpline.ArkParameterization = false;

            ChangeArking(testSpline, true);
            int count = SplineSegmentPointCount(testSpline);
            Assert.Greater(count, originalCount);

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

            testSpline.ArkLength = 2.5f;
            int newCount = SplineSegmentPointCount(testSpline);
            Assert.Greater(newCount, count);
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
            CompareProgressEquals(testSpline, 1f, a + move);
        }

        [Test]
        public void Translation2()
        {
            IArkableSpline testSpline = PrepareSpline();
            GameObject parent = new GameObject("Parent");
            ((MonoBehaviour) testSpline).transform.parent = parent.transform;
            parent.transform.position = new float3(-20f, 0f, 0f);

            float3 move = new float3(10f, 0f, 10f);
            ((MonoBehaviour) testSpline).transform.position = move;

            float3 a = new float3(80);
            AddControlPointLocalSpace(testSpline, a);

            ChangeArking(testSpline, true);
            testSpline.ArkLength = 1f;
            int count = SplineSegmentPointCount(testSpline);
            Assert.AreEqual(1, count);

            CompareProgressEquals(testSpline, 0f, a + move);
        }

        [Test]
        public void Translation3()
        {
            IArkableSpline testSpline = PrepareSpline();
            GameObject parent = new GameObject("Parent");
            ((MonoBehaviour) testSpline).transform.parent = parent.transform;
            parent.transform.position = new float3(-20f, 0f, 0f);

            // todo fix this as it really fucks up spline planes
            Assume.That(!(testSpline is ISpline3DPlane));

            float3 move = new float3(10f, 0f, 10f);
            ((MonoBehaviour) testSpline).transform.position = move;

            float3 a = new float3(80);
            AddControlPointLocalSpace(testSpline, a);
            float3 b = new float3(60);
            AddControlPointLocalSpace(testSpline, b);
            float3 c = new float3(100);
            AddControlPointLocalSpace(testSpline, c);

            ChangeArking(testSpline, true);
            testSpline.ArkLength = 1f;
            int count = SplineSegmentPointCount(testSpline);
            Assert.GreaterOrEqual(count, math.abs(80 - 60) + math.abs(60 - 100));

            CompareProgressEquals(testSpline, 0f, a + move);
            CompareProgressEquals(testSpline, 1f, c + move);
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

        public override float3 GetProgressWorld(IArkableSpline spline, float progress) =>
            s_splineBase.GetProgressWorld(spline as ISimpleSpline3D, progress);

        public override float3 GetProgressLocal(IArkableSpline spline, float progress) =>
            s_splineBase.GetProgressWorld(spline as ISimpleSpline3D, progress);

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

        protected override IEnumerable<float3> PointData(ISpline spline)
        {
            ISpline3D spline3D = (spline as ISpline3D);
            return spline3D.SplineEntityData3D.Value.Points;
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

        public override float3 GetProgressWorld(IArkableSpline spline, float progress) =>
            s_splineBase.GetProgressWorld(spline as ISpline3DPlane, progress);

        public override float3 GetProgressLocal(IArkableSpline spline, float progress) =>
            s_splineBase.GetProgressWorld(spline as ISpline3DPlane, progress);

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

        public override float3 GetProgressWorld(IArkableSpline spline, float progress) =>
            s_splineBase.GetProgressWorld(spline as ISpline2D, progress);

        public override float3 GetProgressLocal(IArkableSpline spline, float progress) =>
            s_splineBase.GetProgressWorld(spline as ISpline2D, progress);

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

        protected override IEnumerable<float3> PointData(ISpline spline)
        {
            ISpline2D spline3D = (spline as ISpline2D);

            foreach (float2 point in spline3D.SplineEntityData2D.Value.Points)
            {
                yield return new float3(point, 0f);
            }
        }
    }
}