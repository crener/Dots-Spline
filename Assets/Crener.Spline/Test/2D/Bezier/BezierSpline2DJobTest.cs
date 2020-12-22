using Crener.Spline._2D.Jobs;
using Crener.Spline.Common;
using Crener.Spline.Common.DataStructs;
using Crener.Spline.Test._2D.Bezier.TestAdapters;
using Crener.Spline.Test.Helpers;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace Crener.Spline.Test._2D.Bezier
{
    public class BezierSpline2DJobTest : SharedBezierSplineTestBase
    {
        [Test]
        public void Point()
        {
            ISimpleTestSpline2D bezierSpline2D = CreateSpline();

            float2 a = float2.zero;
            bezierSpline2D.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            bezierSpline2D.AddControlPoint(b);

            Spline2DData data = bezierSpline2D.SplineEntityData2D.Value;
            Assert.AreEqual(1f, bezierSpline2D.Length());
            Assert.AreEqual(1f, data.Length);

            Assert.AreEqual(1, bezierSpline2D.Times.Count);
            Assert.AreEqual(1, data.Time.Length);
            Assert.AreEqual(1f, bezierSpline2D.Times[0]);
            Assert.AreEqual(1f, data.Time[0]);
            
            BezierSpline2DPointJob job = new BezierSpline2DPointJob(bezierSpline2D, -0.5f, Allocator.Temp);

            try
            {
                CheckFloatJob(a, job, -0.5f);
                CheckFloatJob(a, job, 0f);
                CheckFloatJob(new float2(0.5f, 0f), job, 0.5f);
                CheckFloatJob(b, job, 5f);
            }
            finally
            {
                job.Dispose();
            }
        }

        [Test]
        public void Point2()
        {
            ISimpleTestSpline2D bezierSpline2D = CreateSpline();

            float2 a = float2.zero;
            bezierSpline2D.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            bezierSpline2D.AddControlPoint(b);
            float2 c = new float2(2f, 0f);
            bezierSpline2D.AddControlPoint(c);

            Spline2DData data = bezierSpline2D.SplineEntityData2D.Value;
            Assert.AreEqual(2f, bezierSpline2D.Length());
            Assert.AreEqual(2f, data.Length);

            Assert.AreEqual(2, bezierSpline2D.Times.Count);
            Assert.AreEqual(2, data.Time.Length);
            Assert.AreEqual(0.5f, bezierSpline2D.Times[0]);
            Assert.AreEqual(0.5f, data.Time[0]);
            Assert.AreEqual(1f, bezierSpline2D.Times[1]);
            Assert.AreEqual(1f, data.Time[1]);

            BezierSpline2DPointJob job = new BezierSpline2DPointJob(bezierSpline2D, -0.5f, Allocator.Temp);

            try
            {
                CheckFloatJob(a, job, -0.5f);
                CheckFloatJob(a, job, 0f);
                CheckFloatJob(new float2(0.5f, 0f), job, 0.25f);
                CheckFloatJob(new float2(1f, 0f), job, 0.5f);
                //CheckFloatJob(new float2(1.2f, 0f), job, 0.6f);
                CheckFloatJob(new float2(2f, 0f), job, 1f);
                CheckFloatJob(new float2(2f, 0f), job, 5f);
            }
            finally
            {
                job.Dispose();
            }
        }

        [Test]
        public void Point3()
        {
            ISimpleTestSpline2D bezierSpline2D = CreateSpline();

            float2 a = float2.zero;
            bezierSpline2D.AddControlPoint(a);
            float2 b = new float2(2.5f, 0f);
            bezierSpline2D.AddControlPoint(b);
            float2 c = new float2(7.5f, 0f);
            bezierSpline2D.AddControlPoint(c);
            float2 d = new float2(10f, 0f);
            bezierSpline2D.AddControlPoint(d);

            Spline2DData data = bezierSpline2D.SplineEntityData2D.Value;
            Assert.AreEqual(10f, bezierSpline2D.Length());
            Assert.AreEqual(10f, data.Length);

            Assert.AreEqual(3, bezierSpline2D.Times.Count);
            Assert.AreEqual(3, data.Time.Length);
            Assert.AreEqual(0.25f, bezierSpline2D.Times[0]);
            Assert.AreEqual(0.25f, data.Time[0]);
            Assert.AreEqual(0.75f, bezierSpline2D.Times[1]);
            Assert.AreEqual(0.75f, data.Time[1]);
            Assert.AreEqual(1f, bezierSpline2D.Times[2]);

            BezierSpline2DPointJob job = new BezierSpline2DPointJob(bezierSpline2D, -0.5f, Allocator.Temp);

            try
            {
                CheckFloatJob(a, job, -0.5f);
                CheckFloatJob(a, job, 0f);
                CheckFloatJob(new float2(2.5f, 0f), job, 0.25f);
                CheckFloatJob(new float2(5f, 0f), job, 0.5f);
                CheckFloatJob(new float2(10f, 0f), job, 1f);
                CheckFloatJob(new float2(10f, 0f), job, 5f);
            }
            finally
            {
                job.Dispose();
            }
        }

        /// <summary>
        /// Tests data generated for 
        /// </summary>
        [Test]
        public void DataEquality()
        {
            ISimpleTestSpline2D bezierSpline2D = CreateSpline();

            Spline2DData data = bezierSpline2D.SplineEntityData2D.Value;
            Assert.AreEqual(bezierSpline2D.Length(), data.Length);
            Assert.AreEqual(bezierSpline2D.Times.Count, data.Time.Length);
            Assert.AreEqual(bezierSpline2D.ControlPoints.Count, data.Points.Length);
            bezierSpline2D.ClearData();

            {
                float2 a = float2.zero;
                bezierSpline2D.AddControlPoint(a);

                data = bezierSpline2D.SplineEntityData2D.Value;
                Assert.AreEqual(bezierSpline2D.Length(), data.Length);
                Assert.AreEqual(bezierSpline2D.Times.Count, data.Time.Length);
                for (int i = 0; i < bezierSpline2D.Times.Count; i++)
                    Assert.AreEqual(bezierSpline2D.Times[i], data.Time[i]);
                Assert.AreEqual(bezierSpline2D.ControlPoints.Count, data.Points.Length);
                for (int i = 0; i < bezierSpline2D.ControlPoints.Count; i++)
                    Assert.AreEqual(bezierSpline2D.ControlPoints[i], data.Points[i]);
                bezierSpline2D.ClearData();
            }
            {
                float2 b = new float2(2.5f, 0f);
                bezierSpline2D.AddControlPoint(b);

                data = bezierSpline2D.SplineEntityData2D.Value;
                Assert.AreEqual(bezierSpline2D.Length(), data.Length);
                Assert.AreEqual(bezierSpline2D.Times.Count, data.Time.Length);
                for (int i = 0; i < bezierSpline2D.Times.Count; i++)
                    Assert.AreEqual(bezierSpline2D.Times[i], data.Time[i]);
                Assert.AreEqual(bezierSpline2D.ControlPoints.Count, data.Points.Length);
                for (int i = 0; i < bezierSpline2D.ControlPoints.Count; i++)
                    Assert.AreEqual(bezierSpline2D.ControlPoints[i], data.Points[i]);
                bezierSpline2D.ClearData();
            }
            {
                float2 c = new float2(7.5f, 0f);
                bezierSpline2D.AddControlPoint(c);

                data = bezierSpline2D.SplineEntityData2D.Value;
                Assert.AreEqual(bezierSpline2D.Length(), data.Length);
                Assert.AreEqual(bezierSpline2D.Times.Count, data.Time.Length);
                for (int i = 0; i < bezierSpline2D.Times.Count; i++)
                    Assert.AreEqual(bezierSpline2D.Times[i], data.Time[i]);
                Assert.AreEqual(bezierSpline2D.ControlPoints.Count, data.Points.Length);
                for (int i = 0; i < bezierSpline2D.ControlPoints.Count; i++)
                    Assert.AreEqual(bezierSpline2D.ControlPoints[i], data.Points[i]);
                bezierSpline2D.ClearData();
            }
            {
                float2 d = new float2(10f, 0f);
                bezierSpline2D.AddControlPoint(d);

                data = bezierSpline2D.SplineEntityData2D.Value;
                Assert.AreEqual(bezierSpline2D.Length(), data.Length);
                Assert.AreEqual(bezierSpline2D.Times.Count, data.Time.Length);
                for (int i = 0; i < bezierSpline2D.Times.Count; i++)
                    Assert.AreEqual(bezierSpline2D.Times[i], data.Time[i]);
                Assert.AreEqual(bezierSpline2D.ControlPoints.Count, data.Points.Length);
                for (int i = 0; i < bezierSpline2D.ControlPoints.Count; i++)
                    Assert.AreEqual(bezierSpline2D.ControlPoints[i], data.Points[i]);
                bezierSpline2D.ClearData();
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