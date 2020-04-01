using System.Collections.Generic;
using Code.Spline2.BezierSpline;
using Code.Spline2.BezierSpline.Entity;
using Code.Spline2.BezierSpline.Jobs;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

namespace Code.Spline2.Test.Simple
{
    /// <summary>
    /// Override for testing <see cref="Spline2DSimple"/>
    /// </summary>
    public class Spline2DSimpleGameobjectTest : BaseSpline2DSimpleTest
    {
        protected override ISimpleTestSpline CreateSpline()
        {
            GameObject game = new GameObject();
            TestSpline2DSimpleTestInspector testSpline = game.AddComponent<TestSpline2DSimpleTestInspector>();
            Assert.IsNotNull(testSpline);

            ClearSpline(testSpline);

            return testSpline;
        }

        public class TestSpline2DSimpleTestInspector : Spline2DSimple, ISimpleTestSpline
        {
            public IReadOnlyList<float2> ControlPoints => Points;
            public IReadOnlyList<float> Times => SegmentLength;
            public IReadOnlyList<SplineEditMode> Modes => PointEdit;
        }
    }

    /// <summary>
    /// Override for testing <see cref="Spline2DPointJob"/>
    /// </summary>
    public class Spline2DSimpleJobTest : BaseSpline2DSimpleTest
    {
        protected override ISimpleTestSpline CreateSpline()
        {
            GameObject game = new GameObject();
            TestSpline2DSimpleJobTestInspector testSpline = game.AddComponent<TestSpline2DSimpleJobTestInspector>();
            Assert.IsNotNull(testSpline);

            ClearSpline(testSpline);

            return testSpline;
        }

        public class TestSpline2DSimpleJobTestInspector : Spline2DSimple, ISimpleTestSpline
        {
            public IReadOnlyList<float2> ControlPoints => Points;
            public IReadOnlyList<float> Times => SegmentLength;
            public IReadOnlyList<SplineEditMode> Modes => PointEdit;

            public new float Length
            {
                get
                {
                    ClearData();
                    ConvertData();

                    Assert.IsTrue(SplineEntityData.HasValue, "Failed to generate spline");
                    return SplineEntityData.Value.Length;
                }
            }

            public new float2 GetPoint(float progress)
            {
                ClearData();
                ConvertData();

                Assert.IsTrue(SplineEntityData.HasValue, "Failed to generate spline");
                Spline2DPointJob job = new Spline2DPointJob()
                {
                    Spline = SplineEntityData.Value,
                    SplineProgress = new SplineProgress() {Progress = progress}
                };
                job.Execute();

                return job.Result;
            }
        }
    }

    public abstract class BaseSpline2DSimpleTest
    {
        protected abstract ISimpleTestSpline CreateSpline();

        [Test]
        public void Basic()
        {
            ISimpleTestSpline spline = CreateSpline();
            Assert.NotNull(spline);
        }

        [Test]
        public void Add1()
        {
            ISimpleTestSpline testSpline = CreateSpline();

            float2 a = float2.zero;
            testSpline.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            testSpline.AddControlPoint(b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(1f, testSpline.Length());

            CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            CheckFloat2(b, testSpline.GetControlPoint(1, SplinePoint.Point));
        }

        [Test]
        public void Add2()
        {
            ISimpleTestSpline testSpline = CreateSpline();

            float2 a = float2.zero;
            testSpline.AddControlPoint(float2.zero);
            float2 b = new float2(1f, 0f);
            testSpline.AddControlPoint(b);
            float2 c = new float2(2f, 0f);
            testSpline.AddControlPoint(c);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(3, testSpline.Modes.Count);
            Assert.AreEqual(2, testSpline.Times.Count);
            Assert.AreEqual(2f, testSpline.Length());

            CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            CheckFloat2(b, testSpline.GetControlPoint(1, SplinePoint.Point));
            CheckFloat2(c, testSpline.GetControlPoint(2, SplinePoint.Point));
        }

        [Test]
        public void Remove()
        {
            ISimpleTestSpline testSpline = CreateSpline();

            float2 a = float2.zero;
            testSpline.AddControlPoint(float2.zero);
            float2 b = new float2(1f, 0f);
            testSpline.AddControlPoint(b);
            float2 c = new float2(2f, 0f);
            testSpline.AddControlPoint(c);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(3, testSpline.Modes.Count);
            Assert.AreEqual(7, testSpline.ControlPoints.Count);
            Assert.AreEqual(2, testSpline.Times.Count);
            Assert.AreEqual(2f, testSpline.Length());

            CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            CheckFloat2(b, testSpline.GetControlPoint(1, SplinePoint.Point));
            CheckFloat2(c, testSpline.GetControlPoint(2, SplinePoint.Point));

            //Remove a point
            testSpline.RemoveControlPoint(1);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(4, testSpline.ControlPoints.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(2f, testSpline.Length());

            CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            CheckFloat2(c, testSpline.GetControlPoint(1, SplinePoint.Point));
        }

        [Test]
        public void RemoveFromStart()
        {
            ISimpleTestSpline testSpline = CreateSpline();

            float2 a = float2.zero;
            testSpline.AddControlPoint(float2.zero);
            float2 b = new float2(1f, 0f);
            testSpline.AddControlPoint(b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(4, testSpline.ControlPoints.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(1f, testSpline.Length());

            CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            CheckFloat2(b, testSpline.GetControlPoint(1, SplinePoint.Point));

            //Remove a point
            testSpline.RemoveControlPoint(0);

            Assert.AreEqual(1, testSpline.ControlPointCount);
            Assert.AreEqual(1, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.ControlPoints.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(0f, testSpline.Length());

            CheckFloat2(b, testSpline.GetControlPoint(0, SplinePoint.Point));
        }

        [Test]
        public void RemoveFromEnd()
        {
            ISimpleTestSpline testSpline = CreateSpline();

            float2 a = float2.zero;
            testSpline.AddControlPoint(float2.zero);
            float2 b = new float2(1f, 0f);
            testSpline.AddControlPoint(b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(4, testSpline.ControlPoints.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(1f, testSpline.Length());

            CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            CheckFloat2(b, testSpline.GetControlPoint(1, SplinePoint.Point));

            //Remove a point
            testSpline.RemoveControlPoint(1);

            Assert.AreEqual(1, testSpline.ControlPointCount);
            Assert.AreEqual(1, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.ControlPoints.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(0f, testSpline.Length());

            CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
        }

        [Test]
        public void Point()
        {
            ISimpleTestSpline testSpline = CreateSpline();

            float2 a = float2.zero;
            testSpline.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            testSpline.AddControlPoint(b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(1f, testSpline.Length());

            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(1f, testSpline.Times[0]);

            CheckFloat2(new float2(0.5f, 0f), testSpline.GetPoint(0.5f));
        }

        [Test]
        public void Point2()
        {
            ISimpleTestSpline testSpline = CreateSpline();

            float2 a = float2.zero;
            testSpline.AddControlPoint(float2.zero);
            float2 b = new float2(1f, 0f);
            testSpline.AddControlPoint(b);
            float2 c = new float2(2f, 0f);
            testSpline.AddControlPoint(c);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(2f, testSpline.Length());

            Assert.AreEqual(2, testSpline.Times.Count);
            Assert.AreEqual(0.5f, testSpline.Times[0]);
            Assert.AreEqual(1f, testSpline.Times[1]);

            CheckFloat2(a, testSpline.GetPoint(0f));
            CheckFloat2(new float2(1f, 0f), testSpline.GetPoint(0.5f));
            //CheckFloat2(c * 0.77f, spline.GetPoint(0.77f)); // fails due to bezier point bunching issues
        }

        [Test]
        public void Point3()
        {
            ISimpleTestSpline testSpline = CreateSpline();

            float2 a = float2.zero;
            testSpline.AddControlPoint(float2.zero);
            float2 b = new float2(2.5f, 0f);
            testSpline.AddControlPoint(b);
            float2 c = new float2(7.5f, 0f);
            testSpline.AddControlPoint(c);
            float2 d = new float2(10f, 0f);
            testSpline.AddControlPoint(d);

            Assert.AreEqual(4, testSpline.ControlPointCount);
            Assert.AreEqual(4, testSpline.Modes.Count);
            Assert.AreEqual(10f, testSpline.Length());

            Assert.AreEqual(3, testSpline.Times.Count);
            Assert.AreEqual(0.25f, testSpline.Times[0]);
            Assert.AreEqual(0.75f, testSpline.Times[1]);
            Assert.AreEqual(1f, testSpline.Times[2]);

            CheckFloat2(a, testSpline.GetPoint(0f));
            CheckFloat2(new float2(2.5f, 0f), testSpline.GetPoint(0.25f));
            CheckFloat2(new float2(5f, 0f), testSpline.GetPoint(0.5f));
            //CheckFloat2(new float2(9.9f, 0f), spline.GetPoint(0.99f)); // fails due to bezier point bunching issues
            CheckFloat2(new float2(10f, 0f), testSpline.GetPoint(1f));
            CheckFloat2(new float2(10f, 0f), testSpline.GetPoint(5f));
        }

        [Test]
        public void Point4()
        {
            ISimpleTestSpline testSpline = CreateSpline();

            float2 a = new float2(3f, 3f);
            testSpline.AddControlPoint(a);

            Assert.AreEqual(1, testSpline.ControlPointCount);
            Assert.AreEqual(0f, testSpline.Length());

            CheckFloat2(new float2(3f, 3f), testSpline.GetPoint(0.5f));
        }

        [Test]
        public void Point5()
        {
            ISimpleTestSpline testSpline = CreateSpline();

            float2 a = new float2(1f, 10f);
            testSpline.AddControlPoint(a);
            float2 b = new float2(2f, 10f);
            testSpline.AddControlPoint(b);
            float2 c = new float2(3f, 10f);
            testSpline.AddControlPoint(c);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(2f, testSpline.Length());

            CheckFloat2(new float2(2.5f, 10f), testSpline.GetPoint(0.7f), 0.01f);
        }

        [Test]
        public void Update()
        {
            ISimpleTestSpline testSpline = CreateSpline();

            float2 a = float2.zero;
            testSpline.AddControlPoint(float2.zero);
            float2 b = new float2(1f, 0f);
            testSpline.AddControlPoint(b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            CheckFloat2(a, testSpline.GetPoint(0f));
            CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            CheckFloat2(b, testSpline.GetPoint(1f));
            CheckFloat2(b, testSpline.GetControlPoint(1, SplinePoint.Point));

            //update 0 point position
            float2 a2 = new float2(-1f, -1f);
            testSpline.UpdateControlPoint(0, a2, SplinePoint.Point);

            CheckFloat2(a2, testSpline.GetPoint(0f));
            CheckFloat2(a2, testSpline.GetControlPoint(0, SplinePoint.Point));

            //update 1 point position
            float2 b2 = new float2(2f, 2f);
            testSpline.UpdateControlPoint(1, b2, SplinePoint.Point);

            CheckFloat2(b2, testSpline.GetPoint(1f));
            CheckFloat2(b2, testSpline.GetControlPoint(1, SplinePoint.Point));
        }

        [Test]
        public void Update2()
        {
            ISimpleTestSpline testSpline = CreateSpline();

            float2 a = float2.zero;
            testSpline.AddControlPoint(float2.zero);
            float2 b = new float2(1f, 0f);
            testSpline.AddControlPoint(b);
            float2 c = new float2(2f, 0f);
            testSpline.AddControlPoint(c);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(3, testSpline.Modes.Count);
            Assert.AreEqual(2, testSpline.Times.Count);
            CheckFloat2(a, testSpline.GetPoint(0f));
            CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            CheckFloat2(b, testSpline.GetPoint(0.5f));
            CheckFloat2(b, testSpline.GetControlPoint(1, SplinePoint.Point));
            CheckFloat2(c, testSpline.GetPoint(1f));
            CheckFloat2(c, testSpline.GetControlPoint(2, SplinePoint.Point));

            //update 0 point position
            float2 a2 = new float2(0f, 1f);
            testSpline.UpdateControlPoint(0, a2, SplinePoint.Point);
            CheckFloat2(a2, testSpline.GetPoint(0f));
            CheckFloat2(a2, testSpline.GetControlPoint(0, SplinePoint.Point));

            //update 1 point position
            float2 b2 = new float2(1f, 1f);
            testSpline.UpdateControlPoint(1, b2, SplinePoint.Point);
            CheckFloat2(b2, testSpline.GetControlPoint(1, SplinePoint.Point));

            //update 2 point position
            float2 c2 = new float2(2f, 1f);
            testSpline.UpdateControlPoint(2, c2, SplinePoint.Point);
            CheckFloat2(c2, testSpline.GetPoint(1f));
            CheckFloat2(c2, testSpline.GetControlPoint(2, SplinePoint.Point));
        }

        [Test]
        public void Update3()
        {
            ISimpleTestSpline testSpline = CreateSpline();

            float2 a = float2.zero;
            testSpline.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            testSpline.AddControlPoint(b);
            float2 c = new float2(2f, 0f);
            testSpline.AddControlPoint(c);

            Assert.AreEqual(2f, testSpline.Length());

            //update 1 point position
            float2 b2 = new float2(1f, 2f);
            testSpline.UpdateControlPoint(1, b2, SplinePoint.Point);
            Assert.GreaterOrEqual(testSpline.Length(), 2f);
        }

        [Test]
        public void Insert()
        {
            ISimpleTestSpline testSpline = CreateSpline();

            float2 a = float2.zero;
            testSpline.AddControlPoint(float2.zero);
            float2 b = new float2(10f, 0f);
            testSpline.AddControlPoint(b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            CheckFloat2(a, testSpline.GetPoint(0f));
            CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            CheckFloat2(b, testSpline.GetPoint(1f));
            CheckFloat2(b, testSpline.GetControlPoint(1, SplinePoint.Point));

            //insert point
            float2 c = new float2(20f, 0f);
            testSpline.InsertControlPoint(1, c);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(3, testSpline.Modes.Count);
            Assert.AreEqual(2, testSpline.Times.Count);
            CheckFloat2(a, testSpline.ControlPoints[0]);
            CheckFloat2(c, testSpline.ControlPoints[3]);
            CheckFloat2(b, testSpline.ControlPoints[6]);
        }

        [Test]
        public void InsertAtStart()
        {
            ISimpleTestSpline testSpline = CreateSpline();

            float2 a = float2.zero;
            testSpline.AddControlPoint(float2.zero);
            float2 b = new float2(2f, 0f);
            testSpline.AddControlPoint(b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.Times.Count);

            CheckFloat2(a, testSpline.GetPoint(0f));
            CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            CheckFloat2(b, testSpline.GetPoint(1f));
            CheckFloat2(b, testSpline.GetControlPoint(1, SplinePoint.Point));

            //insert point
            float2 c = new float2(-2f, 0f);
            testSpline.InsertControlPoint(0, c);

            Assert.AreEqual(3, testSpline.ControlPointCount);
            Assert.AreEqual(3, testSpline.Modes.Count);
            Assert.AreEqual(2, testSpline.Times.Count);
            CheckFloat2(c, testSpline.ControlPoints[0]);
            CheckFloat2(a, testSpline.ControlPoints[3]);
            CheckFloat2(b, testSpline.ControlPoints[6]);
            CheckFloat2(c, testSpline.GetControlPoint(0, SplinePoint.Point));
            CheckFloat2(a, testSpline.GetControlPoint(1, SplinePoint.Point));
            CheckFloat2(b, testSpline.GetControlPoint(2, SplinePoint.Point));
        }

        [Test]
        public void PointCreation()
        {
            ISimpleTestSpline testSpline = CreateSpline();

            float2 a = new float2(0f, 0f);
            testSpline.AddControlPoint(float2.zero);

            Assert.AreEqual(1, testSpline.ControlPointCount);
            Assert.AreEqual(1, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.Times.Count);

            Assert.AreEqual(1, testSpline.ControlPoints.Count);
            CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));

            float2 b = new float2(10f, 0f);
            testSpline.AddControlPoint(b);

            Assert.AreEqual(2, testSpline.ControlPointCount);
            Assert.AreEqual(2, testSpline.Modes.Count);
            Assert.AreEqual(1, testSpline.Times.Count);
            Assert.AreEqual(10f, testSpline.Length());

            Assert.AreEqual(4, testSpline.ControlPoints.Count);
            CheckFloat2(a, testSpline.GetControlPoint(0, SplinePoint.Point));
            CheckFloat2(new float2(1f, 0f), testSpline.GetControlPoint(0, SplinePoint.Post));
            CheckFloat2(new float2(9f, 0f), testSpline.GetControlPoint(1, SplinePoint.Pre));
            CheckFloat2(b, testSpline.GetControlPoint(1, SplinePoint.Point));
        }

        private void CheckFloat2(float2 expected, float2 reality, float tolerance = 0.00001f)
        {
            Assert.IsTrue(math.length(math.abs(expected.x - reality.x)) <= tolerance,
                $"X axis is out of range!\n Expected: {expected.x}\n Received: {reality.x}\n Tolerance: {tolerance:N3}");
            Assert.IsTrue(math.length(math.abs(expected.y - reality.y)) <= tolerance,
                $"Y axis is out of range!\n Expected: {expected.x}\n Received: {reality.x}\n Tolerance: {tolerance:N3}");
        }

        protected void ClearSpline(Spline2DSimple spline)
        {
            while (spline.ControlPointCount > 0)
            {
                spline.RemoveControlPoint(0);
            }

            Assert.AreEqual(0f, spline.Length());
            Assert.AreEqual(0, spline.ControlPointCount);
        }
    }
}