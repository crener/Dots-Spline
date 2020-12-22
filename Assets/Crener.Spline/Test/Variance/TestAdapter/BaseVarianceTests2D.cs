using System;
using Crener.Spline.Common;
using Crener.Spline.Test._2D;
using Crener.Spline.Test.Helpers;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

namespace Crener.Spline.Test.Variance.TestAdapter
{
    public abstract class BaseVarianceTests2D : BaseSimpleSplineTests2D
    {
        [Test]
        public void VarianceAdd1()
        {
            IVarianceTestSpline2D spline2D = CreateVarianceSpline();

            float2 a = float2.zero;
            spline2D.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            spline2D.AddControlPoint(b);

            Assert.AreEqual(2, spline2D.ControlPointCount);
            Assert.AreEqual(2, spline2D.Modes.Count);
            Assert.AreEqual(1, spline2D.Times.Count);
            Assert.AreEqual(1f, spline2D.Length());

            TestHelpers.CheckFloat2(a, spline2D.GetControlPoint(0, SplinePointVariance.Point));
            TestHelpers.CheckFloat2(b, spline2D.GetControlPoint(1, SplinePointVariance.Point));
        }

        [Test]
        public void VarianceAdd2()
        {
            IVarianceTestSpline2D spline2D = CreateVarianceSpline();

            float2 a = float2.zero;
            spline2D.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            spline2D.AddControlPoint(b);
            float2 c = new float2(2f, 0f);
            spline2D.AddControlPoint(c);

            Assert.AreEqual(3, spline2D.ControlPointCount);
            Assert.AreEqual(3, spline2D.Modes.Count);
            Assert.AreEqual(2, spline2D.Times.Count);
            Assert.AreEqual(2f, spline2D.Length());

            TestHelpers.CheckFloat2(a, spline2D.GetControlPoint(0, SplinePointVariance.Point));
            TestHelpers.CheckFloat2(b, spline2D.GetControlPoint(1, SplinePointVariance.Point));
            TestHelpers.CheckFloat2(c, spline2D.GetControlPoint(2, SplinePointVariance.Point));
        }

        [Test]
        public void VarianceRemove()
        {
            IVarianceTestSpline2D spline2D = CreateVarianceSpline();

            float2 a = float2.zero;
            spline2D.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            spline2D.AddControlPoint(b);
            float2 c = new float2(2f, 0f);
            spline2D.AddControlPoint(c);

            Assert.AreEqual(3, spline2D.ControlPointCount);
            Assert.AreEqual(3, spline2D.Modes.Count);
            Assert.AreEqual(21, spline2D.ControlPoints.Count);
            Assert.AreEqual(2, spline2D.Times.Count);
            Assert.AreEqual(2f, spline2D.Length());

            TestHelpers.CheckFloat2(a, spline2D.GetControlPoint(0, SplinePointVariance.Point));
            TestHelpers.CheckFloat2(b, spline2D.GetControlPoint(1, SplinePointVariance.Point));
            TestHelpers.CheckFloat2(c, spline2D.GetControlPoint(2, SplinePointVariance.Point));

            //Remove a point
            spline2D.RemoveControlPoint(1);

            Assert.AreEqual(2, spline2D.ControlPointCount);
            Assert.AreEqual(2, spline2D.Modes.Count);
            Assert.AreEqual(12, spline2D.ControlPoints.Count);
            Assert.AreEqual(1, spline2D.Times.Count);
            Assert.AreEqual(2f, spline2D.Length());

            TestHelpers.CheckFloat2(a, spline2D.GetControlPoint(0, SplinePointVariance.Point));
            TestHelpers.CheckFloat2(c, spline2D.GetControlPoint(1, SplinePointVariance.Point));
        }

        [Test]
        public void VarianceRemoveFromStart()
        {
            IVarianceTestSpline2D spline2D = CreateVarianceSpline();

            float2 a = float2.zero;
            spline2D.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            spline2D.AddControlPoint(b);

            Assert.AreEqual(2, spline2D.ControlPointCount);
            Assert.AreEqual(2, spline2D.Modes.Count);
            Assert.AreEqual(12, spline2D.ControlPoints.Count);
            Assert.AreEqual(1, spline2D.Times.Count);
            Assert.AreEqual(1f, spline2D.Length());

            TestHelpers.CheckFloat2(a, spline2D.GetControlPoint(0, SplinePointVariance.Point));
            TestHelpers.CheckFloat2(b, spline2D.GetControlPoint(1, SplinePointVariance.Point));

            //Remove a point
            spline2D.RemoveControlPoint(0);

            Assert.AreEqual(1, spline2D.ControlPointCount);
            Assert.AreEqual(1, spline2D.Modes.Count);
            Assert.AreEqual(3, spline2D.ControlPoints.Count);
            Assert.AreEqual(1, spline2D.Times.Count);
            Assert.AreEqual(0f, spline2D.Length());

            TestHelpers.CheckFloat2(b, spline2D.GetControlPoint(0, SplinePointVariance.Point));
        }

        [Test]
        public void RemoveFromEnd()
        {
            IVarianceTestSpline2D spline2D = CreateVarianceSpline();

            float2 a = float2.zero;
            spline2D.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            spline2D.AddControlPoint(b);
            float2 c = new float2(2f, 0f);
            spline2D.AddControlPoint(c);

            Assert.AreEqual(3, spline2D.ControlPointCount);
            Assert.AreEqual(3, spline2D.Modes.Count);
            Assert.AreEqual(21, spline2D.ControlPoints.Count);
            Assert.AreEqual(2, spline2D.Times.Count);
            Assert.AreEqual(2f, spline2D.Length());

            TestHelpers.CheckFloat2(a, spline2D.GetControlPoint(0, SplinePointVariance.Point));
            TestHelpers.CheckFloat2(b, spline2D.GetControlPoint(1, SplinePointVariance.Point));
            TestHelpers.CheckFloat2(c, spline2D.GetControlPoint(2, SplinePointVariance.Point));

            //Remove a point
            spline2D.RemoveControlPoint(2);

            Assert.AreEqual(2, spline2D.ControlPointCount);
            Assert.AreEqual(2, spline2D.Modes.Count);
            Assert.AreEqual(12, spline2D.ControlPoints.Count);
            Assert.AreEqual(1, spline2D.Times.Count);
            Assert.AreEqual(1f, spline2D.Length());

            TestHelpers.CheckFloat2(a, spline2D.GetControlPoint(0, SplinePointVariance.Point));
            TestHelpers.CheckFloat2(b, spline2D.GetControlPoint(1, SplinePointVariance.Point));

            //Remove a point
            spline2D.RemoveControlPoint(1);

            Assert.AreEqual(1, spline2D.ControlPointCount);
            Assert.AreEqual(1, spline2D.Modes.Count);
            Assert.AreEqual(3, spline2D.ControlPoints.Count);
            Assert.AreEqual(1, spline2D.Times.Count);
            Assert.AreEqual(0f, spline2D.Length());

            TestHelpers.CheckFloat2(a, spline2D.GetControlPoint(0, SplinePointVariance.Point));

            //Remove final point
            spline2D.RemoveControlPoint(0);

            Assert.AreEqual(0, spline2D.ControlPointCount);
            Assert.AreEqual(0, spline2D.Modes.Count);
            Assert.AreEqual(0, spline2D.ControlPoints.Count);
            Assert.AreEqual(0f, spline2D.Length());
        }

        [Test]
        public void VariancePoint()
        {
            IVarianceTestSpline2D spline2D = CreateVarianceSpline();

            float2 a = float2.zero;
            spline2D.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            spline2D.AddControlPoint(b);

            Assert.AreEqual(2, spline2D.ControlPointCount);
            Assert.AreEqual(1f, spline2D.Length());

            Assert.AreEqual(1, spline2D.Times.Count);
            Assert.AreEqual(1f, spline2D.Times[0]);

            TestHelpers.CheckFloat2(new float2(0.5f, 0f), spline2D.GetPoint(0.5f, new half(0)));
        }

        [Test]
        public void VariancePoint2()
        {
            IVarianceTestSpline2D spline2D = CreateVarianceSpline();

            float2 a = float2.zero;
            spline2D.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            spline2D.AddControlPoint(b);
            float2 c = new float2(2f, 0f);
            spline2D.AddControlPoint(c);

            Assert.AreEqual(3, spline2D.ControlPointCount);
            Assert.AreEqual(2f, spline2D.Length());

            Assert.AreEqual(2, spline2D.Times.Count);
            Assert.AreEqual(0.5f, spline2D.Times[0]);
            Assert.AreEqual(1f, spline2D.Times[1]);

            TestHelpers.CheckFloat2(a, spline2D.GetPoint(0f, new half(0)));
            TestHelpers.CheckFloat2(new float2(1f, 0f), spline2D.GetPoint(0.5f, new half(0)));
            //TestHelpers.CheckFloat2(c * 0.77f, spline.GetPoint(0.77f)); // fails due to bezier point bunching issues
        }

        [Test]
        public void VariancePoint3()
        {
            IVarianceTestSpline2D spline2D = CreateVarianceSpline();

            float2 a = float2.zero;
            spline2D.AddControlPoint(a);
            float2 b = new float2(2.5f, 0f);
            spline2D.AddControlPoint(b);
            float2 c = new float2(7.5f, 0f);
            spline2D.AddControlPoint(c);
            float2 d = new float2(10f, 0f);
            spline2D.AddControlPoint(d);

            Assert.AreEqual(4, spline2D.ControlPointCount);
            Assert.AreEqual(4, spline2D.Modes.Count);
            Assert.AreEqual(10f, spline2D.Length());

            Assert.AreEqual(3, spline2D.Times.Count);
            Assert.AreEqual(0.25f, spline2D.Times[0]);
            Assert.AreEqual(0.75f, spline2D.Times[1]);
            Assert.AreEqual(1f, spline2D.Times[2]);

            TestHelpers.CheckFloat2(a, spline2D.GetPoint(0f, new half(0)));
            TestHelpers.CheckFloat2(new float2(2.5f, 0f), spline2D.GetPoint(0.25f, new half(0)));
            TestHelpers.CheckFloat2(new float2(5f, 0f), spline2D.GetPoint(0.5f, new half(0)));
            TestHelpers.CheckFloat2(new float2(9.88f, 0f), spline2D.GetPoint(0.99f, new half(0)), 0.01f);
            TestHelpers.CheckFloat2(d, spline2D.GetPoint(1f, new half(0)));
            TestHelpers.CheckFloat2(d, spline2D.GetPoint(5f, new half(0)));
        }

        [Test]
        public void VariancePoint4()
        {
            IVarianceTestSpline2D spline2D = CreateVarianceSpline();

            float2 a = new float2(3f, 3f);
            spline2D.AddControlPoint(a);

            Assert.AreEqual(1, spline2D.ControlPointCount);
            Assert.AreEqual(0f, spline2D.Length());

            TestHelpers.CheckFloat2(new float2(3f, 3f), spline2D.GetPoint(0.5f, new half(0)));
        }

        [Test]
        public void VariancePoint5()
        {
            IVarianceTestSpline2D spline2D = CreateVarianceSpline();

            float2 a = new float2(1f, 10f);
            spline2D.AddControlPoint(a);
            float2 b = new float2(2f, 10f);
            spline2D.AddControlPoint(b);
            float2 c = new float2(3f, 10f);
            spline2D.AddControlPoint(c);

            Assert.AreEqual(3, spline2D.ControlPointCount);
            Assert.IsTrue(Math.Abs(spline2D.Length() - 2f) < 0.00001f);

            TestHelpers.CheckFloat2(new float2(2.5f, 10f), spline2D.GetPoint(0.7f, new half(0f)), 0.01f);
        }

        [Test]
        public void VariancePointEnd()
        {
            IVarianceTestSpline2D spline2D = CreateVarianceSpline();

            float2 a = float2.zero;
            spline2D.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            spline2D.AddControlPoint(b);

            spline2D.UpdateControlPointLocal(0, new float2(0f, 1f), SplinePointVariance.PointLeft);
            spline2D.UpdateControlPointLocal(0, new float2(0f, 1.1f), SplinePointVariance.PostLeft);
            spline2D.UpdateControlPointLocal(1, new float2(1f, 0.9f), SplinePointVariance.PreLeft);
            spline2D.UpdateControlPointLocal(1, new float2(1f, 1f), SplinePointVariance.PointLeft);

            Assert.AreEqual(2, spline2D.ControlPointCount);

            TestHelpers.CheckFloat2(new float2(1f, 1f), spline2D.GetPoint(1f, new half(-1)));
        }

        [Test]
        public void VarianceUpdate()
        {
            IVarianceTestSpline2D spline2D = CreateVarianceSpline();

            float2 a = float2.zero;
            spline2D.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            spline2D.AddControlPoint(b);

            Assert.AreEqual(2, spline2D.ControlPointCount);
            Assert.AreEqual(2, spline2D.Modes.Count);
            Assert.AreEqual(1, spline2D.Times.Count);
            TestHelpers.CheckFloat2(a, spline2D.GetPoint(0f, new half(0)));
            TestHelpers.CheckFloat2(a, spline2D.GetControlPoint(0, SplinePointVariance.Point));
            TestHelpers.CheckFloat2(b, spline2D.GetPoint(1f, new half(0)));
            TestHelpers.CheckFloat2(b, spline2D.GetControlPoint(1, SplinePointVariance.Point));

            //update 0 point position
            float2 a2 = new float2(-1f, -1f);
            spline2D.UpdateControlPointLocal(0, a2, SplinePointVariance.Point);

            TestHelpers.CheckFloat2(a2, spline2D.GetPoint(0f, new half(0)));
            TestHelpers.CheckFloat2(a2, spline2D.GetControlPoint(0, SplinePointVariance.Point));

            //update 1 point position
            float2 b2 = new float2(2f, 2f);
            spline2D.UpdateControlPointLocal(1, b2, SplinePointVariance.Point);

            TestHelpers.CheckFloat2(b2, spline2D.GetPoint(1f, new half(0)));
            TestHelpers.CheckFloat2(b2, spline2D.GetControlPoint(1, SplinePointVariance.Point));
        }

        [Test]
        public void VarianceUpdate2()
        {
            IVarianceTestSpline2D spline2D = CreateVarianceSpline();

            float2 a = float2.zero;
            spline2D.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            spline2D.AddControlPoint(b);
            float2 c = new float2(2f, 0f);
            spline2D.AddControlPoint(c);

            Assert.AreEqual(3, spline2D.ControlPointCount);
            Assert.AreEqual(3, spline2D.Modes.Count);
            Assert.AreEqual(2, spline2D.Times.Count);
            TestHelpers.CheckFloat2(a, spline2D.GetPoint(0f, new half(0)));
            TestHelpers.CheckFloat2(a, spline2D.GetControlPoint(0, SplinePointVariance.Point));
            TestHelpers.CheckFloat2(b, spline2D.GetPoint(0.5f, new half(0)));
            TestHelpers.CheckFloat2(b, spline2D.GetControlPoint(1, SplinePointVariance.Point));
            TestHelpers.CheckFloat2(c, spline2D.GetPoint(1f, new half(0)));
            TestHelpers.CheckFloat2(c, spline2D.GetControlPoint(2, SplinePointVariance.Point));

            //update 0 point position
            float2 a2 = new float2(0f, 1f);
            spline2D.UpdateControlPointLocal(0, a2, SplinePointVariance.Point);
            TestHelpers.CheckFloat2(a2, spline2D.GetPoint(0f, new half(0)));
            TestHelpers.CheckFloat2(a2, spline2D.GetControlPoint(0, SplinePointVariance.Point));

            //update 1 point position
            float2 b2 = new float2(1f, 1f);
            spline2D.UpdateControlPointLocal(1, b2, SplinePointVariance.Point);
            TestHelpers.CheckFloat2(b2, spline2D.GetControlPoint(1, SplinePointVariance.Point));

            //update 2 point position
            float2 c2 = new float2(2f, 1f);
            spline2D.UpdateControlPointLocal(2, c2, SplinePointVariance.Point);
            TestHelpers.CheckFloat2(c2, spline2D.GetPoint(1f, new half(0)));
            TestHelpers.CheckFloat2(c2, spline2D.GetControlPoint(2, SplinePointVariance.Point));
        }

        [Test]
        public void VarianceUpdate3()
        {
            IVarianceTestSpline2D spline2D = CreateVarianceSpline();

            float2 a = float2.zero;
            spline2D.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            spline2D.AddControlPoint(b);
            float2 c = new float2(2f, 0f);
            spline2D.AddControlPoint(c);

            Assert.AreEqual(2f, spline2D.Length());

            //update 1 point position
            float2 b2 = new float2(1f, 2f);
            spline2D.UpdateControlPointLocal(1, b2, SplinePointVariance.Point);

            Assert.GreaterOrEqual(spline2D.Length(), 2f);
        }

        [Test]
        public void VarianceInsert()
        {
            IVarianceTestSpline2D spline2D = CreateVarianceSpline();

            float2 a = float2.zero;
            spline2D.AddControlPoint(a);
            float2 b = new float2(20f, 0f);
            spline2D.AddControlPoint(b);

            Assert.AreEqual(2, spline2D.ControlPointCount);
            Assert.AreEqual(2, spline2D.Modes.Count);
            Assert.AreEqual(1, spline2D.Times.Count);
            Assert.AreEqual(12, spline2D.ControlPoints.Count);
            TestHelpers.CheckFloat2(a, spline2D.GetPoint(0f, new half(0)));
            TestHelpers.CheckFloat2(a, spline2D.GetControlPoint(0, SplinePointVariance.Point));
            TestHelpers.CheckFloat2(a, spline2D.ControlPoints[0]);
            TestHelpers.CheckFloat2(b, spline2D.GetPoint(1f, new half(0)));
            TestHelpers.CheckFloat2(b, spline2D.GetControlPoint(1, SplinePointVariance.Point));
            TestHelpers.CheckFloat2(b, spline2D.ControlPoints[9]);

            //insert point
            float2 c = new float2(10f, 0f);
            spline2D.InsertControlPoint(1, c);

            Assert.AreEqual(3, spline2D.ControlPointCount);
            Assert.AreEqual(3, spline2D.Modes.Count);
            Assert.AreEqual(2, spline2D.Times.Count);
            Assert.AreEqual(21, spline2D.ControlPoints.Count);
            TestHelpers.CheckFloat2(a, spline2D.ControlPoints[0]);
            TestHelpers.CheckFloat2(c, spline2D.ControlPoints[9]);
            TestHelpers.CheckFloat2(b, spline2D.ControlPoints[18]);
        }

        [Test]
        public void VarianceInsertAtStart()
        {
            IVarianceTestSpline2D spline2D = CreateVarianceSpline();

            float2 a = float2.zero;
            spline2D.AddControlPoint(a);
            float2 b = new float2(2f, 0f);
            spline2D.AddControlPoint(b);

            Assert.AreEqual(2, spline2D.ControlPointCount);
            Assert.AreEqual(2, spline2D.Modes.Count);
            Assert.AreEqual(1, spline2D.Times.Count);
            Assert.AreEqual(12, spline2D.ControlPoints.Count);

            TestHelpers.CheckFloat2(a, spline2D.GetPoint(0f, new half(0)));
            TestHelpers.CheckFloat2(a, spline2D.ControlPoints[0]);
            TestHelpers.CheckFloat2(a, spline2D.GetControlPoint(0, SplinePointVariance.Point));
            TestHelpers.CheckFloat2(b, spline2D.GetPoint(1f, new half(0)));
            TestHelpers.CheckFloat2(b, spline2D.ControlPoints[9]);
            TestHelpers.CheckFloat2(b, spline2D.GetControlPoint(1, SplinePointVariance.Point));

            //insert point
            float2 c = new float2(-2f, 0f);
            spline2D.InsertControlPoint(0, c);

            Assert.AreEqual(3, spline2D.ControlPointCount);
            Assert.AreEqual(3, spline2D.Modes.Count);
            Assert.AreEqual(2, spline2D.Times.Count);
            Assert.AreEqual(21, spline2D.ControlPoints.Count);

            TestHelpers.CheckFloat2(c, spline2D.ControlPoints[0]);
            TestHelpers.CheckFloat2(a, spline2D.ControlPoints[9]);
            TestHelpers.CheckFloat2(b, spline2D.ControlPoints[18]);
            TestHelpers.CheckFloat2(c, spline2D.GetControlPoint(0, SplinePointVariance.Point));
            TestHelpers.CheckFloat2(a, spline2D.GetControlPoint(1, SplinePointVariance.Point));
            TestHelpers.CheckFloat2(b, spline2D.GetControlPoint(2, SplinePointVariance.Point));
        }

        [Test]
        public void VariancePointCreation()
        {
            IVarianceTestSpline2D spline2D = CreateVarianceSpline();

            float2 a = new float2(0f, 0f);
            spline2D.AddControlPoint(a);

            Assert.AreEqual(1, spline2D.ControlPointCount);
            Assert.AreEqual(1, spline2D.Modes.Count);
            Assert.AreEqual(3, spline2D.ControlPoints.Count);

            TestHelpers.CheckFloat2(a, spline2D.GetControlPoint(0, SplinePointVariance.Point));

            float2 b = new float2(10f, 0f);
            spline2D.AddControlPoint(b);

            Assert.AreEqual(2, spline2D.ControlPointCount);
            Assert.AreEqual(2, spline2D.Modes.Count);
            Assert.AreEqual(1, spline2D.Times.Count);
            Assert.AreEqual(10f, spline2D.Length());
            Assert.AreEqual(12, spline2D.ControlPoints.Count);

            TestHelpers.CheckFloat2(a, spline2D.GetControlPoint(0, SplinePointVariance.Point));
            TestHelpers.CheckFloat2(new float2(1f, 0f), spline2D.GetControlPoint(0, SplinePointVariance.Post));
            TestHelpers.CheckFloat2(new float2(9f, 0f), spline2D.GetControlPoint(1, SplinePointVariance.Pre));
            TestHelpers.CheckFloat2(b, spline2D.GetControlPoint(1, SplinePointVariance.Point));
        }

        /// <summary>
        /// Make sure that the points are stored correctly
        /// </summary>
        [Test]
        public void VariancePointArrayVerification()
        {
            IVarianceTestSpline2D spline2D = CreateVarianceSpline();

            float2 a = new float2(10f, 10f);
            spline2D.AddControlPoint(a);
            float2 b = new float2(20f, 20f);
            spline2D.AddControlPoint(b);
            float2 c = new float2(30f, 30f);
            spline2D.AddControlPoint(c);

            Assert.AreEqual(3, spline2D.ControlPointCount);
            Assert.AreEqual(21, spline2D.ControlPoints.Count);

            float s = 1f;

            // Point A
            float2 aPost = a + new float2(s, 0f);
            float2 aLeft = a + new float2(0f, s);
            float2 aLeftPost = a + new float2(s, s);
            float2 aRight = a + new float2(0f, -s);
            float2 aRightPost = a + new float2(s, -s);

            spline2D.UpdateControlPointLocal(0, aPost, SplinePointVariance.Post);
            spline2D.UpdateControlPointLocal(0, aLeft, SplinePointVariance.PointLeft);
            spline2D.UpdateControlPointLocal(0, aLeftPost, SplinePointVariance.PostLeft);
            spline2D.UpdateControlPointLocal(0, aRight, SplinePointVariance.PointRight);
            spline2D.UpdateControlPointLocal(0, aRightPost, SplinePointVariance.PostRight);

            Assert.AreEqual(a, spline2D.GetControlPoint(0, SplinePointVariance.Point));
            Assert.AreEqual(a, spline2D.ControlPoints[0]);
            Assert.AreEqual(aLeft, spline2D.GetControlPoint(0, SplinePointVariance.PointLeft));
            Assert.AreEqual(aLeft, spline2D.ControlPoints[1]);
            Assert.AreEqual(aRight, spline2D.GetControlPoint(0, SplinePointVariance.PointRight));
            Assert.AreEqual(aRight, spline2D.ControlPoints[2]);
            Assert.AreEqual(aPost, spline2D.GetControlPoint(0, SplinePointVariance.Post));
            Assert.AreEqual(aPost, spline2D.ControlPoints[3]);
            Assert.AreEqual(aLeftPost, spline2D.GetControlPoint(0, SplinePointVariance.PostLeft));
            Assert.AreEqual(aLeftPost, spline2D.ControlPoints[4]);
            Assert.AreEqual(aRightPost, spline2D.GetControlPoint(0, SplinePointVariance.PostRight));
            Assert.AreEqual(aRightPost, spline2D.ControlPoints[5]);

            // Point B
            float2 bPost = b + new float2(s, 0f);
            float2 bPre = b + new float2(-s, 0f);
            float2 bLeft = b + new float2(0f, s);
            float2 bLeftPre = b + new float2(-s, s);
            float2 bLeftPost = b + new float2(s, s);
            float2 bRight = b + new float2(0f, -s);
            float2 bRightPre = b + new float2(-s, -s);
            float2 bRightPost = b + new float2(s, -s);

            spline2D.UpdateControlPointLocal(1, bPre, SplinePointVariance.Pre);
            spline2D.UpdateControlPointLocal(1, bPost, SplinePointVariance.Post);
            spline2D.UpdateControlPointLocal(1, bLeft, SplinePointVariance.PointLeft);
            spline2D.UpdateControlPointLocal(1, bLeftPre, SplinePointVariance.PreLeft);
            spline2D.UpdateControlPointLocal(1, bLeftPost, SplinePointVariance.PostLeft);
            spline2D.UpdateControlPointLocal(1, bRight, SplinePointVariance.PointRight);
            spline2D.UpdateControlPointLocal(1, bRightPre, SplinePointVariance.PreRight);
            spline2D.UpdateControlPointLocal(1, bRightPost, SplinePointVariance.PostRight);

            Assert.AreEqual(bPre, spline2D.GetControlPoint(1, SplinePointVariance.Pre));
            Assert.AreEqual(bPre, spline2D.ControlPoints[6]);
            Assert.AreEqual(bLeftPre, spline2D.GetControlPoint(1, SplinePointVariance.PreLeft));
            Assert.AreEqual(bLeftPre, spline2D.ControlPoints[7]);
            Assert.AreEqual(bRightPre, spline2D.GetControlPoint(1, SplinePointVariance.PreRight));
            Assert.AreEqual(bRightPre, spline2D.ControlPoints[8]);
            Assert.AreEqual(b, spline2D.GetControlPoint(1, SplinePointVariance.Point));
            Assert.AreEqual(b, spline2D.ControlPoints[9]);
            Assert.AreEqual(bLeft, spline2D.GetControlPoint(1, SplinePointVariance.PointLeft));
            Assert.AreEqual(bLeft, spline2D.ControlPoints[10]);
            Assert.AreEqual(bRight, spline2D.GetControlPoint(1, SplinePointVariance.PointRight));
            Assert.AreEqual(bRight, spline2D.ControlPoints[11]);
            Assert.AreEqual(bPost, spline2D.GetControlPoint(1, SplinePointVariance.Post));
            Assert.AreEqual(bPost, spline2D.ControlPoints[12]);
            Assert.AreEqual(bLeftPost, spline2D.GetControlPoint(1, SplinePointVariance.PostLeft));
            Assert.AreEqual(bLeftPost, spline2D.ControlPoints[13]);
            Assert.AreEqual(bRightPost, spline2D.GetControlPoint(1, SplinePointVariance.PostRight));
            Assert.AreEqual(bRightPost, spline2D.ControlPoints[14]);

            // Point C
            float2 cPre = c + new float2(-s, 0f);
            float2 cLeft = c + new float2(0f, s);
            float2 cLeftPre = c + new float2(s, s);
            float2 cRight = c + new float2(0f, -s);
            float2 cRightPre = c + new float2(s, -s);

            spline2D.UpdateControlPointLocal(2, cPre, SplinePointVariance.Pre);
            spline2D.UpdateControlPointLocal(2, cLeft, SplinePointVariance.PointLeft);
            spline2D.UpdateControlPointLocal(2, cLeftPre, SplinePointVariance.PreLeft);
            spline2D.UpdateControlPointLocal(2, cRight, SplinePointVariance.PointRight);
            spline2D.UpdateControlPointLocal(2, cRightPre, SplinePointVariance.PreRight);

            Assert.AreEqual(cPre, spline2D.GetControlPoint(2, SplinePointVariance.Pre));
            Assert.AreEqual(cPre, spline2D.ControlPoints[15]);
            Assert.AreEqual(cLeftPre, spline2D.GetControlPoint(2, SplinePointVariance.PreLeft));
            Assert.AreEqual(cLeftPre, spline2D.ControlPoints[16]);
            Assert.AreEqual(cRightPre, spline2D.GetControlPoint(2, SplinePointVariance.PreRight));
            Assert.AreEqual(cRightPre, spline2D.ControlPoints[17]);
            Assert.AreEqual(c, spline2D.GetControlPoint(2, SplinePointVariance.Point));
            Assert.AreEqual(c, spline2D.ControlPoints[18]);
            Assert.AreEqual(cLeft, spline2D.GetControlPoint(2, SplinePointVariance.PointLeft));
            Assert.AreEqual(cLeft, spline2D.ControlPoints[19]);
            Assert.AreEqual(cRight, spline2D.GetControlPoint(2, SplinePointVariance.PointRight));
            Assert.AreEqual(cRight, spline2D.ControlPoints[20]);
        }

        [Test]
        public void Variance()
        {
            IVarianceTestSpline2D spline2D = CreateVarianceSpline();
            const float y = 2f;

            float2 a = new float2(2f, y);
            spline2D.AddControlPoint(a);
            float2 b = new float2(4f, y);
            spline2D.AddControlPoint(b);

            Assert.AreEqual(2, spline2D.ControlPointCount);
            Assert.AreEqual(2f, spline2D.Length());
            Assert.AreEqual(1, spline2D.Times.Count);
            Assert.AreEqual(1f, spline2D.Times[0]);

            float2 al = PositionAngle(a, 180f);
            spline2D.UpdateControlPointLocal(0, al, SplinePointVariance.PointLeft);
            float2 alPost = PositionAngle(al, 90f);
            spline2D.UpdateControlPointLocal(0, alPost, SplinePointVariance.PostLeft);

            float2 ar = PositionAngle(a, 0f);
            spline2D.UpdateControlPointLocal(0, ar, SplinePointVariance.PointRight);
            float2 arPost = PositionAngle(ar, 90f);
            spline2D.UpdateControlPointLocal(0, arPost, SplinePointVariance.PostRight);


            float2 bl = PositionAngle(b, 180f);
            spline2D.UpdateControlPointLocal(1, bl, SplinePointVariance.PointLeft);
            float2 blPre = PositionAngle(bl, -90f);
            spline2D.UpdateControlPointLocal(1, blPre, SplinePointVariance.PreLeft);

            float2 br = PositionAngle(b, 0);
            spline2D.UpdateControlPointLocal(1, br, SplinePointVariance.PointRight);
            float2 brPre = PositionAngle(br, -90f);
            spline2D.UpdateControlPointLocal(1, brPre, SplinePointVariance.PreRight);

            //check spline is still the same
            Assert.AreEqual(2, spline2D.ControlPointCount);
            Assert.AreEqual(2f, spline2D.Length());
            Assert.AreEqual(1, spline2D.Times.Count);
            Assert.AreEqual(1f, spline2D.Times[0]);

            //check updated points
            Assert.AreEqual(alPost, spline2D.GetControlPoint(0, SplinePointVariance.PostLeft), "Point updated incorrectly");
            Assert.AreEqual(arPost, spline2D.GetControlPoint(0, SplinePointVariance.PostRight), "Point updated incorrectly");
            Assert.AreEqual(bl, spline2D.GetControlPoint(1, SplinePointVariance.PointLeft), "Point updated incorrectly");
            Assert.AreEqual(blPre, spline2D.GetControlPoint(1, SplinePointVariance.PreLeft), "Point updated incorrectly");
            Assert.AreEqual(br, spline2D.GetControlPoint(1, SplinePointVariance.PointRight), "Point updated incorrectly");
            Assert.AreEqual(brPre, spline2D.GetControlPoint(1, SplinePointVariance.PreRight), "Point updated incorrectly");

            TestHelpers.CheckFloat2(new float2(b.x - (a.x / 2f), y + 0f), spline2D.GetPoint(0.5f, new half(0)));
            TestHelpers.CheckFloat2(new float2((b.x - (a.x / 2f)), y + -0.5f), spline2D.GetPoint(0.5f, new half(-0.5f)));
            TestHelpers.CheckFloat2(new float2((b.x - (a.x / 2f)), y + 0.5f), spline2D.GetPoint(0.5f, new half(0.5f)));
            TestHelpers.CheckFloat2(new float2((b.x - (a.x / 2f)), y + -1f), spline2D.GetPoint(0.5f, new half(-1)));
            TestHelpers.CheckFloat2(new float2((b.x - (a.x / 2f)), y + 1f), spline2D.GetPoint(0.5f, new half(1)));
        }

        [Test]
        public void VarianceLength()
        {
            IVarianceTestSpline2D spline2D = CreateVarianceSpline();
            const float y = 18f;

            float2 a = new float2(20f, y);
            spline2D.AddControlPoint(a);
            float2 b = new float2(40f, y);
            spline2D.AddControlPoint(b);

            Assert.AreEqual(2, spline2D.ControlPointCount);
            Assert.AreEqual(20f, spline2D.Length());
            Assert.AreEqual(20f, spline2D.Length(new half(-1f)));
            Assert.AreEqual(20f, spline2D.Length(new half(1f)));
            Assert.AreEqual(1, spline2D.Times.Count);
            Assert.AreEqual(1f, spline2D.Times[0]);

            float2 al = PositionAngle(a, 180f);
            spline2D.UpdateControlPointLocal(0, al, SplinePointVariance.PointLeft);
            float2 alPost = PositionAngle(al, 90f);
            spline2D.UpdateControlPointLocal(0, alPost, SplinePointVariance.PostLeft);

            float2 ar = PositionAngle(a, 0f);
            spline2D.UpdateControlPointLocal(0, ar, SplinePointVariance.PointRight);
            float2 arPost = PositionAngle(ar, 90f);
            spline2D.UpdateControlPointLocal(0, arPost, SplinePointVariance.PostRight);


            float2 bl = PositionAngle(b, 180f);
            spline2D.UpdateControlPointLocal(1, bl, SplinePointVariance.PointLeft);
            float2 blPre = PositionAngle(bl, -90f);
            spline2D.UpdateControlPointLocal(1, blPre, SplinePointVariance.PreLeft);

            float2 br = PositionAngle(b, 0);
            spline2D.UpdateControlPointLocal(1, br, SplinePointVariance.PointRight);
            float2 brPre = PositionAngle(br, -90f);
            spline2D.UpdateControlPointLocal(1, brPre, SplinePointVariance.PreRight);

            //check spline is still the same
            Assert.AreEqual(2, spline2D.ControlPointCount);
            
            Assert.AreEqual(20f, spline2D.Length());
            Assert.AreEqual(20f, spline2D.Length(new half(-1f)));
            Assert.AreEqual(20f, spline2D.Length(new half(1f)));
            Assert.AreEqual(1, spline2D.Times.Count);
            Assert.AreEqual(1f, spline2D.Times[0]);
        }

        private float2 PositionAngle(float2 position, float angleDegrees, float magnitude = 1f)
        {
            return new float2(
                position.x + (math.sin(angleDegrees * Mathf.Deg2Rad) * magnitude),
                position.y + (math.cos(angleDegrees * Mathf.Deg2Rad) * magnitude));
        }

        protected abstract IVarianceTestSpline2D CreateVarianceSpline();

        protected void ClearSpline(IVarianceTestSpline2D spline2D)
        {
            spline2D.ClearData();
            
            while (spline2D.ControlPointCount > 0)
            {
                spline2D.RemoveControlPoint(0);
            }

            Assert.AreEqual(0f, spline2D.Length());
            Assert.AreEqual(0, spline2D.ControlPointCount);
            Assert.AreEqual(0, spline2D.ControlPoints.Count);
        }
    }
}