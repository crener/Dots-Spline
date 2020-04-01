using System.Collections.Generic;
using Code.Spline2.BezierSpline;
using Code.Spline2.BezierSpline.Entity;
using Code.Spline2.BezierSpline.Jobs;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

namespace Code.Spline2.Test.Simple
{
    public class Spline2DJobTest
    {
        [Test]
        public void Point()
        {
            Spline2DSimpleInspector spline = CreateSpline();

            float2 a = float2.zero;
            spline.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            spline.AddControlPoint(b);

            Spline2DData data = spline.ConvertJobData();
            Assert.AreEqual(1f, spline.Length());
            Assert.AreEqual(1f, data.Length);

            Assert.AreEqual(1, spline.Times.Count);
            Assert.AreEqual(1, data.Time.Length);
            Assert.AreEqual(1f, spline.Times[0]);
            Assert.AreEqual(1f, data.Time[0]);
            
            Spline2DPointJob job = new Spline2DPointJob()
            {
                Spline = data,
                SplineProgress = new SplineProgress() { Progress = -0.5f }
            };

            CheckFloatJob(a, job, -0.5f);
            CheckFloatJob(a, job, 0f);
            CheckFloatJob(new float2(0.5f, 0f), job, 0.5f);
            CheckFloatJob(b, job, 5f);

            data.Dispose();
        }

        [Test]
        public void Point2()
        {
            Spline2DSimpleInspector spline = CreateSpline();

            float2 a = float2.zero;
            spline.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            spline.AddControlPoint(b);
            float2 c = new float2(2f, 0f);
            spline.AddControlPoint(c);

            Spline2DData data = spline.ConvertJobData();
            Assert.AreEqual(2f, spline.Length());
            Assert.AreEqual(2f, data.Length);

            Assert.AreEqual(2, spline.Times.Count);
            Assert.AreEqual(2, data.Time.Length);
            Assert.AreEqual(0.5f, spline.Times[0]);
            Assert.AreEqual(0.5f, data.Time[0]);
            Assert.AreEqual(1f, spline.Times[1]);
            Assert.AreEqual(1f, data.Time[1]);

            Spline2DPointJob job = new Spline2DPointJob()
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
            
            data.Dispose();
        }

        [Test]
        public void Point3()
        {
            Spline2DSimpleInspector spline = CreateSpline();

            float2 a = float2.zero;
            spline.AddControlPoint(a);
            float2 b = new float2(2.5f, 0f);
            spline.AddControlPoint(b);
            float2 c = new float2(7.5f, 0f);
            spline.AddControlPoint(c);
            float2 d = new float2(10f, 0f);
            spline.AddControlPoint(d);

            Spline2DData data = spline.ConvertJobData();
            Assert.AreEqual(10f, spline.Length());
            Assert.AreEqual(10f, data.Length);

            Assert.AreEqual(3, spline.Times.Count);
            Assert.AreEqual(3, data.Time.Length);
            Assert.AreEqual(0.25f, spline.Times[0]);
            Assert.AreEqual(0.25f, data.Time[0]);
            Assert.AreEqual(0.75f, spline.Times[1]);
            Assert.AreEqual(0.75f, data.Time[1]);
            Assert.AreEqual(1f, spline.Times[2]);

            Spline2DPointJob job = new Spline2DPointJob()
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

            data.Dispose();
        }

        /// <summary>
        /// Tests data generated for 
        /// </summary>
        [Test]
        public void DataEquality()
        {
            Spline2DSimpleInspector spline = CreateSpline();

            Spline2DData data = spline.ConvertJobData();
            Assert.AreEqual(spline.Length(), data.Length);
            Assert.AreEqual(spline.Times.Count, data.Time.Length);
            Assert.AreEqual(spline.ControlPoints.Count, data.Points.Length);
            spline.ClearData();

            {
                float2 a = float2.zero;
                spline.AddControlPoint(a);

                data = spline.ConvertJobData();
                Assert.AreEqual(spline.Length(), data.Length);
                Assert.AreEqual(spline.Times.Count, data.Time.Length);
                for (int i = 0; i < spline.Times.Count; i++)
                    Assert.AreEqual(spline.Times[i], data.Time[i]);
                Assert.AreEqual(spline.ControlPoints.Count, data.Points.Length);
                for (int i = 0; i < spline.ControlPoints.Count; i++)
                    Assert.AreEqual(spline.ControlPoints[i], data.Points[i]);
                spline.ClearData();
            }
            {
                float2 b = new float2(2.5f, 0f);
                spline.AddControlPoint(b);

                data = spline.ConvertJobData();
                Assert.AreEqual(spline.Length(), data.Length);
                Assert.AreEqual(spline.Times.Count, data.Time.Length);
                for (int i = 0; i < spline.Times.Count; i++)
                    Assert.AreEqual(spline.Times[i], data.Time[i]);
                Assert.AreEqual(spline.ControlPoints.Count, data.Points.Length);
                for (int i = 0; i < spline.ControlPoints.Count; i++)
                    Assert.AreEqual(spline.ControlPoints[i], data.Points[i]);
                spline.ClearData();
            }
            {
                float2 c = new float2(7.5f, 0f);
                spline.AddControlPoint(c);

                data = spline.ConvertJobData();
                Assert.AreEqual(spline.Length(), data.Length);
                Assert.AreEqual(spline.Times.Count, data.Time.Length);
                for (int i = 0; i < spline.Times.Count; i++)
                    Assert.AreEqual(spline.Times[i], data.Time[i]);
                Assert.AreEqual(spline.ControlPoints.Count, data.Points.Length);
                for (int i = 0; i < spline.ControlPoints.Count; i++)
                    Assert.AreEqual(spline.ControlPoints[i], data.Points[i]);
                spline.ClearData();
            }
            {
                float2 d = new float2(10f, 0f);
                spline.AddControlPoint(d);

                data = spline.ConvertJobData();
                Assert.AreEqual(spline.Length(), data.Length);
                Assert.AreEqual(spline.Times.Count, data.Time.Length);
                for (int i = 0; i < spline.Times.Count; i++)
                    Assert.AreEqual(spline.Times[i], data.Time[i]);
                Assert.AreEqual(spline.ControlPoints.Count, data.Points.Length);
                for (int i = 0; i < spline.ControlPoints.Count; i++)
                    Assert.AreEqual(spline.ControlPoints[i], data.Points[i]);
                spline.ClearData();
            }
        }

        private void CheckFloatJob(float2 expected, Spline2DPointJob job, float progress)
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

        private Spline2DSimpleInspector CreateSpline()
        {
            GameObject game = new GameObject();
            Spline2DSimpleInspector spline = game.AddComponent<Spline2DSimpleInspector>();
            Assert.IsNotNull(spline);

            ClearSpline(spline);

            return spline;
        }

        private void ClearSpline(Spline2DSimple spline)
        {
            while (spline.ControlPointCount > 0)
            {
                spline.RemoveControlPoint(0);
            }

            Assert.AreEqual(0f, spline.Length());
            Assert.AreEqual(0, spline.ControlPointCount);
        }

        public class Spline2DSimpleInspector : Spline2DSimple
        {
            public IReadOnlyList<float2> ControlPoints => base.Points;
            public IReadOnlyList<float> Times => base.SegmentLength;
            public IReadOnlyList<SplineEditMode> Modes => base.PointEdit;

            public Spline2DData ConvertJobData()
            {
                return this.ConvertData();
            }
        }
    }
}