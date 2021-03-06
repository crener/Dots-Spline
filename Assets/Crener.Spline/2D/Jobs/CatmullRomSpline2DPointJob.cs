﻿using Crener.Spline.Common;
using Crener.Spline.Common.DataStructs;
using Crener.Spline.Common.Interfaces;
using Unity.Assertions;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Crener.Spline._2D.Jobs
{
    /// <summary>
    /// Simple way of sampling a single point from a 2D spline via <see cref="Spline2DData"/>
    /// </summary>
    [BurstCompile, BurstCompatible]
    public struct CatmullRomSpline2DPointJob : IJob, ISplineJob2D
    {
        [ReadOnly]
        public Spline2DData Spline;
        [ReadOnly]
        private SplineProgress m_splineProgress;
        [WriteOnly]
        private NativeReference<float2> m_result;

        #region Interface properties
        public SplineProgress SplineProgress
        {
            get => m_splineProgress;
            set => m_splineProgress = value;
        }
        
        public float2 Result
        {
            get => m_result.Value;
            set => m_result.Value = value;
        }
        #endregion

        public CatmullRomSpline2DPointJob(ISpline2D spline, float progress, Allocator allocator = Allocator.None)
            : this(spline, new SplineProgress(progress), allocator) { }
        
        public CatmullRomSpline2DPointJob(ISpline2D spline, SplineProgress splineProgress, Allocator allocator = Allocator.None) 
            : this()
        {
            Spline = spline.SplineEntityData2D.Value;
            m_splineProgress = splineProgress;
            m_result = new NativeReference<float2>(allocator);
        }
        
        // 0.0 for the uniform spline, 0.5 for the centripetal spline, 1.0 for the chordal spline
        private const float c_alpha = 0.5f;

        public void Execute()
        {
            m_result.Value = Run(ref Spline, ref m_splineProgress);
        }

        public static float2 Run(ref Spline2DData spline, ref SplineProgress progress)
        {
#if UNITY_EDITOR && NO_BURST
            if(Spline.Points.Length == 0) throw new ArgumentException($"Should be using {nameof(Empty2DPointJob)}");
            if(Spline.Points.Length == 1) throw new ArgumentException($"Should be using {nameof(SinglePoint2DPointJob)}");
            if(Spline.Points.Length == 2) throw new ArgumentException($"Should be using {nameof(LinearSpline2DPointJob)}");
#endif

            if(progress.Progress <= 0f)
                return spline.Points[0];
            else if(progress.Progress >= 1f)
                return spline.Points[spline.Points.Length - 1];
            else
            {
                int aIndex = SplineHelperMethods.SegmentIndex(ref spline, ref progress);
                return SplineInterpolation(ref spline, SplineHelperMethods.SegmentProgress(ref spline, ref progress, aIndex), aIndex);
            }
        }

        private static float2 SplineInterpolation(ref Spline2DData spline, float progress, int a)
        {
            float2 p0, p1, p2, p3;
            // not looped
            if(spline.Points.Length == 3)
            {
                // 3 points require 2 of the points at the start and end to be fabricated
                if(progress == 0f) return spline.Points[a];
                if(progress == 1f) return spline.Points[(a + 1) % spline.Points.Length];

                p1 = spline.Points[a];
                p2 = spline.Points[(a + 1) % spline.Points.Length];

                float2 delta = p2 - p1;
                float angle = math.atan2(delta.y, delta.x) - (math.PI / 2);

                if(a == 0)
                {
                    // need to create a fake point for p0
                    p0 = new float2(p1.x + math.sin(angle), p1.y - math.cos(angle));
                    p3 = spline.Points[2];
                }
                else
                {
#if UNITY_EDITOR
                    Assert.AreEqual(1, a);
#endif

                    // need to create a fake point for p3
                    p0 = spline.Points[(a - 1) % spline.Points.Length];
                    p3 = new float2(p2.x + math.sin(-angle), p2.y + math.cos(-angle));
                }
            }
            else
            {
                if(a == 0)
                {
                    p1 = spline.Points[a];
                    p2 = spline.Points[(a + 1) % spline.Points.Length];
                    p3 = spline.Points[(a + 2) % spline.Points.Length];

                    float2 delta = p2 - p1;
                    float angle = math.atan2(delta.y, delta.x) - (math.PI / 2);
                    float size = math.max(math.length(delta) * 0.5f, float.Epsilon);
                    p0 = new float2(p1.x + (math.sin(angle) * size), p1.y - (math.cos(angle) * size));
                }
                else if(a == spline.Points.Length - 2)
                {
                    p1 = spline.Points[a];
                    if(progress <= 0f) return p1;

                    p2 = spline.Points[(a + 1) % spline.Points.Length];
                    if(progress >= 1f) return p2;

                    p0 = spline.Points[(a - 1) % spline.Points.Length];

                    float2 delta = p2 - p1;
                    float angle = math.atan2(delta.y, delta.x) - (math.PI / 2);
                    float size = math.distance(delta.x, delta.y) * 0.5f;
                    p3 = new float2(p2.x + (math.sin(-angle) * size), p2.y + (math.cos(-angle) * size));
                }
                else
                {
                    p0 = spline.Points[(a - 1) % spline.Points.Length];
                    p1 = spline.Points[a];
                    p2 = spline.Points[(a + 1) % spline.Points.Length];
                    p3 = spline.Points[(a + 2) % spline.Points.Length];
                }
            }

            const float t0 = 0.0f;
            float start = GetT(t0, p0, p1);
            float end = GetT(start, p1, p2);
            float t3 = GetT(end, p2, p3);
            float t = start + ((end - start) * progress);

            float2 a1 = (start - t) / (start - t0) * p0 + (t - t0) / (start - t0) * p1;
            float2 a2 = (end - t) / (end - start) * p1 + (t - start) / (end - start) * p2;
            float2 a3 = (t3 - t) / (t3 - end) * p2 + (t - end) / (t3 - end) * p3;

            float2 b1 = (end - t) / (end - t0) * a1 + (t - t0) / (end - t0) * a2;
            float2 b2 = (t3 - t) / (t3 - start) * a2 + (t - start) / (t3 - start) * a3;

            return (end - t) / (end - start) * b1 + (t - start) / (end - start) * b2;
        }

        private static float GetT(float t, float2 p0, float2 p1)
        {
            float a = math.pow((p1.x - p0.x), 2.0f) + math.pow((p1.y - p0.y), 2.0f);
            float b = math.pow(a, c_alpha * 0.5f);

            return (b + t);
        }

        public void Dispose()
        {
            m_result.Dispose();
        }

        public JobHandle Dispose(JobHandle inputDeps)
        {
            return m_result.Dispose(inputDeps);
        }
    }
}