using System.Collections.Generic;
using Crener.Spline.BezierSpline;
using Crener.Spline.BezierSpline.Jobs;
using Crener.Spline.Common;
using Crener.Spline.Common.DataStructs;
using Crener.Spline.Test._2D.Bezier.TestAdapters;
using Crener.Spline.Test.Helpers;
using NUnit.Framework;
using Unity.Mathematics;

namespace Crener.Spline.Test._2D
{
    public class BezierSpline2DJobTest : SharedBezierSplineTestBase
    {
        [Test]
        public void Point()
        {
            ISimpleTestSpline bezierSpline = CreateSpline();

            float2 a = float2.zero;
            bezierSpline.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            bezierSpline.AddControlPoint(b);

            Spline2DData data = bezierSpline.SplineEntityData.Value;
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
            ISimpleTestSpline bezierSpline = CreateSpline();

            float2 a = float2.zero;
            bezierSpline.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            bezierSpline.AddControlPoint(b);
            float2 c = new float2(2f, 0f);
            bezierSpline.AddControlPoint(c);

            Spline2DData data = bezierSpline.SplineEntityData.Value;
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
            ISimpleTestSpline bezierSpline = CreateSpline();

            float2 a = float2.zero;
            bezierSpline.AddControlPoint(a);
            float2 b = new float2(2.5f, 0f);
            bezierSpline.AddControlPoint(b);
            float2 c = new float2(7.5f, 0f);
            bezierSpline.AddControlPoint(c);
            float2 d = new float2(10f, 0f);
            bezierSpline.AddControlPoint(d);

            Spline2DData data = bezierSpline.SplineEntityData.Value;
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
            ISimpleTestSpline bezierSpline = CreateSpline();

            Spline2DData data = bezierSpline.SplineEntityData.Value;
            Assert.AreEqual(bezierSpline.Length(), data.Length);
            Assert.AreEqual(bezierSpline.Times.Count, data.Time.Length);
            Assert.AreEqual(bezierSpline.ControlPoints.Count, data.Points.Length);
            bezierSpline.ClearData();

            {
                float2 a = float2.zero;
                bezierSpline.AddControlPoint(a);

                data = bezierSpline.SplineEntityData.Value;
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

                data = bezierSpline.SplineEntityData.Value;
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

                data = bezierSpline.SplineEntityData.Value;
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

                data = bezierSpline.SplineEntityData.Value;
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

            TestHelpers.CheckFloat2(expected, job.Result);
        }
    }
}