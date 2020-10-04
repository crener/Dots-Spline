using System;
using Crener.Spline.BezierSpline;
using Crener.Spline.BezierSpline.Jobs;
using Crener.Spline.Common;
using Crener.Spline.Common.DataStructs;
using Crener.Spline.Common.Interfaces;
using Crener.Spline.Linear.Jobs._2D;
using Crener.Spline.Test._2D.Bezier.TestAdapters;
using Crener.Spline.Test._2D.Bezier.TestTypes;
using Crener.Spline.Test.Helpers;
using NUnit.Framework;
using Unity.Mathematics;

namespace Crener.Spline.Test._2D.Bezier
{
    /// <summary>
    /// Tests the <see cref="BezierSpline2DSimple"/>s ability to convert from a Bezier spline to an ark parameterized
    /// point to point spline.
    /// </summary>
    public class BezierSplineArkConversionTests : SharedBezierSplineTestBase
    {
        private const float c_requiredPrecision = 0.00001f;

        [Test]
        public void SplineTypeTest()
        {
            MeaninglessTestWrapper2.TestBezierSpline2DSimpleJob spline =
                (MeaninglessTestWrapper2.TestBezierSpline2DSimpleJob) CreateSpline();

            spline.ArkParameterization = false;
            Assert.IsFalse(spline.ArkParameterization);
            Assert.AreEqual(spline.SplineDataType, SplineType.Empty);
            
            spline.AddControlPoint(new float2(3));
            Assert.IsFalse(spline.ArkParameterization);
            Assert.AreEqual(spline.SplineDataType, SplineType.Single);
            
            spline.AddControlPoint(new float2(5));
            Assert.IsFalse(spline.ArkParameterization);
            Assert.AreEqual(spline.SplineDataType, SplineType.Bezier);

            spline.ArkParameterization = true;
            Assert.IsTrue(spline.ArkParameterization);
            Assert.AreEqual(spline.SplineDataType, SplineType.Linear);
        }

        [Test]
        public void ConvertedLength()
        {
            MeaninglessTestWrapper2.TestBezierSpline2DSimpleJob spline =
                (MeaninglessTestWrapper2.TestBezierSpline2DSimpleJob) CreateSpline();

            spline.AddControlPoint(new float2(10, 10));
            spline.AddControlPoint(new float2(20, 10));
            Assert.AreEqual(2, spline.ControlPointCount);
            spline.UpdateControlPointLocal(0, new float2(15, 10), SplinePoint.Post);
            spline.UpdateControlPointLocal(1, new float2(19, 10), SplinePoint.Pre);

            Assert.IsFalse(spline.ArkParameterization);
            Assert.AreEqual(10f, spline.Length());

            spline.ArkParameterization = true;
            Assert.IsTrue(spline.ArkParameterization);
            Assert.AreEqual(10f, spline.Length());
        }

        [Test]
        public void ConvertedDataLength()
        {
            MeaninglessTestWrapper2.TestBezierSpline2DSimpleJob spline =
                (MeaninglessTestWrapper2.TestBezierSpline2DSimpleJob) CreateSpline();

            spline.AddControlPoint(new float2(10, 10));
            spline.AddControlPoint(new float2(20, 10));
            Assert.AreEqual(2, spline.ControlPointCount);
            spline.UpdateControlPointLocal(0, new float2(15, 10), SplinePoint.Post);
            spline.UpdateControlPointLocal(1, new float2(19, 10), SplinePoint.Pre);

            Assert.IsFalse(spline.ArkParameterization);
            Spline2DData? bezierData = spline.SplineEntityData2D;
            Assert.NotNull(bezierData);
            Assert.AreEqual(10f, bezierData?.Length);

            spline.ArkParameterization = true;
            Assert.IsTrue(spline.ArkParameterization);
            bezierData = spline.SplineEntityData2D;
            Assert.NotNull(bezierData);
            Assert.AreEqual(10f, bezierData?.Length);
        }

        /// <summary>
        /// make sure that the ark data is not still the same bezier data after the ark setting is changed 
        /// </summary>
        [Test]
        public void ConvertedDataRecalculated()
        {
            MeaninglessTestWrapper2.TestBezierSpline2DSimpleJob spline =
                (MeaninglessTestWrapper2.TestBezierSpline2DSimpleJob) CreateSpline();

            spline.AddControlPoint(new float2(10, 10));
            spline.AddControlPoint(new float2(20, 10));
            spline.UpdateControlPointLocal(0, new float2(15, 10), SplinePoint.Post);
            spline.UpdateControlPointLocal(1, new float2(19, 10), SplinePoint.Pre);
            Assert.AreEqual(2, spline.ControlPointCount);

            Assert.IsFalse(spline.ArkParameterization);
            Spline2DData? bezierData = spline.SplineEntityData2D;
            Assert.NotNull(bezierData, "Failed to generate bezier data");
            int bezierHash = bezierData.GetHashCode();
            int bezierPointHash = bezierData.Value.Points.GetHashCode();
            int bezierTimeHash = bezierData.Value.Time.GetHashCode();

            spline.ArkParameterization = true;
            Assert.IsTrue(spline.ArkParameterization);
            Spline2DData? bezierData2 = spline.SplineEntityData2D;
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
            MeaninglessTestWrapper2.TestBezierSpline2DSimpleJob spline =
                (MeaninglessTestWrapper2.TestBezierSpline2DSimpleJob) CreateSpline();

            const float length = 10f;
            const float subDivision = 0.1f;
            const float yAxis = 10f;

            spline.AddControlPoint(new float2(10f, yAxis));
            spline.AddControlPoint(new float2(10f + length, yAxis));
            const float sharedMidPoint = 10f + (length * 0.9f);
            spline.UpdateControlPointLocal(0, new float2(sharedMidPoint, yAxis), SplinePoint.Post);
            spline.UpdateControlPointLocal(1, new float2(sharedMidPoint, yAxis), SplinePoint.Pre);
            Assert.AreEqual(2, spline.ControlPointCount);
            Assert.AreEqual(length, spline.SplineEntityData2D?.Length, "unexpected length!");

            // make sure that points converge from left to right, otherwise ark param wouldn't be required ;)
            float2 left1 = spline.Get2DPoint(0f);
            float2 left2 = spline.Get2DPoint(0.1f);
            float left = math.distance(left1, left2);
            float2 right1 = spline.Get2DPoint(0.9f);
            float2 right2 = spline.Get2DPoint(1f);
            float right = math.distance(right1, right2);
            Assert.Greater(left, right, $"Left Delta '{left}' should be grater than right delta '{right}'");

            // enable ark parameterization 
            spline.ArkParameterization = true;
            spline.ArkLength = subDivision;
            Assert.AreEqual(length, spline.SplineEntityData2D?.Length, "unexpected length!");

            // in sure that the point count is as expected
            // if there where multiple control points there might be more resulting points than just totalLength / ark length
            Assert.AreEqual((int) (length / subDivision) + 1, spline.SplineEntityData2D?.Points.Length);

            // if y-axis has shifted at all we know that something has gone very wrong
            Assert.NotNull(spline.SplineEntityData2D);
            foreach (float2 point in spline.SplineEntityData2D?.Points)
            {
                Assert.IsTrue(math.abs(yAxis - point.y) < c_requiredPrecision);
            }

            // check that the left side doesn't converge and is separated uniformly
            left1 = getPointViaJob(spline, 0f);
            left2 = getPointViaJob(spline, 0.1f);
            left = math.distance(left1, left2);

            right1 = getPointViaJob(spline, 0.9f);
            right2 = getPointViaJob(spline, 1f);
            right = math.distance(right1, right2);

            Assert.AreEqual(left, right, $"Left Delta '{left}' should be the same as right delta '{right}'");
        }

        [Test]
        public void ConvertedSpline3PointComparison()
        {
            MeaninglessTestWrapper2.TestBezierSpline2DSimpleJob spline =
                (MeaninglessTestWrapper2.TestBezierSpline2DSimpleJob) CreateSpline();

            const float length = 10f;
            const float subDivision = length / 2f;
            const float yAxis = 10f;
            const float start = 10f;
            const float end = start + length;

            spline.AddControlPoint(new float2(start, yAxis));
            spline.AddControlPoint(new float2(end, yAxis));
            Assert.AreEqual(2, spline.ControlPointCount);
            Assert.AreEqual(length, spline.SplineEntityData2D?.Length, "unexpected length!");

            spline.ArkLength = subDivision;
            spline.ArkParameterization = true;
            Assert.AreEqual(length, spline.SplineEntityData2D?.Length, "unexpected length!");

            // there should be exactly 3 points; start, middle, end
            float2[] points = spline.SplineEntityData2D?.Points.ToArray();
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
            MeaninglessTestWrapper2.TestBezierSpline2DSimpleJob spline =
                (MeaninglessTestWrapper2.TestBezierSpline2DSimpleJob) CreateSpline();

            const float length = 10f;
            const float subDivision = length / 3f;
            const float yAxis = 10f;
            const float start = 10f;
            const float end = start + length;

            spline.AddControlPoint(new float2(start, yAxis));
            spline.AddControlPoint(new float2(end, yAxis));
            Assert.AreEqual(2, spline.ControlPointCount);
            Assert.AreEqual(length, spline.SplineEntityData2D?.Length, "unexpected length!");

            spline.ArkParameterization = true;
            spline.ArkLength = subDivision;
            Assert.AreEqual(length, spline.SplineEntityData2D?.Length, "unexpected length!");

            // there should be exactly 3 points; start, middle, end
            float2[] points = spline.SplineEntityData2D?.Points.ToArray();
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
        public void Points()
        {
            MeaninglessTestWrapper2.TestBezierSpline2DSimpleJob spline =
                (MeaninglessTestWrapper2.TestBezierSpline2DSimpleJob) CreateSpline();

            const float subDivision = 0.2f;

            float2 a = new float2(20f, 5f);
            spline.AddControlPoint(a);
            float2 b = new float2(10f, 5f);
            spline.AddControlPoint(b);
            Assert.AreEqual(2, spline.ControlPointCount);

            spline.ArkParameterization = true;
            spline.ArkLength = subDivision;
            Assert.AreEqual(math.distance(a, b), spline.SplineEntityData2D?.Length, "unexpected length!");

            // exact expected points from the conversion process
            float2 point = getPointViaJob(spline, 0f);
            Assert.AreEqual(a.x, point.x);
            Assert.AreEqual(a.y, point.y);

            point = getPointViaJob(spline, 0.5f);
            Assert.AreEqual((b + ((a - b) / 2f)).x, point.x);
            Assert.AreEqual((b + ((a - b) / 2f)).y, point.y);

            point = getPointViaJob(spline, 1f);
            Assert.AreEqual(b.x, point.x);
            Assert.AreEqual(b.y, point.y);
        }

        [Test]
        public void Points2()
        {
            PointTest(new float2(10f, 10f), new float2(20f, 20f));
        }

        //[Test]
        public void Points3()
        {
            PointTest(new float2(10f, 10f), new float2(20f, 60f));
        }

        [Test]
        public void Points4()
        {
            PointTest(new float2(-10f, -10f), new float2(0f, 0f));
        }

        public void PointTest(float2 a, float2 b)
        {
            MeaninglessTestWrapper2.TestBezierSpline2DSimpleJob spline =
                (MeaninglessTestWrapper2.TestBezierSpline2DSimpleJob) CreateSpline();

            const float subDivision = 0.1f;

            // create a large 'S' via spline points
            spline.AddControlPoint(a);
            spline.AddControlPoint(b);
            spline.UpdateControlPointLocal(0, new float2(b.x, a.x), SplinePoint.Post);
            spline.UpdateControlPointLocal(1, new float2(a.x, b.x), SplinePoint.Pre);
            Assert.AreEqual(2, spline.ControlPointCount);

            spline.ArkLength = subDivision;
            spline.ArkParameterization = true;
            Assert.AreEqual(spline.Length, spline.SplineEntityData2D?.Length, "unexpected length!");

            // exact expected points from the conversion process
            TestHelpers.CheckFloat2(a, getPointViaJob(spline, 0f));
            float2 point = getPointViaJob(spline, 0.5f);
            TestHelpers.CheckFloat2((a + ((b - a) / 2f)), point, 0.001f);

            TestHelpers.CheckFloat2(b, getPointViaJob(spline, 1f));
            TestHelpers.CheckFloat2(b, getPointViaJob(spline, 2f));
        }

        [Test]
        public void ArkDistance()
        {
            MeaninglessTestWrapper2.TestBezierSpline2DSimpleJob spline =
                (MeaninglessTestWrapper2.TestBezierSpline2DSimpleJob) CreateSpline();

            const float length = 10f;
            const float subDivision = 0.1f;
            const float yAxis = 10f;

            spline.AddControlPoint(new float2(10f, yAxis));
            spline.AddControlPoint(new float2(10f + length, yAxis));
            float sharedMidPoint = 10f + (length * 0.9f);
            spline.UpdateControlPointLocal(0, new float2(sharedMidPoint, yAxis), SplinePoint.Post);
            spline.UpdateControlPointLocal(1, new float2(sharedMidPoint, yAxis), SplinePoint.Pre);

            spline.ArkParameterization = true;
            spline.ArkLength = subDivision;
            Assert.AreEqual(length, spline.SplineEntityData2D?.Length, "unexpected length!");

            float2[] array = spline.SplineEntityData2D?.Points.ToArray();
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
            MeaninglessTestWrapper2.TestBezierSpline2DSimpleJob spline =
                (MeaninglessTestWrapper2.TestBezierSpline2DSimpleJob) CreateSpline();

            spline.AddControlPoint(new float2(10f, 10f));
            spline.AddControlPoint(new float2(20f, 10f));

            spline.ArkParameterization = true;
            spline.ArkLength = 0.1f;
            Assert.NotNull(spline.SplineEntityData2D);

            int dataCount = spline.SplineEntityData2D.Value.Points.Length;
            Assert.Greater(dataCount, 2);

            // more distance between points means less points in total
            spline.ArkLength = 1f;
            Assert.Less(spline.SplineEntityData2D.Value.Points.Length, dataCount);
        }

        private float2 getPointViaJob(ISpline2D spline, float progress)
        {
            switch (spline.SplineDataType)
            {
                case SplineType.Bezier:
                    BezierSpline2DPointJob bzSpline = new BezierSpline2DPointJob
                    {
                        Spline = spline.SplineEntityData2D.Value,
                        SplineProgress = new SplineProgress {Progress = progress}
                    };
                    bzSpline.Execute();
                    return bzSpline.Result;
                case SplineType.Linear:
                    LinearSpline2DPointJob pSpline = new LinearSpline2DPointJob
                    {
                        Spline = spline.SplineEntityData2D.Value,
                        SplineProgress = new SplineProgress {Progress = progress}
                    };
                    pSpline.Execute();
                    return pSpline.Result;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [Test]
        public void PointGeneration()
        {
            MeaninglessTestWrapper2.TestBezierSpline2DSimpleJob spline =
                (MeaninglessTestWrapper2.TestBezierSpline2DSimpleJob) CreateSpline();

            float2 a = new float2(0f, 60f);
            float2 b = new float2(100f, 60f);
            spline.AddControlPoint(a);
            spline.AddControlPoint(b);
            Assert.AreEqual(2, spline.ControlPointCount);

            spline.ArkLength = 1;
            spline.ArkParameterization = true;

            Assert.AreEqual(spline.Length, spline.SplineEntityData2D.Value.Length, "unexpected length!");
            Assert.NotNull(spline.SplineEntityData2D);
            Spline2DData splineData = spline.SplineEntityData2D.Value;

            Assert.AreEqual(101, splineData.Points.Length);
            for (int i = 0; i <= 100; i++)
            {
                float2 point = splineData.Points[i];
                TestHelpers.CheckFloat2(new float2(i, 60f), point, 0.0001f);
            }
        }

        [Test]
        public void PointGeneration2()
        {
            MeaninglessTestWrapper2.TestBezierSpline2DSimpleJob spline =
                (MeaninglessTestWrapper2.TestBezierSpline2DSimpleJob) CreateSpline();

            float2 a = new float2(0f, 0f);
            float2 b = new float2(100f, 100f);
            spline.AddControlPoint(a);
            spline.AddControlPoint(b);
            spline.UpdateControlPointLocal(0, new float2(1f, 1f), SplinePoint.Post);
            spline.UpdateControlPointLocal(1, new float2(99f, 99f), SplinePoint.Pre);
            Assert.AreEqual(2, spline.ControlPointCount);

            spline.ArkLength = math.length(new float2(1f, 1f));
            spline.ArkParameterization = true;
            Assert.NotNull(spline.SplineEntityData2D);
            Spline2DData splineData = spline.SplineEntityData2D.Value;

            Assert.AreEqual(101, splineData.Points.Length);
            for (int i = 0; i <= 100; i++)
            {
                float2 point = splineData.Points[i];
                TestHelpers.CheckFloat2(new float2(i, i), point, 0.0003f);
            }
        }
    }
}