using System.Collections.Generic;
using Crener.Spline.BezierSpline;
using Crener.Spline.BezierSpline.Jobs;
using Crener.Spline.Common;
using Crener.Spline.Common.DataStructs;
using Crener.Spline.Test._2D.Bezier.TestAdapters;
using NUnit.Framework;
using Unity.Mathematics;

namespace Crener.Spline.Test._2D.Bezier
{
    public class BezierSpline2DJobTest : SharedBezierSplineTestBase
    {
        [Test]
        public void Point()
        {
            BezierSpline2DSimpleInspector bezierSpline = CreateSpline();

            float2 a = float2.zero;
            bezierSpline.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            bezierSpline.AddControlPoint(b);

            Spline2DData data = bezierSpline.ConvertJobData();
            Assert.AreEqual(1f, bezierSpline.Length());
            Assert.AreEqual(1f, data.Length);

            Assert.AreEqual(1, bezierSpline.Times.Count);
            Assert.AreEqual(1, data.Time.Length);
            Assert.AreEqual(1f, bezierSpline.Times[0]);
            Assert.AreEqual(1f, data.Time[0]);
            
            BezierSpline2DPointJob job = new BezierSpline2DPointJob()
            {
                Spline = data,
                SplineProgress = new SplineProgress() { Progress = -0.5f }
            };

            CheckFloatJob(a, job, -0.5f);
            CheckFloatJob(a, job, 0f);
            CheckFloatJob(new float2(0.5f, 0f), job, 0.5f);
            CheckFloatJob(b, job, 5f);
        }

        [Test]
        public void Point2()
        {
            BezierSpline2DSimpleInspector bezierSpline = CreateSpline();

            float2 a = float2.zero;
            bezierSpline.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            bezierSpline.AddControlPoint(b);
            float2 c = new float2(2f, 0f);
            bezierSpline.AddControlPoint(c);

            Spline2DData data = bezierSpline.ConvertJobData();
            Assert.AreEqual(2f, bezierSpline.Length());
            Assert.AreEqual(2f, data.Length);

            Assert.AreEqual(2, bezierSpline.Times.Count);
            Assert.AreEqual(2, data.Time.Length);
            Assert.AreEqual(0.5f, bezierSpline.Times[0]);
            Assert.AreEqual(0.5f, data.Time[0]);
            Assert.AreEqual(1f, bezierSpline.Times[1]);
            Assert.AreEqual(1f, data.Time[1]);

            BezierSpline2DPointJob job = new BezierSpline2DPointJob()
            {
                Spline = data,
                SplineProgress = new SplineProgress() { Progress = -0.5f }
            };

            CheckFloatJob(a, job, -0.5f);
            CheckFloatJob(a, job, 0f);
            CheckFloatJob(new float2(0.5f, 0f), job, 0.25f);
            CheckFloatJob(new float2(1f, 0f), job, 0.5f);
            //CheckFloatJob(new float2(1.2f, 0f), job, 0.6f);
            CheckFloatJob(new float2(2f, 0f), job, 1f);
            CheckFloatJob(new float2(2f, 0f), job, 5f);
        }

        [Test]
        public void Point3()
        {
            BezierSpline2DSimpleInspector bezierSpline = CreateSpline();

            float2 a = float2.zero;
            bezierSpline.AddControlPoint(a);
            float2 b = new float2(2.5f, 0f);
            bezierSpline.AddControlPoint(b);
            float2 c = new float2(7.5f, 0f);
            bezierSpline.AddControlPoint(c);
            float2 d = new float2(10f, 0f);
            bezierSpline.AddControlPoint(d);

            Spline2DData data = bezierSpline.ConvertJobData();
            Assert.AreEqual(10f, bezierSpline.Length());
            Assert.AreEqual(10f, data.Length);

            Assert.AreEqual(3, bezierSpline.Times.Count);
            Assert.AreEqual(3, data.Time.Length);
            Assert.AreEqual(0.25f, bezierSpline.Times[0]);
            Assert.AreEqual(0.25f, data.Time[0]);
            Assert.AreEqual(0.75f, bezierSpline.Times[1]);
            Assert.AreEqual(0.75f, data.Time[1]);
            Assert.AreEqual(1f, bezierSpline.Times[2]);

            BezierSpline2DPointJob job = new BezierSpline2DPointJob()
            {
                Spline = data,
                SplineProgress = new SplineProgress() { Progress = -0.5f }
            };

            CheckFloatJob(a, job, -0.5f);
            CheckFloatJob(a, job, 0f);
            CheckFloatJob(new float2(2.5f, 0f), job, 0.25f);
            CheckFloatJob(new float2(5f, 0f), job, 0.5f);
            CheckFloatJob(new float2(10f, 0f), job, 1f);
            CheckFloatJob(new float2(10f, 0f), job, 5f);
        }

        /// <summary>
        /// Tests data generated for 
        /// </summary>
        [Test]
        public void DataEquality()
        {
            BezierSpline2DSimpleInspector bezierSpline = CreateSpline();

            Spline2DData data = bezierSpline.ConvertJobData();
            Assert.AreEqual(bezierSpline.Length(), data.Length);
            Assert.AreEqual(bezierSpline.Times.Count, data.Time.Length);
            Assert.AreEqual(bezierSpline.ControlPoints.Count, data.Points.Length);
            bezierSpline.ClearData();

            {
                float2 a = float2.zero;
                bezierSpline.AddControlPoint(a);

                data = bezierSpline.ConvertJobData();
                Assert.AreEqual(bezierSpline.Length(), data.Length);
                Assert.AreEqual(bezierSpline.Times.Count, data.Time.Length);
                for (int i = 0; i < bezierSpline.Times.Count; i++)
                    Assert.AreEqual(bezierSpline.Times[i], data.Time[i]);
                Assert.AreEqual(bezierSpline.ControlPoints.Count, data.Points.Length);
                for (int i = 0; i < bezierSpline.ControlPoints.Count; i++)
                    Assert.AreEqual(bezierSpline.ControlPoints[i], data.Points[i]);
                bezierSpline.ClearData();
            }
            {
                float2 b = new float2(2.5f, 0f);
                bezierSpline.AddControlPoint(b);

                data = bezierSpline.ConvertJobData();
                Assert.AreEqual(bezierSpline.Length(), data.Length);
                Assert.AreEqual(bezierSpline.Times.Count, data.Time.Length);
                for (int i = 0; i < bezierSpline.Times.Count; i++)
                    Assert.AreEqual(bezierSpline.Times[i], data.Time[i]);
                Assert.AreEqual(bezierSpline.ControlPoints.Count, data.Points.Length);
                for (int i = 0; i < bezierSpline.ControlPoints.Count; i++)
                    Assert.AreEqual(bezierSpline.ControlPoints[i], data.Points[i]);
                bezierSpline.ClearData();
            }
            {
                float2 c = new float2(7.5f, 0f);
                bezierSpline.AddControlPoint(c);

                data = bezierSpline.ConvertJobData();
                Assert.AreEqual(bezierSpline.Length(), data.Length);
                Assert.AreEqual(bezierSpline.Times.Count, data.Time.Length);
                for (int i = 0; i < bezierSpline.Times.Count; i++)
                    Assert.AreEqual(bezierSpline.Times[i], data.Time[i]);
                Assert.AreEqual(bezierSpline.ControlPoints.Count, data.Points.Length);
                for (int i = 0; i < bezierSpline.ControlPoints.Count; i++)
                    Assert.AreEqual(bezierSpline.ControlPoints[i], data.Points[i]);
                bezierSpline.ClearData();
            }
            {
                float2 d = new float2(10f, 0f);
                bezierSpline.AddControlPoint(d);

                data = bezierSpline.ConvertJobData();
                Assert.AreEqual(bezierSpline.Length(), data.Length);
                Assert.AreEqual(bezierSpline.Times.Count, data.Time.Length);
                for (int i = 0; i < bezierSpline.Times.Count; i++)
                    Assert.AreEqual(bezierSpline.Times[i], data.Time[i]);
                Assert.AreEqual(bezierSpline.ControlPoints.Count, data.Points.Length);
                for (int i = 0; i < bezierSpline.ControlPoints.Count; i++)
                    Assert.AreEqual(bezierSpline.ControlPoints[i], data.Points[i]);
                bezierSpline.ClearData();
            }
        }

        private void CheckFloatJob(float2 expected, BezierSpline2DPointJob job, float progress)
        {
            job.SplineProgress = new SplineProgress() { Progress = progress };
            job.Execute();

            CheckFloat2(expected, job.Result);
        }

        private void CheckFloat2(float2 expected, float2 reality, float tolerance = 0.00001f)
        {
            Assert.IsTrue(math.length(math.abs(expected.x - reality.x)) <= tolerance,
                $"X axis is out of range!\n Expected: {expected.x}\n Received: {reality.x}\n Tolerance: {tolerance:N3}");
            Assert.IsTrue(math.length(math.abs(expected.y - reality.y)) <= tolerance,
                $"Y axis is out of range!\n Expected: {expected.x}\n Received: {reality.x}\n Tolerance: {tolerance:N3}");
        }
        
        public class BezierSpline2DSimpleInspector : BezierSpline2DSimple
        {
            public IReadOnlyList<float2> ControlPoints => base.Points;
            public IReadOnlyList<float> Times => base.SegmentLength;
            public IReadOnlyList<SplineEditMode> Modes => base.PointEdit;

            public Spline2DData ConvertJobData()
            {
                return ConvertData();
            }
        }
    }
}