using System;
using Code.Spline2.BezierSpline.Entity;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Code.Spline2.BezierSpline.Jobs
{
    /// <summary>
    /// Simple way of sampling a single point from a spline with a given variance from the center line
    /// as allowed by the specific spline setup.
    /// </summary>
    /// <remarks>
    /// If no variance is required <see cref="Spline2DPointJob"/> should be used with the accompanying simple spline setup in scene.
    /// </remarks>
    [BurstCompile]
    public struct Spline2DVariancePointJob : IJob
    {
        [ReadOnly]
        public Spline2DVarianceData Spline;
        [ReadOnly]
        public SplineProgress SplineProgress;
        [ReadOnly]
        public SplineVariance SplineVariance;
        [WriteOnly]
        public float2 Result;

        public void Execute()
        {
            if(Spline.ControlPointCount == 0)
            {
                Result = float2.zero;
                return;
            }

            if(Spline.ControlPointCount == 1)
            {
                Result = Spline.Points[0];
                return;
            }

            float progress = math.clamp(SplineProgress.Progress, 0f, 1f);
            int side = VarianceToSide(SplineVariance.Variance);
            
            int aIndex = FindSegmentIndex(progress, 0);
            int bIndex = FindSegmentIndex(progress, side);

            float2 splineProgress = new float2(
                SegmentProgress(progress, aIndex, 0),
                SegmentProgress(progress, bIndex, side));
            
#if UNITY_EDITOR
            if(splineProgress.x < 0f || splineProgress.x > 1f)
                throw new IndexOutOfRangeException($"{nameof(splineProgress)}.x out of range: {splineProgress.x}");
            if(splineProgress.y < 0f || splineProgress.y > 1f)
                throw new IndexOutOfRangeException($"{nameof(splineProgress)}.y out of range: {splineProgress.y}");
#endif

            Result = CubicBezierPoint(splineProgress, aIndex, bIndex, SplineVariance.Variance);
        }

        /// <summary>
        /// Find the segment index of the given <see cref="side"/> at <see cref="progress"/> of the entire spline
        /// </summary>
        /// <param name="progress">range from 0 to 1 of spline completion</param>
        /// <param name="side">which spline to use <seealso cref="Spline2DVariance.SplineSide"/></param>
        /// <returns>segment index for <see cref="side"/> at according to total spline <see cref="progress"/></returns>
        private int FindSegmentIndex(float progress, int side)
        {
            int segCount = Spline.Time.Length == 0 ? 0 : Spline.Time.Length / 3;
            for (int i = 0; i < segCount; i++)
            {
                float time = Spline.Time[i * 3 + side];
                if(time >= progress) return i;
            }

            return 0;
        }

        /// <summary>
        /// Whats the segment progress for the spline progress 
        /// </summary>
        /// <param name="progress">progress for entire spline</param>
        /// <param name="index">index of spline segment</param>
        /// <param name="side">which spline to use <seealso cref="Spline2DVariance.SplineSide"/></param>
        /// <returns>progress through spline segment</returns>
        private float SegmentProgress(float progress, int index, int side)
        {
            if(index == 0)
            {
                float segmentProgress = Spline.Time[index * 3 + side];
                return progress / segmentProgress;
            }

            float aLn = Spline.Time[(index - 1) * 3 + side];
            float bLn = Spline.Time[index * 3 + side];

            return (progress - aLn) / (bLn - aLn);
        }
        
        /// <summary>
        /// Method that actually calculates the bezier spline with variance
        /// </summary>
        /// <param name="t">progress X for center spline, Y for side spline</param>
        /// <param name="spline1">center spline index</param>
        /// <param name="spline2">side spline index</param>
        /// <param name="variance">deviation from center</param>
        /// <returns>final cubic variance position</returns>
        private float2 CubicBezierPoint(float2 t, int spline1, int spline2, half variance)
        {
            //center spline
            float2x4 a = new float2x4(
                Spline.Points[spline1 * 9 + 0],
                Spline.Points[spline1 * 9 + 3],
                Spline.Points[(spline1 + 1) * 9 - 3],
                Spline.Points[(spline1 + 1) * 9 + 0]);

            float2x4 b;
            if(variance > 0)
            {
                // Right spline
                b = new float2x4(
                    Spline.Points[spline2 * 9 + 2],
                    Spline.Points[spline2 * 9 + 5],
                    Spline.Points[(spline2 + 1) * 9 - 1],
                    Spline.Points[(spline2 + 1) * 9 + 2]); 
            }
            else
            {
                // Left spline
                b = new float2x4(
                    Spline.Points[spline2 * 9 + 1],
                    Spline.Points[spline2 * 9 + 4],
                    Spline.Points[(spline2 + 1) * 9 - 2],
                    Spline.Points[(spline2 + 1) * 9 + 1]);
            }
            
            float2 oneMinusT = 1f - t;
            return math.lerp(
                (oneMinusT.x * oneMinusT.x * oneMinusT.x * a.c0) +
                (3f * oneMinusT.x * oneMinusT.x * t.x * a.c1) +
                (3f * oneMinusT.x * t.x * t.x * a.c2) +
                (t.x * t.x * t.x * a.c3),
                (oneMinusT.y * oneMinusT.y * oneMinusT.y * b.c0) +
                (3f * oneMinusT.y * oneMinusT.y * t.y * b.c1) +
                (3f * oneMinusT.y * t.y * t.y * b.c2) +
                (t.y * t.y * t.y * b.c3), math.abs(variance));
        }

        /// <summary>
        /// Conversion of variance to a side int
        /// </summary>
        /// <remarks>This is essentially performing the same function as the <see cref="Spline2DVariance.SplineSide"/> enum</remarks>
        /// <param name="variance">variance to convert</param>
        /// <returns>int code for the spline</returns>
        private static int VarianceToSide(half variance)
        {
            if(variance > 0) return 2; // Right
            if(variance < 0) return 1; // Left
            return 0; // Center
        }
    }
}