using System;
using System.Collections.Generic;
using Code.Spline2.BezierSpline;
using Code.Spline2.BezierSpline.Entity;
using Code.Spline2.BezierSpline.Jobs;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

namespace Code.Spline2.Test.Variance
{
    public class Spline2DVarianceGameobjectTest : Spline2DVarianceTest
    {
        protected override IVarianceTestSpline CreateSpline()
        {
            GameObject game = new GameObject();
            IVarianceTestSpline spline = game.AddComponent<Spline2DVarianceInspector>();
            Assert.IsNotNull(spline);

            ClearSpline(spline);

            return spline;
        }
        
        public class Spline2DVarianceInspector : Spline2DVariance, IVarianceTestSpline
        {
            public IReadOnlyList<float2> ControlPoints => Points;
            public IReadOnlyList<float> Times
            {
                get
                {
                    if(SegmentLength.Length == 0) return new List<float>();

                    List<float> data = new List<float>(SegmentLength.GetLength(1));
                    for (int i = 0; i < SegmentLength.GetLength(1); i++)
                        data.Add(SegmentLength[0, i]);

                    return data;
                }
            }
            public IReadOnlyList<SplineEditMode> Modes => PointMode;

            public new void ClearData()
            {
                base.ClearData();
                
                Points.Clear();
                SegmentLength = new float[0, 0];
                PointMode.Clear();

                RecalculateLengthBias();
            }
        }
    }
    
    public class Spline2DVarianceJobTest : Spline2DVarianceTest
    {
        protected override IVarianceTestSpline CreateSpline()
        {
            GameObject game = new GameObject();
            IVarianceTestSpline spline = game.AddComponent<Spline2DVarianceJobInspector>();
            Assert.IsNotNull(spline);

            ClearSpline(spline);

            return spline;
        }
        
        public class Spline2DVarianceJobInspector : Spline2DVariance, IVarianceTestSpline
        {
            public IReadOnlyList<float2> ControlPoints => Points;
            public IReadOnlyList<float> Times
            {
                get
                {
                    if(SegmentLength.Length == 0) return new List<float>();

                    List<float> data = new List<float>(SegmentLength.GetLength(1));
                    for (int i = 0; i < SegmentLength.GetLength(1); i++)
                        data.Add(SegmentLength[0, i]);

                    return data;
                }
            }
            public IReadOnlyList<SplineEditMode> Modes => PointMode;

            public new int ControlPointCount
            {
                get
                {
                    ClearData();
                    ConvertSplineData();
                    Assert.IsTrue(SplineEntityData.HasValue, "Failed to generate spline");

                    return SplineEntityData.Value.ControlPointCount;
                }
            }
            
            protected new float Length()
            {
                ClearData();
                ConvertSplineData();
                Assert.IsTrue(SplineEntityData.HasValue, "Failed to generate spline");

                return SplineEntityData.Value.Length[0];
            }
            
            public new float2 GetPoint(float progress, half variance)
            {
                ClearData();
                ConvertSplineData();
                Assert.IsTrue(SplineEntityData.HasValue, "Failed to generate spline");

                Spline2DVariancePointJob job = new Spline2DVariancePointJob()
                {
                    Spline = SplineEntityData.Value,
                    SplineProgress = new SplineProgress() {Progress = progress},
                    SplineVariance = new SplineVariance() {Variance = variance},
                };
                job.Execute();

                return job.Result;
            }
            
            public new void ClearData()
            {
                base.ClearData();
                
                RecalculateLengthBias();
            }
        }
    }
    
    public abstract class Spline2DVarianceTest
    {
        [Test]
        public void Basic()
        {
            CreateSpline();
        }

        [Test]
        public void Add1()
        {
            IVarianceTestSpline spline = CreateSpline();

            float2 a = float2.zero;
            spline.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            spline.AddControlPoint(b);

            Assert.AreEqual(2, spline.ControlPointCount);
            Assert.AreEqual(2, spline.Modes.Count);
            Assert.AreEqual(1, spline.Times.Count);
            Assert.AreEqual(1f, spline.Length());

            CheckFloat2(a, spline.GetControlPoint(0, SplinePointVariance.Point));
            CheckFloat2(b, spline.GetControlPoint(1, SplinePointVariance.Point));
        }

        [Test]
        public void Add2()
        {
            IVarianceTestSpline spline = CreateSpline();

            float2 a = float2.zero;
            spline.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            spline.AddControlPoint(b);
            float2 c = new float2(2f, 0f);
            spline.AddControlPoint(c);

            Assert.AreEqual(3, spline.ControlPointCount);
            Assert.AreEqual(3, spline.Modes.Count);
            Assert.AreEqual(2, spline.Times.Count);
            Assert.AreEqual(2f, spline.Length());

            CheckFloat2(a, spline.GetControlPoint(0, SplinePointVariance.Point));
            CheckFloat2(b, spline.GetControlPoint(1, SplinePointVariance.Point));
            CheckFloat2(c, spline.GetControlPoint(2, SplinePointVariance.Point));
        }

        [Test]
        public void Remove()
        {
            IVarianceTestSpline spline = CreateSpline();

            float2 a = float2.zero;
            spline.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            spline.AddControlPoint(b);
            float2 c = new float2(2f, 0f);
            spline.AddControlPoint(c);

            Assert.AreEqual(3, spline.ControlPointCount);
            Assert.AreEqual(3, spline.Modes.Count);
            Assert.AreEqual(21, spline.ControlPoints.Count);
            Assert.AreEqual(2, spline.Times.Count);
            Assert.AreEqual(2f, spline.Length());

            CheckFloat2(a, spline.GetControlPoint(0, SplinePointVariance.Point));
            CheckFloat2(b, spline.GetControlPoint(1, SplinePointVariance.Point));
            CheckFloat2(c, spline.GetControlPoint(2, SplinePointVariance.Point));

            //Remove a point
            spline.RemoveControlPoint(1);

            Assert.AreEqual(2, spline.ControlPointCount);
            Assert.AreEqual(2, spline.Modes.Count);
            Assert.AreEqual(12, spline.ControlPoints.Count);
            Assert.AreEqual(1, spline.Times.Count);
            Assert.AreEqual(2f, spline.Length());

            CheckFloat2(a, spline.GetControlPoint(0, SplinePointVariance.Point));
            CheckFloat2(c, spline.GetControlPoint(1, SplinePointVariance.Point));
        }

        [Test]
        public void RemoveFromStart()
        {
            IVarianceTestSpline spline = CreateSpline();

            float2 a = float2.zero;
            spline.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            spline.AddControlPoint(b);

            Assert.AreEqual(2, spline.ControlPointCount);
            Assert.AreEqual(2, spline.Modes.Count);
            Assert.AreEqual(12, spline.ControlPoints.Count);
            Assert.AreEqual(1, spline.Times.Count);
            Assert.AreEqual(1f, spline.Length());

            CheckFloat2(a, spline.GetControlPoint(0, SplinePointVariance.Point));
            CheckFloat2(b, spline.GetControlPoint(1, SplinePointVariance.Point));

            //Remove a point
            spline.RemoveControlPoint(0);

            Assert.AreEqual(1, spline.ControlPointCount);
            Assert.AreEqual(1, spline.Modes.Count);
            Assert.AreEqual(3, spline.ControlPoints.Count);
            Assert.AreEqual(1, spline.Times.Count);
            Assert.AreEqual(0f, spline.Length());

            CheckFloat2(b, spline.GetControlPoint(0, SplinePointVariance.Point));
        }

        [Test]
        public void RemoveFromEnd()
        {
            IVarianceTestSpline spline = CreateSpline();

            float2 a = float2.zero;
            spline.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            spline.AddControlPoint(b);
            float2 c = new float2(2f, 0f);
            spline.AddControlPoint(c);

            Assert.AreEqual(3, spline.ControlPointCount);
            Assert.AreEqual(3, spline.Modes.Count);
            Assert.AreEqual(21, spline.ControlPoints.Count);
            Assert.AreEqual(2, spline.Times.Count);
            Assert.AreEqual(2f, spline.Length());

            CheckFloat2(a, spline.GetControlPoint(0, SplinePointVariance.Point));
            CheckFloat2(b, spline.GetControlPoint(1, SplinePointVariance.Point));
            CheckFloat2(c, spline.GetControlPoint(2, SplinePointVariance.Point));

            //Remove a point
            spline.RemoveControlPoint(2);

            Assert.AreEqual(2, spline.ControlPointCount);
            Assert.AreEqual(2, spline.Modes.Count);
            Assert.AreEqual(12, spline.ControlPoints.Count);
            Assert.AreEqual(1, spline.Times.Count);
            Assert.AreEqual(1f, spline.Length());

            CheckFloat2(a, spline.GetControlPoint(0, SplinePointVariance.Point));
            CheckFloat2(b, spline.GetControlPoint(1, SplinePointVariance.Point));

            //Remove a point
            spline.RemoveControlPoint(1);

            Assert.AreEqual(1, spline.ControlPointCount);
            Assert.AreEqual(1, spline.Modes.Count);
            Assert.AreEqual(3, spline.ControlPoints.Count);
            Assert.AreEqual(1, spline.Times.Count);
            Assert.AreEqual(0f, spline.Length());

            CheckFloat2(a, spline.GetControlPoint(0, SplinePointVariance.Point));

            //Remove final point
            spline.RemoveControlPoint(0);

            Assert.AreEqual(0, spline.ControlPointCount);
            Assert.AreEqual(0, spline.Modes.Count);
            Assert.AreEqual(0, spline.ControlPoints.Count);
            Assert.AreEqual(0, spline.Times.Count);
            Assert.AreEqual(0f, spline.Length());
        }

        [Test]
        public void Point()
        {
            IVarianceTestSpline spline = CreateSpline();

            float2 a = float2.zero;
            spline.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            spline.AddControlPoint(b);

            Assert.AreEqual(2, spline.ControlPointCount);
            Assert.AreEqual(1f, spline.Length());

            Assert.AreEqual(1, spline.Times.Count);
            Assert.AreEqual(1f, spline.Times[0]);

            CheckFloat2(new float2(0.5f, 0f), spline.GetPoint(0.5f, new half(0)));
        }

        [Test]
        public void Point2()
        {
            IVarianceTestSpline spline = CreateSpline();

            float2 a = float2.zero;
            spline.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            spline.AddControlPoint(b);
            float2 c = new float2(2f, 0f);
            spline.AddControlPoint(c);

            Assert.AreEqual(3, spline.ControlPointCount);
            Assert.AreEqual(2f, spline.Length());

            Assert.AreEqual(2, spline.Times.Count);
            Assert.AreEqual(0.5f, spline.Times[0]);
            Assert.AreEqual(1f, spline.Times[1]);

            CheckFloat2(a, spline.GetPoint(0f, new half(0)));
            CheckFloat2(new float2(1f, 0f), spline.GetPoint(0.5f, new half(0)));
            //CheckFloat2(c * 0.77f, spline.GetPoint(0.77f)); // fails due to bezier point bunching issues
        }

        [Test]
        public void Point3()
        {
            IVarianceTestSpline spline = CreateSpline();

            float2 a = float2.zero;
            spline.AddControlPoint(a);
            float2 b = new float2(2.5f, 0f);
            spline.AddControlPoint(b);
            float2 c = new float2(7.5f, 0f);
            spline.AddControlPoint(c);
            float2 d = new float2(10f, 0f);
            spline.AddControlPoint(d);

            Assert.AreEqual(4, spline.ControlPointCount);
            Assert.AreEqual(4, spline.Modes.Count);
            Assert.AreEqual(10f, spline.Length());

            Assert.AreEqual(3, spline.Times.Count);
            Assert.AreEqual(0.25f, spline.Times[0]);
            Assert.AreEqual(0.75f, spline.Times[1]);
            Assert.AreEqual(1f, spline.Times[2]);

            CheckFloat2(a, spline.GetPoint(0f, new half(0)));
            CheckFloat2(new float2(2.5f, 0f), spline.GetPoint(0.25f, new half(0)));
            CheckFloat2(new float2(5f, 0f), spline.GetPoint(0.5f, new half(0)));
            CheckFloat2(new float2(9.88f, 0f), spline.GetPoint(0.99f, new half(0)), 0.01f);
            CheckFloat2(d, spline.GetPoint(1f, new half(0)));
            CheckFloat2(d, spline.GetPoint(5f, new half(0)));
        }

        [Test]
        public void Point4()
        {
            IVarianceTestSpline spline = CreateSpline();

            float2 a = new float2(3f, 3f);
            spline.AddControlPoint(a);

            Assert.AreEqual(1, spline.ControlPointCount);
            Assert.AreEqual(0f, spline.Length());

            CheckFloat2(new float2(3f, 3f), spline.GetPoint(0.5f, new half(0)));
        }

        [Test]
        public void Point5()
        {
            IVarianceTestSpline spline = CreateSpline();

            float2 a = new float2(1f, 10f);
            spline.AddControlPoint(a);
            float2 b = new float2(2f, 10f);
            spline.AddControlPoint(b);
            float2 c = new float2(3f, 10f);
            spline.AddControlPoint(c);

            Assert.AreEqual(3, spline.ControlPointCount);
            Assert.IsTrue(Math.Abs(spline.Length() - 2f) < 0.00001f);

            CheckFloat2(new float2(2.5f, 10f), spline.GetPoint(0.7f, new half(0f)), 0.01f);
        }

        [Test]
        public void PointEnd()
        {
            IVarianceTestSpline spline = CreateSpline();

            float2 a = float2.zero;
            spline.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            spline.AddControlPoint(b);

            spline.UpdateControlPoint(0, new float2(0f, 1f), SplinePointVariance.PointLeft);
            spline.UpdateControlPoint(0, new float2(0f, 1.1f), SplinePointVariance.PostLeft);
            spline.UpdateControlPoint(1, new float2(1f, 0.9f), SplinePointVariance.PreLeft);
            spline.UpdateControlPoint(1, new float2(1f, 1f), SplinePointVariance.PointLeft);

            Assert.AreEqual(2, spline.ControlPointCount);

            CheckFloat2(new float2(1f, 1f), spline.GetPoint(1f, new half(-1)));
        }

        [Test]
        public void Update()
        {
            IVarianceTestSpline spline = CreateSpline();

            float2 a = float2.zero;
            spline.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            spline.AddControlPoint(b);

            Assert.AreEqual(2, spline.ControlPointCount);
            Assert.AreEqual(2, spline.Modes.Count);
            Assert.AreEqual(1, spline.Times.Count);
            CheckFloat2(a, spline.GetPoint(0f, new half(0)));
            CheckFloat2(a, spline.GetControlPoint(0, SplinePointVariance.Point));
            CheckFloat2(b, spline.GetPoint(1f, new half(0)));
            CheckFloat2(b, spline.GetControlPoint(1, SplinePointVariance.Point));

            //update 0 point position
            float2 a2 = new float2(-1f, -1f);
            spline.UpdateControlPoint(0, a2, SplinePointVariance.Point);

            CheckFloat2(a2, spline.GetPoint(0f, new half(0)));
            CheckFloat2(a2, spline.GetControlPoint(0, SplinePointVariance.Point));

            //update 1 point position
            float2 b2 = new float2(2f, 2f);
            spline.UpdateControlPoint(1, b2, SplinePointVariance.Point);

            CheckFloat2(b2, spline.GetPoint(1f, new half(0)));
            CheckFloat2(b2, spline.GetControlPoint(1, SplinePointVariance.Point));
        }

        [Test]
        public void Update2()
        {
            IVarianceTestSpline spline = CreateSpline();

            float2 a = float2.zero;
            spline.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            spline.AddControlPoint(b);
            float2 c = new float2(2f, 0f);
            spline.AddControlPoint(c);

            Assert.AreEqual(3, spline.ControlPointCount);
            Assert.AreEqual(3, spline.Modes.Count);
            Assert.AreEqual(2, spline.Times.Count);
            CheckFloat2(a, spline.GetPoint(0f, new half(0)));
            CheckFloat2(a, spline.GetControlPoint(0, SplinePointVariance.Point));
            CheckFloat2(b, spline.GetPoint(0.5f, new half(0)));
            CheckFloat2(b, spline.GetControlPoint(1, SplinePointVariance.Point));
            CheckFloat2(c, spline.GetPoint(1f, new half(0)));
            CheckFloat2(c, spline.GetControlPoint(2, SplinePointVariance.Point));

            //update 0 point position
            float2 a2 = new float2(0f, 1f);
            spline.UpdateControlPoint(0, a2, SplinePointVariance.Point);
            CheckFloat2(a2, spline.GetPoint(0f, new half(0)));
            CheckFloat2(a2, spline.GetControlPoint(0, SplinePointVariance.Point));

            //update 1 point position
            float2 b2 = new float2(1f, 1f);
            spline.UpdateControlPoint(1, b2, SplinePointVariance.Point);
            CheckFloat2(b2, spline.GetControlPoint(1, SplinePointVariance.Point));

            //update 2 point position
            float2 c2 = new float2(2f, 1f);
            spline.UpdateControlPoint(2, c2, SplinePointVariance.Point);
            CheckFloat2(c2, spline.GetPoint(1f, new half(0)));
            CheckFloat2(c2, spline.GetControlPoint(2, SplinePointVariance.Point));
        }

        [Test]
        public void Update3()
        {
            IVarianceTestSpline spline = CreateSpline();

            float2 a = float2.zero;
            spline.AddControlPoint(a);
            float2 b = new float2(1f, 0f);
            spline.AddControlPoint(b);
            float2 c = new float2(2f, 0f);
            spline.AddControlPoint(c);

            Assert.AreEqual(2f, spline.Length());

            //update 1 point position
            float2 b2 = new float2(1f, 2f);
            spline.UpdateControlPoint(1, b2, SplinePointVariance.Point);

            Assert.GreaterOrEqual(spline.Length(), 2f);
        }

        [Test]
        public void Insert()
        {
            IVarianceTestSpline spline = CreateSpline();

            float2 a = float2.zero;
            spline.AddControlPoint(a);
            float2 b = new float2(20f, 0f);
            spline.AddControlPoint(b);

            Assert.AreEqual(2, spline.ControlPointCount);
            Assert.AreEqual(2, spline.Modes.Count);
            Assert.AreEqual(1, spline.Times.Count);
            Assert.AreEqual(12, spline.ControlPoints.Count);
            CheckFloat2(a, spline.GetPoint(0f, new half(0)));
            CheckFloat2(a, spline.GetControlPoint(0, SplinePointVariance.Point));
            CheckFloat2(a, spline.ControlPoints[0]);
            CheckFloat2(b, spline.GetPoint(1f, new half(0)));
            CheckFloat2(b, spline.GetControlPoint(1, SplinePointVariance.Point));
            CheckFloat2(b, spline.ControlPoints[9]);

            //insert point
            float2 c = new float2(10f, 0f);
            spline.InsertControlPoint(1, c);

            Assert.AreEqual(3, spline.ControlPointCount);
            Assert.AreEqual(3, spline.Modes.Count);
            Assert.AreEqual(2, spline.Times.Count);
            Assert.AreEqual(21, spline.ControlPoints.Count);
            CheckFloat2(a, spline.ControlPoints[0]);
            CheckFloat2(c, spline.ControlPoints[9]);
            CheckFloat2(b, spline.ControlPoints[18]);
        }

        [Test]
        public void InsertAtStart()
        {
            IVarianceTestSpline spline = CreateSpline();

            float2 a = float2.zero;
            spline.AddControlPoint(a);
            float2 b = new float2(2f, 0f);
            spline.AddControlPoint(b);

            Assert.AreEqual(2, spline.ControlPointCount);
            Assert.AreEqual(2, spline.Modes.Count);
            Assert.AreEqual(1, spline.Times.Count);
            Assert.AreEqual(12, spline.ControlPoints.Count);

            CheckFloat2(a, spline.GetPoint(0f, new half(0)));
            CheckFloat2(a, spline.ControlPoints[0]);
            CheckFloat2(a, spline.GetControlPoint(0, SplinePointVariance.Point));
            CheckFloat2(b, spline.GetPoint(1f, new half(0)));
            CheckFloat2(b, spline.ControlPoints[9]);
            CheckFloat2(b, spline.GetControlPoint(1, SplinePointVariance.Point));

            //insert point
            float2 c = new float2(-2f, 0f);
            spline.InsertControlPoint(0, c);

            Assert.AreEqual(3, spline.ControlPointCount);
            Assert.AreEqual(3, spline.Modes.Count);
            Assert.AreEqual(2, spline.Times.Count);
            Assert.AreEqual(21, spline.ControlPoints.Count);

            CheckFloat2(c, spline.ControlPoints[0]);
            CheckFloat2(a, spline.ControlPoints[9]);
            CheckFloat2(b, spline.ControlPoints[18]);
            CheckFloat2(c, spline.GetControlPoint(0, SplinePointVariance.Point));
            CheckFloat2(a, spline.GetControlPoint(1, SplinePointVariance.Point));
            CheckFloat2(b, spline.GetControlPoint(2, SplinePointVariance.Point));
        }

        [Test]
        public void PointCreation()
        {
            IVarianceTestSpline spline = CreateSpline();

            float2 a = new float2(0f, 0f);
            spline.AddControlPoint(a);

            Assert.AreEqual(1, spline.ControlPointCount);
            Assert.AreEqual(1, spline.Modes.Count);
            Assert.AreEqual(0, spline.Times.Count);
            Assert.AreEqual(3, spline.ControlPoints.Count);

            CheckFloat2(a, spline.GetControlPoint(0, SplinePointVariance.Point));

            float2 b = new float2(10f, 0f);
            spline.AddControlPoint(b);

            Assert.AreEqual(2, spline.ControlPointCount);
            Assert.AreEqual(2, spline.Modes.Count);
            Assert.AreEqual(1, spline.Times.Count);
            Assert.AreEqual(10f, spline.Length());
            Assert.AreEqual(12, spline.ControlPoints.Count);

            CheckFloat2(a, spline.GetControlPoint(0, SplinePointVariance.Point));
            CheckFloat2(new float2(1f, 0f), spline.GetControlPoint(0, SplinePointVariance.Post));
            CheckFloat2(new float2(9f, 0f), spline.GetControlPoint(1, SplinePointVariance.Pre));
            CheckFloat2(b, spline.GetControlPoint(1, SplinePointVariance.Point));
        }

        /// <summary>
        /// Make sure that the points are stored correctly
        /// </summary>
        [Test]
        public void PointArrayVerification()
        {
            IVarianceTestSpline spline = CreateSpline();

            float2 a = new float2(10f, 10f);
            spline.AddControlPoint(a);
            float2 b = new float2(20f, 20f);
            spline.AddControlPoint(b);
            float2 c = new float2(30f, 30f);
            spline.AddControlPoint(c);

            Assert.AreEqual(3, spline.ControlPointCount);
            Assert.AreEqual(21, spline.ControlPoints.Count);

            float s = 1f;

            // Point A
            float2 aPost = a + new float2(s, 0f);
            float2 aLeft = a + new float2(0f, s);
            float2 aLeftPost = a + new float2(s, s);
            float2 aRight = a + new float2(0f, -s);
            float2 aRightPost = a + new float2(s, -s);

            spline.UpdateControlPoint(0, aPost, SplinePointVariance.Post);
            spline.UpdateControlPoint(0, aLeft, SplinePointVariance.PointLeft);
            spline.UpdateControlPoint(0, aLeftPost, SplinePointVariance.PostLeft);
            spline.UpdateControlPoint(0, aRight, SplinePointVariance.PointRight);
            spline.UpdateControlPoint(0, aRightPost, SplinePointVariance.PostRight);

            Assert.AreEqual(a, spline.GetControlPoint(0, SplinePointVariance.Point));
            Assert.AreEqual(a, spline.ControlPoints[0]);
            Assert.AreEqual(aLeft, spline.GetControlPoint(0, SplinePointVariance.PointLeft));
            Assert.AreEqual(aLeft, spline.ControlPoints[1]);
            Assert.AreEqual(aRight, spline.GetControlPoint(0, SplinePointVariance.PointRight));
            Assert.AreEqual(aRight, spline.ControlPoints[2]);
            Assert.AreEqual(aPost, spline.GetControlPoint(0, SplinePointVariance.Post));
            Assert.AreEqual(aPost, spline.ControlPoints[3]);
            Assert.AreEqual(aLeftPost, spline.GetControlPoint(0, SplinePointVariance.PostLeft));
            Assert.AreEqual(aLeftPost, spline.ControlPoints[4]);
            Assert.AreEqual(aRightPost, spline.GetControlPoint(0, SplinePointVariance.PostRight));
            Assert.AreEqual(aRightPost, spline.ControlPoints[5]);

            // Point B
            float2 bPost = b + new float2(s, 0f);
            float2 bPre = b + new float2(-s, 0f);
            float2 bLeft = b + new float2(0f, s);
            float2 bLeftPre = b + new float2(-s, s);
            float2 bLeftPost = b + new float2(s, s);
            float2 bRight = b + new float2(0f, -s);
            float2 bRightPre = b + new float2(-s, -s);
            float2 bRightPost = b + new float2(s, -s);

            spline.UpdateControlPoint(1, bPre, SplinePointVariance.Pre);
            spline.UpdateControlPoint(1, bPost, SplinePointVariance.Post);
            spline.UpdateControlPoint(1, bLeft, SplinePointVariance.PointLeft);
            spline.UpdateControlPoint(1, bLeftPre, SplinePointVariance.PreLeft);
            spline.UpdateControlPoint(1, bLeftPost, SplinePointVariance.PostLeft);
            spline.UpdateControlPoint(1, bRight, SplinePointVariance.PointRight);
            spline.UpdateControlPoint(1, bRightPre, SplinePointVariance.PreRight);
            spline.UpdateControlPoint(1, bRightPost, SplinePointVariance.PostRight);

            Assert.AreEqual(bPre, spline.GetControlPoint(1, SplinePointVariance.Pre));
            Assert.AreEqual(bPre, spline.ControlPoints[6]);
            Assert.AreEqual(bLeftPre, spline.GetControlPoint(1, SplinePointVariance.PreLeft));
            Assert.AreEqual(bLeftPre, spline.ControlPoints[7]);
            Assert.AreEqual(bRightPre, spline.GetControlPoint(1, SplinePointVariance.PreRight));
            Assert.AreEqual(bRightPre, spline.ControlPoints[8]);
            Assert.AreEqual(b, spline.GetControlPoint(1, SplinePointVariance.Point));
            Assert.AreEqual(b, spline.ControlPoints[9]);
            Assert.AreEqual(bLeft, spline.GetControlPoint(1, SplinePointVariance.PointLeft));
            Assert.AreEqual(bLeft, spline.ControlPoints[10]);
            Assert.AreEqual(bRight, spline.GetControlPoint(1, SplinePointVariance.PointRight));
            Assert.AreEqual(bRight, spline.ControlPoints[11]);
            Assert.AreEqual(bPost, spline.GetControlPoint(1, SplinePointVariance.Post));
            Assert.AreEqual(bPost, spline.ControlPoints[12]);
            Assert.AreEqual(bLeftPost, spline.GetControlPoint(1, SplinePointVariance.PostLeft));
            Assert.AreEqual(bLeftPost, spline.ControlPoints[13]);
            Assert.AreEqual(bRightPost, spline.GetControlPoint(1, SplinePointVariance.PostRight));
            Assert.AreEqual(bRightPost, spline.ControlPoints[14]);

            // Point C
            float2 cPre = c + new float2(-s, 0f);
            float2 cLeft = c + new float2(0f, s);
            float2 cLeftPre = c + new float2(s, s);
            float2 cRight = c + new float2(0f, -s);
            float2 cRightPre = c + new float2(s, -s);

            spline.UpdateControlPoint(2, cPre, SplinePointVariance.Pre);
            spline.UpdateControlPoint(2, cLeft, SplinePointVariance.PointLeft);
            spline.UpdateControlPoint(2, cLeftPre, SplinePointVariance.PreLeft);
            spline.UpdateControlPoint(2, cRight, SplinePointVariance.PointRight);
            spline.UpdateControlPoint(2, cRightPre, SplinePointVariance.PreRight);

            Assert.AreEqual(cPre, spline.GetControlPoint(2, SplinePointVariance.Pre));
            Assert.AreEqual(cPre, spline.ControlPoints[15]);
            Assert.AreEqual(cLeftPre, spline.GetControlPoint(2, SplinePointVariance.PreLeft));
            Assert.AreEqual(cLeftPre, spline.ControlPoints[16]);
            Assert.AreEqual(cRightPre, spline.GetControlPoint(2, SplinePointVariance.PreRight));
            Assert.AreEqual(cRightPre, spline.ControlPoints[17]);
            Assert.AreEqual(c, spline.GetControlPoint(2, SplinePointVariance.Point));
            Assert.AreEqual(c, spline.ControlPoints[18]);
            Assert.AreEqual(cLeft, spline.GetControlPoint(2, SplinePointVariance.PointLeft));
            Assert.AreEqual(cLeft, spline.ControlPoints[19]);
            Assert.AreEqual(cRight, spline.GetControlPoint(2, SplinePointVariance.PointRight));
            Assert.AreEqual(cRight, spline.ControlPoints[20]);
        }

        [Test]
        public void Variance()
        {
            IVarianceTestSpline spline = CreateSpline();
            const float y = 2f;

            float2 a = new float2(2f, y);
            spline.AddControlPoint(a);
            float2 b = new float2(4f, y);
            spline.AddControlPoint(b);

            Assert.AreEqual(2, spline.ControlPointCount);
            Assert.AreEqual(2f, spline.Length());
            Assert.AreEqual(1, spline.Times.Count);
            Assert.AreEqual(1f, spline.Times[0]);

            float2 al = PositionAngle(a, 180f);
            spline.UpdateControlPoint(0, al, SplinePointVariance.PointLeft);
            float2 alPost = PositionAngle(al, 90f);
            spline.UpdateControlPoint(0, alPost, SplinePointVariance.PostLeft);

            float2 ar = PositionAngle(a, 0f);
            spline.UpdateControlPoint(0, ar, SplinePointVariance.PointRight);
            float2 arPost = PositionAngle(ar, 90f);
            spline.UpdateControlPoint(0, arPost, SplinePointVariance.PostRight);


            float2 bl = PositionAngle(b, 180f);
            spline.UpdateControlPoint(1, bl, SplinePointVariance.PointLeft);
            float2 blPre = PositionAngle(bl, -90f);
            spline.UpdateControlPoint(1, blPre, SplinePointVariance.PreLeft);

            float2 br = PositionAngle(b, 0);
            spline.UpdateControlPoint(1, br, SplinePointVariance.PointRight);
            float2 brPre = PositionAngle(br, -90f);
            spline.UpdateControlPoint(1, brPre, SplinePointVariance.PreRight);

            //check spline is still the same
            Assert.AreEqual(2, spline.ControlPointCount);
            Assert.AreEqual(2f, spline.Length());
            Assert.AreEqual(1, spline.Times.Count);
            Assert.AreEqual(1f, spline.Times[0]);

            //check updated points
            Assert.AreEqual(alPost, spline.GetControlPoint(0, SplinePointVariance.PostLeft), "Point updated incorrectly");
            Assert.AreEqual(arPost, spline.GetControlPoint(0, SplinePointVariance.PostRight), "Point updated incorrectly");
            Assert.AreEqual(bl, spline.GetControlPoint(1, SplinePointVariance.PointLeft), "Point updated incorrectly");
            Assert.AreEqual(blPre, spline.GetControlPoint(1, SplinePointVariance.PreLeft), "Point updated incorrectly");
            Assert.AreEqual(br, spline.GetControlPoint(1, SplinePointVariance.PointRight), "Point updated incorrectly");
            Assert.AreEqual(brPre, spline.GetControlPoint(1, SplinePointVariance.PreRight), "Point updated incorrectly");

            CheckFloat2(new float2(b.x - (a.x / 2f), y + 0f), spline.GetPoint(0.5f, new half(0)));
            CheckFloat2(new float2((b.x - (a.x / 2f)), y + -0.5f), spline.GetPoint(0.5f, new half(-0.5f)));
            CheckFloat2(new float2((b.x - (a.x / 2f)), y + 0.5f), spline.GetPoint(0.5f, new half(0.5f)));
            CheckFloat2(new float2((b.x - (a.x / 2f)), y + -1f), spline.GetPoint(0.5f, new half(-1)));
            CheckFloat2(new float2((b.x - (a.x / 2f)), y + 1f), spline.GetPoint(0.5f, new half(1)));
        }

        [Test]
        public void VarianceLength()
        {
            IVarianceTestSpline spline = CreateSpline();
            const float y = 18f;

            float2 a = new float2(20f, y);
            spline.AddControlPoint(a);
            float2 b = new float2(40f, y);
            spline.AddControlPoint(b);

            Assert.AreEqual(2, spline.ControlPointCount);
            Assert.AreEqual(20f, spline.Length());
            Assert.AreEqual(20f, spline.Length(new half(-1f)));
            Assert.AreEqual(20f, spline.Length(new half(1f)));
            Assert.AreEqual(1, spline.Times.Count);
            Assert.AreEqual(1f, spline.Times[0]);

            float2 al = PositionAngle(a, 180f);
            spline.UpdateControlPoint(0, al, SplinePointVariance.PointLeft);
            float2 alPost = PositionAngle(al, 90f);
            spline.UpdateControlPoint(0, alPost, SplinePointVariance.PostLeft);

            float2 ar = PositionAngle(a, 0f);
            spline.UpdateControlPoint(0, ar, SplinePointVariance.PointRight);
            float2 arPost = PositionAngle(ar, 90f);
            spline.UpdateControlPoint(0, arPost, SplinePointVariance.PostRight);


            float2 bl = PositionAngle(b, 180f);
            spline.UpdateControlPoint(1, bl, SplinePointVariance.PointLeft);
            float2 blPre = PositionAngle(bl, -90f);
            spline.UpdateControlPoint(1, blPre, SplinePointVariance.PreLeft);

            float2 br = PositionAngle(b, 0);
            spline.UpdateControlPoint(1, br, SplinePointVariance.PointRight);
            float2 brPre = PositionAngle(br, -90f);
            spline.UpdateControlPoint(1, brPre, SplinePointVariance.PreRight);

            //check spline is still the same
            Assert.AreEqual(2, spline.ControlPointCount);
            
            Assert.AreEqual(20f, spline.Length());
            Assert.AreEqual(20f, spline.Length(new half(-1f)));
            Assert.AreEqual(20f, spline.Length(new half(1f)));
            Assert.AreEqual(1, spline.Times.Count);
            Assert.AreEqual(1f, spline.Times[0]);
        }

        private float2 PositionAngle(float2 position, float angleDegrees, float magnitude = 1f)
        {
            return new float2(
                position.x + (math.sin(angleDegrees * Mathf.Deg2Rad) * magnitude),
                position.y + (math.cos(angleDegrees * Mathf.Deg2Rad) * magnitude));
        }

        private void CheckFloat2(float2 expected, float2 reality, float tolerance = 0.00001f)
        {
            Assert.IsTrue(math.length(math.abs(expected.x - reality.x)) <= tolerance,
                $"X axis is out of range!\n Expected: {expected.x:N4}\n Received: {reality.x:N4}\n Tolerance: {tolerance:N4}");
            Assert.IsTrue(math.length(math.abs(expected.y - reality.y)) <= tolerance,
                $"Y axis is out of range!\n Expected: {expected.y:N4}\n Received: {reality.y:N4}\n Tolerance: {tolerance:N4}");
        }

        protected abstract IVarianceTestSpline CreateSpline();

        protected void ClearSpline(IVarianceTestSpline spline)
        {
            spline.ClearData();
            
            while (spline.ControlPointCount > 0)
            {
                spline.RemoveControlPoint(0);
            }

            Assert.AreEqual(0f, spline.Length());
            Assert.AreEqual(0, spline.ControlPointCount);
            Assert.AreEqual(0, spline.ControlPoints.Count);
        }
    }
}