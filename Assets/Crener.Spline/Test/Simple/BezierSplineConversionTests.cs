using Crener.Spline.BezierSpline;
using Crener.Spline.BezierSpline.Entity;
using NUnit.Framework;
using Unity.Mathematics;

namespace Crener.Spline.Test.Simple
{
    /// <summary>
    /// Tests the <see cref="BezierSpline2DSimple"/>s ability to convert from a Bezier spline to an ark parameterized
    /// point to point spline.
    /// </summary>
    public class BezierSplineConversionTests : SharedBezierSplineTestBase
    {
        private const float c_requiredPrecision = 0.00001f;

        [Test]
        public void SplineTypeTest()
        {
            BezierSpline2DJobTest.BezierSpline2DSimpleInspector spline = CreateSpline();

            spline.ArkParameterization = false;
            Assert.IsFalse(spline.ArkParameterization);
            Assert.AreEqual(spline.SplineDataType, SplineType.Bezier);

            spline.ArkParameterization = true;
            Assert.IsTrue(spline.ArkParameterization);
            Assert.AreEqual(spline.SplineDataType, SplineType.PointToPoint);
        }

        [Test]
        public void ConvertedLength()
        {
            BezierSpline2DJobTest.BezierSpline2DSimpleInspector spline = CreateSpline();

            spline.AddControlPoint(new float2(10, 10));
            spline.AddControlPoint(new float2(20, 10));
            Assert.AreEqual(2, spline.ControlPointCount);
            spline.UpdateControlPoint(0, new float2(15, 10), SplinePoint.Post);
            spline.UpdateControlPoint(1, new float2(19, 10), SplinePoint.Pre);

            Assert.IsFalse(spline.ArkParameterization);
            Assert.AreEqual(10f, spline.Length());

            spline.ArkParameterization = true;
            Assert.IsTrue(spline.ArkParameterization);
            Assert.AreEqual(10f, spline.Length());
        }

        [Test]
        public void ConvertedDataLength()
        {
            BezierSpline2DJobTest.BezierSpline2DSimpleInspector spline = CreateSpline();

            spline.AddControlPoint(new float2(10, 10));
            spline.AddControlPoint(new float2(20, 10));
            Assert.AreEqual(2, spline.ControlPointCount);
            spline.UpdateControlPoint(0, new float2(15, 10), SplinePoint.Post);
            spline.UpdateControlPoint(1, new float2(19, 10), SplinePoint.Pre);

            Assert.IsFalse(spline.ArkParameterization);
            Spline2DData? bezierData = spline.SplineEntityData;
            Assert.NotNull(bezierData);
            Assert.AreEqual(10f, bezierData?.Length);

            spline.ArkParameterization = true;
            Assert.IsTrue(spline.ArkParameterization);
            bezierData = spline.SplineEntityData;
            Assert.NotNull(bezierData);
            Assert.AreEqual(10f, bezierData?.Length);
        }

        /// <summary>
        /// make sure that the ark data is not still the same bezier data after the ark setting is changed 
        /// </summary>
        [Test]
        public void ConvertedDataRecalculated()
        {
            BezierSpline2DJobTest.BezierSpline2DSimpleInspector spline = CreateSpline();

            spline.AddControlPoint(new float2(10, 10));
            spline.AddControlPoint(new float2(20, 10));
            spline.UpdateControlPoint(0, new float2(15, 10), SplinePoint.Post);
            spline.UpdateControlPoint(1, new float2(19, 10), SplinePoint.Pre);
            Assert.AreEqual(2, spline.ControlPointCount);

            Assert.IsFalse(spline.ArkParameterization);
            Spline2DData? bezierData = spline.SplineEntityData;
            Assert.NotNull(bezierData, "Failed to generate bezier data");
            int bezierHash = bezierData.GetHashCode();
            int bezierPointHash = bezierData.Value.Points.GetHashCode();
            int bezierTimeHash = bezierData.Value.Time.GetHashCode();

            spline.ArkParameterization = true;
            Assert.IsTrue(spline.ArkParameterization);
            Spline2DData? bezierData2 = spline.SplineEntityData;
            Assert.NotNull(bezierData, "Failed to generate p2p data");

            // moment of truth!
            const string unequalMessage = "ark parametrization has still resulted in identical data";
            Assert.AreNotSame(bezierData, bezierData2, "after ark parametrization data is still pointing to the same reference");
            Assert.AreNotEqual(bezierHash, bezierData2.GetHashCode(), unequalMessage);
            Assert.AreNotEqual(bezierPointHash, bezierData2?.Points.GetHashCode(), unequalMessage);
            Assert.AreNotEqual(bezierTimeHash, bezierData2?.Time.GetHashCode(), unequalMessage);
        }

        [Test]
        public void ConvertedSpline()
        {
            BezierSpline2DJobTest.BezierSpline2DSimpleInspector spline = CreateSpline();

            const float length = 10f;
            const float subDivision = 0.1f;
            const float yAxis = 10f;

            spline.AddControlPoint(new float2(10f, yAxis));
            spline.AddControlPoint(new float2(10f + length, yAxis));
            const float sharedMidPoint = 10f + (length * 0.9f);
            spline.UpdateControlPoint(0, new float2(sharedMidPoint, yAxis), SplinePoint.Post);
            spline.UpdateControlPoint(1, new float2(sharedMidPoint, yAxis), SplinePoint.Pre);
            Assert.AreEqual(2, spline.ControlPointCount);
            Assert.AreEqual(length, spline.SplineEntityData?.Length, "unexpected length!");

            spline.ArkParameterization = true;
            spline.ArkLength = subDivision;
            Assert.AreEqual(length, spline.SplineEntityData?.Length, "unexpected length!");

            // make sure that points converge from left to right, otherwise ark param wouldn't be required ;)
            float2 left1 = spline.GetPoint(0f);
            float2 left2 = spline.GetPoint(0.1f);
            float left = math.distance(left1, left2);
            float2 right1 = spline.GetPoint(0.9f);
            float2 right2 = spline.GetPoint(1f);
            float right = math.distance(right1, right2);
            Assert.Greater(left, right, $"Left Delta '{left}' should be grater than '{right}'");

            // in sure that the point count is as expected
            // if there where multiple control points there might be more resulting points than just totalLength / ark length
            Assert.AreEqual((int) (length / subDivision) + 1, spline.SplineEntityData?.Points.Length);

            // if y-axis has shifted at all we know that something has gone very wrong
            Assert.NotNull(spline.SplineEntityData);
            foreach (float2 point in spline.SplineEntityData?.Points)
            {
                Assert.IsTrue(math.abs(yAxis - point.y) < c_requiredPrecision);
            }

            // check that the left side doesn't converge and is separated uniformly
            left1 = spline.GetPoint(0f); //todo make these use the spline point data
            left2 = spline.GetPoint(0.1f);
            left = math.distance(left1, left2);
            right1 = spline.GetPoint(0.9f);
            right2 = spline.GetPoint(1f);
            right = math.distance(right1, right2);
            Assert.Less(left, right, $"Left Delta '{left}' should be less than '{right}'");
        }

        [Test]
        public void ConvertedSpline3PointComparison()
        {
            BezierSpline2DJobTest.BezierSpline2DSimpleInspector spline = CreateSpline();

            const float length = 10f;
            const float subDivision = length / 2f;
            const float yAxis = 10f;
            const float start = 10f;
            const float end = start + length;

            spline.AddControlPoint(new float2(start, yAxis));
            spline.AddControlPoint(new float2(end, yAxis));
            Assert.AreEqual(2, spline.ControlPointCount);
            Assert.AreEqual(length, spline.SplineEntityData?.Length, "unexpected length!");

            spline.ArkLength = subDivision;
            spline.ArkParameterization = true;
            Assert.AreEqual(length, spline.SplineEntityData?.Length, "unexpected length!");

            // there should be exactly 3 points; start, middle, end
            float2[] points = spline.SplineEntityData?.Points.ToArray();
            Assert.NotNull(points);
            Assert.AreEqual(3, points.Length);

            // exact expected points from the conversion process
            Assert.AreEqual(start, points[0].x);
            Assert.AreEqual(yAxis, points[0].y);
            Assert.AreEqual(start + ((end - start) / 2), points[1].x);
            Assert.AreEqual(yAxis, points[1].y);
            Assert.AreEqual(end, points[2].x);
            Assert.AreEqual(yAxis, points[2].y);
        }

        [Test]
        public void ConvertedSpline4PointComparison()
        {
            BezierSpline2DJobTest.BezierSpline2DSimpleInspector spline = CreateSpline();

            const float length = 10f;
            const float subDivision = length / 3f;
            const float yAxis = 10f;
            const float start = 10f;
            const float end = start + length;

            spline.AddControlPoint(new float2(start, yAxis));
            spline.AddControlPoint(new float2(end, yAxis));
            Assert.AreEqual(2, spline.ControlPointCount);
            Assert.AreEqual(length, spline.SplineEntityData?.Length, "unexpected length!");

            spline.ArkParameterization = true;
            spline.ArkLength = subDivision;
            Assert.AreEqual(length, spline.SplineEntityData?.Length, "unexpected length!");

            // there should be exactly 3 points; start, middle, end
            float2[] points = spline.SplineEntityData?.Points.ToArray();
            Assert.NotNull(points);
            Assert.AreEqual(4, points.Length);

            // exact expected points from the conversion process
            Assert.AreEqual(start, points[0].x);
            Assert.AreEqual(yAxis, points[0].y);

            Assert.AreEqual(start + (((end - start) / 3) * 1), points[1].x);
            Assert.AreEqual(yAxis, points[1].y);

            Assert.AreEqual(start + (((end - start) / 3) * 2), points[2].x);
            Assert.AreEqual(yAxis, points[2].y);

            Assert.AreEqual(end, points[3].x);
            Assert.AreEqual(yAxis, points[3].y);
        }

        [Test]
        public void ArkDistance()
        {
            BezierSpline2DJobTest.BezierSpline2DSimpleInspector spline = CreateSpline();

            const float length = 10f;
            const float subDivision = 0.1f;
            const float yAxis = 10f;

            spline.AddControlPoint(new float2(10f, yAxis));
            spline.AddControlPoint(new float2(10f + length, yAxis));
            float sharedMidPoint = 10f + (length * 0.9f);
            spline.UpdateControlPoint(0, new float2(sharedMidPoint, yAxis), SplinePoint.Post);
            spline.UpdateControlPoint(1, new float2(sharedMidPoint, yAxis), SplinePoint.Pre);

            spline.ArkParameterization = true;
            spline.ArkLength = subDivision;
            Assert.AreEqual(length, spline.SplineEntityData?.Length, "unexpected length!");

            float2[] array = spline.SplineEntityData?.Points.ToArray();
            Assert.NotNull(array);
            for (int i = 1; i < array.Length; i++)
            {
                float2 previous = array[i - 1];
                float2 next = array[i];

                float distance = math.distance(previous, next);
                Assert.IsTrue(math.abs(subDivision - distance) < c_requiredPrecision,
                    $"Expected point distance: {subDivision:N7}, but was: {distance:N7}! Index {i}");
            }
        }

        [Test]
        public void ArkParameterizationDistanceChange()
        {
            BezierSpline2DJobTest.BezierSpline2DSimpleInspector spline = CreateSpline();

            spline.AddControlPoint(new float2(10f, 10f));
            spline.AddControlPoint(new float2(20f, 10f));

            spline.ArkParameterization = true;
            spline.ArkLength = 0.1f;
            Assert.NotNull(spline.SplineEntityData);

            int dataCount = spline.SplineEntityData.Value.Points.Length;
            Assert.Greater(dataCount, 2);

            // more distance between points means less points in total
            spline.ArkLength = 1f;
            Assert.Less(spline.SplineEntityData.Value.Points.Length, dataCount);
        }
    }
}