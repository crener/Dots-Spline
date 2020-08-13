using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Crener.Spline.Test.Helpers;
using NUnit.Framework;
using Unity.Mathematics;

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
        public void SegmentChange([Range(2, 9)] int points)
        {
            IArkableSpline testSpline = PrepareSpline();
            const int pointLength = 20;

            float3 first = new float3(0);
            AddControlPoint(testSpline, first);

            for (int i = 1; i < points; i++)
            {
                float3 point = new float3(i * pointLength);
                AddControlPoint(testSpline, point);
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
        public void LengthIncreaseSwitchBeforeFirstPoint([Range(2, 9)] int points)
        {
            IArkableSpline testSpline = PrepareSpline();
            const int pointLength = 20;

            float3 first = new float3(0);
            AddControlPoint(testSpline, first);

            for (int i = 1; i < points; i++)
            {
                float3 point = new float3(i * pointLength);
                AddControlPoint(testSpline, point);
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
            AddControlPoint(testSpline, new float3(80));

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
            AddControlPoint(testSpline, a);
            AddControlPoint(testSpline, b);

            testSpline.ArkLength = 5f;
            testSpline.ArkParameterization = false;
            int originalCount = SplineSegmentPointCount(testSpline);

            ChangeArking(testSpline, false);
            int count = SplineSegmentPointCount(testSpline);
            Assert.AreEqual(originalCount, count, "This value shouldn't have changed");

            ChangeArking(testSpline, true);
            count = SplineSegmentPointCount(testSpline);
            Assert.AreEqual((int)math.ceil(Length(a, b) / 5f)+1, count);
        }

        [Test]
        public void ArkEnabledToDisabled()
        {
            IArkableSpline testSpline = PrepareSpline();
            
            float3 a = new float3(10f);
            float3 b = new float3(80f);
            AddControlPoint(testSpline, a);
            AddControlPoint(testSpline, b);

            int originalCount = SplineSegmentPointCount(testSpline);
            testSpline.ArkLength = 5f;
            testSpline.ArkParameterization = false;

            ChangeArking(testSpline, true);
            int count = SplineSegmentPointCount(testSpline);
            Assert.AreEqual((int)math.ceil(Length(a, b) / 5f)+1, count);

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
            AddControlPoint(testSpline, a);
            AddControlPoint(testSpline, b);

            testSpline.ArkLength = 5f;

            ChangeArking(testSpline, true);
            int count = SplineSegmentPointCount(testSpline);
            Assert.AreEqual((int)math.ceil(Length(a, b) / 5f)+1, count);
            
            testSpline.ArkLength = 2.5f;
            count = SplineSegmentPointCount(testSpline);
            Assert.AreEqual((int)math.ceil(Length(a, b) / 2.5f)+1, count);
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
            AddControlPoint(testSpline, a);
            AddControlPoint(testSpline, b);

            testSpline.ArkLength = 5f;
            int splineCount = SplineSegmentPointCount(testSpline);

            ChangeArking(testSpline, false);
            int count = SplineSegmentPointCount(testSpline);
            Assert.AreEqual(splineCount, count);
            
            testSpline.ArkLength = 2.5f;
            count = SplineSegmentPointCount(testSpline);
            Assert.AreEqual(splineCount, count);
        }
    }

    public abstract class BaseArkTests3D : BaseArkTests
    {
        protected override void AddControlPoint(IArkableSpline spline, float3 point)
        {
            Assert.NotNull(spline);

            ISpline3D spline3D = spline as ISpline3D;
            Assert.NotNull(spline3D, $"Failed to convert to 3D spline! Spline type was: {spline.GetType().Name}");

            int before = spline3D.ControlPointCount;
            spline3D.AddControlPoint(point);

            Assert.Greater(spline3D.ControlPointCount, before, "Adding a point did not increase the control point count");
        }

        protected override float Length(float3 a, float3 b) => math.distance(a, b);

        protected override float3 GetProgress(IArkableSpline spline, float progress)
        {
            ISpline3D spline3D = spline as ISpline3D;
            Assert.NotNull(spline3D);
            return spline3D.GetPoint(progress);
        }

        protected override void CompareProgressEquals(IArkableSpline spline, float progress, float3 expectedPoint,
            float tolerance = 0.00001f)
        {
            ISpline3D spline3D = spline as ISpline3D;
            Assert.NotNull(spline3D);

            float3 point = spline3D.GetPoint(progress);
            TestHelpers.CheckFloat3(point, expectedPoint, tolerance);
        }

        protected override void CompareProgress(IArkableSpline spline, float progress, float3 unexpectedPoint)
        {
            ISpline3D spline3D = spline as ISpline3D;
            Assert.NotNull(spline3D);

            float3 point = spline3D.GetPoint(progress);
            Assert.AreNotEqual(point, unexpectedPoint);
        }

        protected override int SplineSegmentPointCount(ISpline spline)
        {
            ISpline3D spline3D = (spline as ISpline3D);
            Assert.NotNull(spline3D, "Unable to convert spline");
            Assert.NotNull(spline3D.SplineEntityData, "spline failed to generate data");

            return spline3D.SplineEntityData.Value.Points.Length;
        }
    }

    public abstract class BaseArkTests2D : BaseArkTests
    {
        protected override void AddControlPoint(IArkableSpline spline, float3 point)
        {
            Assert.NotNull(spline);

            ISpline2D spline2D = spline as ISpline2D;
            Assert.NotNull(spline2D, $"Failed to convert to 2D spline! Spline type was: {spline.GetType().Name}");

            int before = spline2D.ControlPointCount;
            spline2D.AddControlPoint(point.xy);

            Assert.Greater(spline2D.ControlPointCount, before, "Adding a point did not increase the control point count");
        }

        protected override float Length(float3 a, float3 b) => math.distance(a.xy, b.xy);

        protected override float3 GetProgress(IArkableSpline spline, float progress)
        {
            ISpline2D spline2D = spline as ISpline2D;
            Assert.NotNull(spline2D);

            float2 point = spline2D.GetPoint(progress);
            return new float3(point.x, point.y, 0f);
        }

        protected override void CompareProgressEquals(IArkableSpline spline, float progress, float3 expectedPoint,
            float tolerance = 0.00001f)
        {
            ISpline2D spline2D = spline as ISpline2D;
            Assert.NotNull(spline2D);

            float2 point = spline2D.GetPoint(progress);
            TestHelpers.CheckFloat2(point, expectedPoint.xy, tolerance);
        }

        protected override void CompareProgress(IArkableSpline spline, float progress, float3 unexpectedPoint)
        {
            ISpline2D spline2D = spline as ISpline2D;
            Assert.NotNull(spline2D);

            float2 point = spline2D.GetPoint(progress);
            Assert.AreNotEqual(point, unexpectedPoint.xy);
        }

        protected override int SplineSegmentPointCount(ISpline spline)
        {
            ISpline2D spline3D = (spline as ISpline2D);
            Assert.NotNull(spline3D, "Unable to convert spline");
            Assert.NotNull(spline3D.SplineEntityData, "spline failed to generate data");

            return spline3D.SplineEntityData.Value.Points.Length;
        }
    }
}