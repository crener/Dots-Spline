using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Crener.Spline.BezierSpline.Entity;
using Crener.Spline.Common;
using Crener.Spline.Common.DataStructs;
using Crener.Spline.Common.Interfaces;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace Crener.Spline.BezierSpline
{
    /// <summary>
    /// Spline along the XY axis with variance as defined when edited in scene
    /// </summary>
    [AddComponentMenu("Spline/2D/Variance/Bezier Spline")]
    public class BezierSpline2DVariance : MonoBehaviour, ISimpleSpline2DVariance
    {
        private static readonly Color s_handleLeft = new Color(0.06f, 0.48f, 0f);
        private static readonly Color s_handleRight = new Color(0.48f, 0.13f, 0f);
        protected const int FloatsPerControlPoint = 9;

        [SerializeField]
        protected List<float2> Points = new List<float2>();
        [SerializeField, FormerlySerializedAs("PointEdit")]
        protected List<SplineEditMode> PointMode = new List<SplineEditMode>();
        [SerializeField]
        protected float[,] SegmentLength = new float[0, 0];
        [SerializeField, HideInInspector]
        protected float[] LengthCache;

        public int ControlPointCount => Points.Count == 0 ? 0 : (int) math.ceil(Points.Count / 9f);
        public int SegmentPointCount => ControlPointCount;
        public float Length() => LengthCache[(int) SplineSide.Center];

        public float Length(half variance)
        {
            float center = LengthCache[(int) SplineSide.Center];

            if(variance == 0f) return center;
            float other = LengthCache[(int) (variance > 0 ? SplineSide.Right : SplineSide.Left)];

            return math.lerp(center, other, math.abs(variance));
        }

        public Spline2DVarianceData? SplineVarianceEntityData { get; private set; }

        Spline2DData? ISpline2D.SplineEntityData2D
        {
            get
            {
                if(SplineVarianceEntityData.HasValue)
                {
                    return Spline2DVarianceData.CenterSplineData(SplineVarianceEntityData.Value, Allocator.Persistent);
                }

                return null;
            }
        }

        public SplineType SplineDataType => SplineType.Bezier;

        private void Awake()
        {
            if(SegmentLength.Length == 0)
            {
                // work around for Unity's lack inability to save nested/multi-dimensional arrays
                RecalculateLengthBias();
            }
        }

        public float2 Get2DPoint(float progress)
        {
            return GetPoint(progress, half.zero);
        }

        public float2 Get2DPoint(float progress, int index)
        {
            return GetPoint(progress, index, half.zero);
        }

        /// <summary>
        /// Retrieve a point on the spline with a certain amount of variance
        /// </summary>
        /// <param name="progress">0 to 1 range of progress along the spline</param>
        /// <param name="variance">-1 to 1 range of variation from the center spline</param>
        /// <returns>point on spline</returns>
        public float2 GetPoint(float progress, half variance)
        {
            if(ControlPointCount == 0)
                return float2.zero;
            if(ControlPointCount <= 1)
                return GetControlPoint(0, SplinePointVariance.Point);
            progress = math.clamp(progress, 0f, 1f);

            int aIndex = FindSegmentIndex(progress);
            float centerProgress = SegmentProgress(progress, aIndex, SplineSide.Center);
#if UNITY_EDITOR
            if(centerProgress < 0f || centerProgress > 1f)
                throw new IndexOutOfRangeException($"{nameof(centerProgress)} out of range: {centerProgress}");
#endif

            SplineSide side = VarianceToSide(variance);
            int bIndex = FindSegmentIndex(progress, side);
            float sideProgress = SegmentProgress(progress, bIndex, side);
#if UNITY_EDITOR
            if(sideProgress < 0f || sideProgress > 1f)
                throw new IndexOutOfRangeException($"{nameof(sideProgress)} out of range: {sideProgress}");
#endif

            return CubicBezierPoint(centerProgress, sideProgress, aIndex, bIndex, variance);
        }

        /// <summary>
        /// Calculates the length of a spline segment using the provided <param name="resolution"></param> 
        /// </summary>
        /// <param name="a">point segment start index</param>
        /// <param name="b">point segment end index</param>
        /// <param name="resolution">how many points to use to sample the line</param>
        /// <param name="side"></param>
        /// <returns>length between points at given precision</returns>
        private float LengthBetweenPoints(int a, int b, int resolution = 300, SplineSide side = 0)
        {
            float currentLength = 0;

            half variance = SideToVariance(side);
            float2 aPoint = CubicBezierPoint(0f, a, b, variance);
            for (float i = 1; i <= resolution; i++)
            {
                float2 bPoint = CubicBezierPoint(i / resolution, a, b, variance);
                currentLength += math.distance(aPoint, bPoint);
                aPoint = bPoint;
            }

            return currentLength;
        }

        /// <summary>
        /// Returns the segment index of the given spline <param name="side"/> 
        /// </summary>
        private int FindSegmentIndex(float progress, SplineSide side = SplineSide.Center)
        {
            int segCount = SegmentLength.GetLength(0) == 0 ? 0 : SegmentLength.GetLength(1);
            for (int i = 0; i < segCount; i++)
            {
                float time = SegmentLength[(int) side, i];
                if(time >= progress) return i;
            }

            return SegmentLength.Length - 1;
        }

        /// <summary>
        /// Calculates the segment progress given the spline progress
        /// </summary>
        /// <param name="progress">spline progress</param>
        /// <param name="index">segment index</param>
        /// <param name="side">the side to get progress for</param>
        /// <returns>segment progress</returns>
        private float SegmentProgress(float progress, int index, SplineSide side)
        {
            if(index == 0)
            {
                float segmentProgress = SegmentLength[(int) side, 0];
                return progress / segmentProgress;
            }

            float aLn = SegmentLength[(int) side, index - 1];
            float bLn = SegmentLength[(int) side, index];

            return (progress - aLn) / (bLn - aLn);
        }

        /// <summary>
        /// Adds a point to the end of the spline
        /// </summary>
        public void AddControlPoint(float2 point)
        {
            if(Points.Count == 0)
            {
                Points.Add(point);
                Points.Add(point); // point left
                Points.Add(point); // point right
            }
            else
            {
                int index = ControlPointCount - 1;

                float2 postCenter, preCenter;
                GeneratePrePost(IndexMode(index, SplinePointVariance.Point), out postCenter, out preCenter, point);
                float2 postLeft, preLeft;
                GeneratePrePost(IndexMode(index, SplinePointVariance.PointLeft), out postLeft, out preLeft, point);
                float2 postRight, preRight;
                GeneratePrePost(IndexMode(index, SplinePointVariance.PointRight), out postRight, out preRight, point);

                Points.Add(postCenter);
                Points.Add(postLeft);
                Points.Add(postRight);

                Points.Add(preCenter);
                Points.Add(preLeft);
                Points.Add(preRight);

                Points.Add(point);
                Points.Add(point);
                Points.Add(point);
            }

            PointMode.Add(SplineEditMode.Standard);
            RecalculateLengthBias();
        }

        private void GeneratePrePost(int existingIndex, out float2 lastPost, out float2 newPre, float2 point)
        {
            float2 lastPoint = Points[existingIndex];
            float2 delta = lastPoint - point;
            float angle = math.atan2(delta.y, delta.x) - (math.PI / 2);

            lastPost = new float2(
                lastPoint.x + math.sin(angle),
                lastPoint.y - math.cos(angle));
            newPre = new float2(
                point.x + math.sin(-angle),
                point.y + math.cos(-angle));
        }

        public void UpdateControlPoint(int index, float2 point, SplinePointVariance mode)
        {
#if UNITY_EDITOR
            Assert.IsTrue(index <= ControlPointCount);
#endif

            int i = IndexMode(index, mode);
            Points[i] = point;

            RecalculateLengthBias();
        }

        /// <summary>
        /// inserts a point before the specified segment index
        /// </summary>
        /// <param name="index">segment index</param>
        /// <param name="point">location to insert</param>
        public void InsertControlPoint(int index, float2 point)
        {
            if(Points.Count <= 1 || index >= ControlPointCount)
            {
                // add as there aren't enough points to insert between
                AddControlPoint(point);
                return;
            }

            if(index == 0)
            {
                // replace the first node
                float2 pre = math.lerp(point, Points[0], 0.3f);
                float2 post = math.lerp(Points[0], point, 0.3f);

                Points.Insert(0, point);
                Points.Insert(1, point); // point left
                Points.Insert(2, point); // point right

                Points.Insert(3, post);
                Points.Insert(4, post); // post left
                Points.Insert(5, post); // post right

                Points.Insert(3, pre);
                Points.Insert(4, pre); // pre left
                Points.Insert(5, pre); // pre right
            }
            else
            {
                int i = IndexMode(index, SplinePointVariance.Point);

                float2 lastPoint = Points[IndexMode(index - 1, SplinePointVariance.Point)];
                float2 delta = point - lastPoint;
                float angle = math.atan2(delta.x, delta.y);

                float2 nextPoint = Points[IndexMode(index, SplinePointVariance.Point)];
                delta = point - nextPoint;
                float angle2 = math.atan2(delta.x, delta.y);
                float superAngle = (angle + angle2 + math.PI) / 2f;

                float2 pre = new float2(
                    point.x - math.sin(superAngle),
                    point.y - math.cos(superAngle));
                float2 post = new float2(
                    point.x + math.sin(superAngle),
                    point.y + math.cos(superAngle));

                // calculate the magnitude between the points to get an averaged 
                float2 leftDelta, rightDelta;
                {
                    float2 lastLeft = Points[IndexMode(index - 1, SplinePointVariance.PointLeft)];
                    float2 lastRight = Points[IndexMode(index - 1, SplinePointVariance.PointRight)];
                    float magLastLeft = math.abs(math.length(lastPoint - lastLeft));
                    float magLastRight = math.abs(math.length(lastPoint - lastRight));

                    float2 nextLeft = Points[IndexMode(index, SplinePointVariance.PointLeft)];
                    float2 nextRight = Points[IndexMode(index, SplinePointVariance.PointRight)];
                    float magNextLeft = math.abs(math.length(nextPoint - nextLeft));
                    float magNextRight = math.abs(math.length(nextPoint - nextRight));

                    float left = (magLastLeft + magNextLeft) / 2;
                    float right = (magLastRight + magNextRight) / 2;
                    float parallelAngle = superAngle + (math.PI / 2);

                    leftDelta = new float2(
                        math.sin(parallelAngle) * left,
                        math.cos(parallelAngle) * left);
                    rightDelta = new float2(
                        math.sin(parallelAngle + math.PI) * right,
                        math.cos(parallelAngle + math.PI) * right);
                }

                Points.Insert(i - 3, pre); // pre
                Points.Insert(i - 2, pre + leftDelta); // pre left
                Points.Insert(i - 1, pre + rightDelta); // pre right

                Points.Insert(i, point); // point
                Points.Insert(i + 1, point + leftDelta); // point left
                Points.Insert(i + 2, point + rightDelta); // point right

                Points.Insert(i + 3, post); // post
                Points.Insert(i + 4, post + leftDelta); // post left
                Points.Insert(i + 5, post + rightDelta); // post right
            }

            PointMode.Insert(index, SplineEditMode.Standard);
            RecalculateLengthBias();
        }

        public void RemoveControlPoint(int index)
        {
            if(ControlPointCount == 0 || index < 0) return;
            if(ControlPointCount == 1)
            {
                Points.Clear();
                PointMode.Clear();
                SegmentLength = new float[0, 0];
                return;
            }

            if(index == 0)
            {
                // remove from start
                Points.RemoveRange(0, FloatsPerControlPoint);
            }
            else if(index >= ControlPointCount - 1)
            {
                // fixes the index for later if it is greater than the amount of control points
                index = ControlPointCount - 1;
                
                // remove from end
                int startIndex = math.max(0, IndexMode(index-1, SplinePointVariance.Post));
                Points.RemoveRange(startIndex, Points.Count - startIndex);
            }
            else
            {
                // remove from middle
                int startIndex = math.max(index - 1, IndexMode(index, SplinePointVariance.Pre));
                int endIndex = math.max(index + 1, IndexMode(index, SplinePointVariance.PostRight));
                Points.RemoveRange(startIndex, (endIndex - startIndex) + 1);
            }

            PointMode.RemoveAt(index);
            RecalculateLengthBias();
        }

        /// <summary>
        /// Gets the given point from a point segment
        /// </summary>
        /// <param name="i">index of the segment</param>
        /// <param name="point">type of point to get</param>
        /// <returns>World Space position for the point</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual float2 GetControlPoint(int i, SplinePointVariance point)
        {
            int index = IndexMode(i, point);
#if UNITY_EDITOR
            if(index > Points.Count || index < 0)
            {
                throw new IndexOutOfRangeException($"Index '{index}' can't be retrieved from an array with {Points.Count} elements!");
            }
#endif
            return Points[index];
        }

        /// <summary>
        /// Helper method for converting a control point index and point type to an index location within the <see cref="Points"/> array
        /// for accessing data in a simpler manner 
        /// </summary>
        /// <param name="i">index of control point</param>
        /// <param name="point">type of point to return</param>
        /// <returns>index of point inside <see cref="Points"/> array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int IndexMode(int i, SplinePointVariance point)
        {
            switch (point)
            {
                case SplinePointVariance.Pre:
                    return (i * FloatsPerControlPoint) - 3;
                case SplinePointVariance.PreLeft:
                    return (i * FloatsPerControlPoint) - 2;
                case SplinePointVariance.PreRight:
                    return (i * FloatsPerControlPoint) - 1;

                case SplinePointVariance.Point:
                    return (i * FloatsPerControlPoint);
                case SplinePointVariance.PointLeft:
                    return (i * FloatsPerControlPoint) + 1;
                case SplinePointVariance.PointRight:
                    return (i * FloatsPerControlPoint) + 2;

                case SplinePointVariance.Post:
                    return (i * FloatsPerControlPoint) + 3;
                case SplinePointVariance.PostLeft:
                    return (i * FloatsPerControlPoint) + 4;
                case SplinePointVariance.PostRight:
                    return (i * FloatsPerControlPoint) + 5;
            }

            throw new ArgumentException("Unexpected enum value", nameof(point));
        }

        /// <summary>
        /// Calculates the distance that each spline covers to a given progress percentage
        /// </summary>
        protected void RecalculateLengthBias()
        {
            ClearData();

            if(ControlPointCount <= 1)
            {
                LengthCache = new float[3];
                return;
            }

            // work out the length of each side
            LengthCache = new[]
            {
                CalculateLength(SplineSide.Center),
                CalculateLength(SplineSide.Left),
                CalculateLength(SplineSide.Right),
            };

            // early out if there aren't enough points to make the calculation worth doing
            if(ControlPointCount == 2)
            {
                SegmentLength = new float[,]
                {
                    {1f}, // Center
                    {1f}, // Left
                    {1f} // Right
                };
                return;
            }

            // work out the distances that each segment is responsible for
            float[] center = CalculateLengthBiasSide(SplineSide.Center);
            float[] left = CalculateLengthBiasSide(SplineSide.Left);
            float[] right = CalculateLengthBiasSide(SplineSide.Right);
            SegmentLength = new float[3, center.Length];

            for (int i = 0; i < center.Length; i++)
            {
                SegmentLength[0, i] = center[i];
                SegmentLength[1, i] = left[i];
                SegmentLength[2, i] = right[i];
            }
        }

        /// <summary>
        /// Extracts a 0 to 1 range of distance covered in respect to the total spline length
        /// </summary>
        /// <param name="side">which side spline to calculate</param>
        private float[] CalculateLengthBiasSide(SplineSide side)
        {
            List<float> segmentLengths = new List<float>();
            half v = SideToVariance(side);

            // this will convert point lengths to the relative distance that a point covers in relation to
            // the total length of the spline. so first point will always be 0 and last point will always be 1
            
            float segmentTally = 0f;
            float sideLength = Length(v);
            for (int a = 0; a < ControlPointCount - 1; a++)
            {
                int b = (a + 1) % ControlPointCount;
                float length = LengthBetweenPoints(a, b, side: side);

                segmentTally += length;
                float segmentLength = segmentTally / sideLength;
                segmentLengths.Add(segmentLength);
            }

#if UNITY_EDITOR
            // if this fails it is an issue and causes snapping/jumping at places where spline segments meet
            float totalLength = segmentLengths[segmentLengths.Count - 1];
            Assert.IsTrue(Math.Abs(totalLength - 1f) < 0.00001f,
                $"Last length point not equal to 1! Lerping using this data will be inaccurate. Actual Point: {totalLength:N}");
#endif

            return segmentLengths.ToArray();
        }

        private float CalculateLength(SplineSide side)
        {
            float currentLength = 0f;
            for (int a = 0; a < ControlPointCount - 1; a++)
            {
                int b = (a + 1) % ControlPointCount;
                float length = LengthBetweenPoints(a, b, side: side);

                currentLength += length;
            }

            return currentLength;
        }

        public void ClearData()
        {
            if(SplineVarianceEntityData.HasValue)
            {
                SplineVarianceEntityData.Value.Dispose();
                SplineVarianceEntityData = null;
            }
        }

        public void ClearControlPoints()
        {
            Points.Clear();
            ClearData();
            RecalculateLengthBias();
        }

        private float2 CubicBezierPoint(float t, float t2, int spline1, int spline2, half variance)
        {
            float2 center = CubicBezierPoint(t, spline1, (spline1 + 1) % ControlPointCount);
            if(variance == 0f) return center;

            float2 other;
            if(variance > 0) other = CubicBezierPointRight(t2, spline2, (spline2 + 1) % ControlPointCount);
            else other = CubicBezierPointLeft(t2, spline2, (spline2 + 1) % ControlPointCount);

            return math.lerp(center, other, math.abs(variance));
        }

        private float2 CubicBezierPoint(float t, int a, int b, half variance)
        {
            float2 center = CubicBezierPoint(t, a, b);
            if(variance == 0f) return center;

            float2 other;
            if(variance > 0) other = CubicBezierPointRight(t, a, b);
            else other = CubicBezierPointLeft(t, a, b);

            return math.lerp(center, other, math.abs(variance));
        }

        private float2 CubicBezierPoint(float t, int a, int b)
        {
            float2 p0 = GetControlPoint(a, SplinePointVariance.Point);
            float2 p1 = GetControlPoint(a, SplinePointVariance.Post);
            float2 p2 = GetControlPoint(b, SplinePointVariance.Pre);
            float2 p3 = GetControlPoint(b, SplinePointVariance.Point);

            return BezierMath.CubicBezierPoint(t, p0, p1, p2, p3);
        }

        private float2 CubicBezierPointLeft(float t, int a, int b)
        {
            float2 p0 = GetControlPoint(a, SplinePointVariance.PointLeft);
            float2 p1 = GetControlPoint(a, SplinePointVariance.PostLeft);
            float2 p2 = GetControlPoint(b, SplinePointVariance.PreLeft);
            float2 p3 = GetControlPoint(b, SplinePointVariance.PointLeft);

            return BezierMath.CubicBezierPoint(t, p0, p1, p2, p3);
        }

        private float2 CubicBezierPointRight(float t, int a, int b)
        {
            float2 p0 = GetControlPoint(a, SplinePointVariance.PointRight);
            float2 p1 = GetControlPoint(a, SplinePointVariance.PostRight);
            float2 p2 = GetControlPoint(b, SplinePointVariance.PreRight);
            float2 p3 = GetControlPoint(b, SplinePointVariance.PointRight);

            return BezierMath.CubicBezierPoint(t, p0, p1, p2, p3);
        }
        
        public void Dispose()
        {
            ClearData();
        }

        private void OnDestroy()
        {
            ClearData();
        }

        private void Reset()
        {
            Points.Clear();

            Vector3 position = transform.position;
            AddControlPoint(new float2(position.x - 2f, position.y));
            AddControlPoint(new float2(position.x + 2f, position.y));
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = s_handleLeft;
            DrawSpline(new half(-1f));

            Gizmos.color = s_handleRight;
            DrawSpline(new half(1f));

            Gizmos.color = Color.black;
            DrawSpline(new half(0));
        }

        private void DrawSpline(half variance)
        {
            const float pointDensity = 13;
            const int maxPoints = ((int) pointDensity) * 300;

            if(SegmentLength.Length == 0) RecalculateLengthBias();

            for (int i = 0; i < ControlPointCount - 1; i++)
            {
                float2 f = GetPoint(0f, i, variance);
                Vector3 lp = new Vector3(f.x, f.y, 0f);
                int points = (int) (pointDensity * (SegmentLength[(int) SplineSide.Center, i] * Length(variance)));
                if(points > maxPoints) points = maxPoints;

                for (int s = 0; s <= points; s++)
                {
                    float progress = s / (float) points;
                    float2 p = GetPoint(progress, i, variance);
                    Vector3 point = new Vector3(p.x, p.y, 0f);

                    Gizmos.DrawLine(lp, point);
                    lp = point;
                }
            }
        }


        /// <summary>
        /// Get the edit mode for a control point 
        /// </summary>
        /// <param name="index"> control point index</param>
        /// <returns>edit mode for the control point</returns>
        public SplineEditMode GetEditMode(int index)
        {
            return PointMode[index];
        }

        /// <summary>
        /// Change the edit mode of a control point
        /// </summary>
        /// <param name="index">control point index</param>
        /// <param name="mode">new control point edit mode</param>
        public void ChangeEditMode(int index, SplineEditMode mode)
        {
            PointMode[index] = mode;
        }


        public float2 GetPoint(float progress, int index, half variance)
        {
            return CubicBezierPoint(progress, index, (index + 1) % ControlPointCount, variance);
        }

        private static half SideToVariance(SplineSide side)
        {
            if(side == SplineSide.Left) return new half(-1);
            if(side == SplineSide.Center) return half.zero;
            if(side == SplineSide.Right) return new half(1);
            return half.zero;
        }

        private static SplineSide VarianceToSide(half variance)
        {
            if(variance > 0) return SplineSide.Right;
            if(variance < 0) return SplineSide.Left;
            return SplineSide.Center;
        }

        public void Convert(Unity.Entities.Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<Spline2DVarianceData>(entity);
            Spline2DVarianceData splineData = ConvertSplineData();
            dstManager.SetSharedComponentData(entity, splineData);
        }

        public Spline2DVarianceData ConvertSplineData()
        {
            NativeArray<float> length = new NativeArray<float>(LengthCache, Allocator.Persistent);
            NativeArray<float> time = new NativeArray<float>(SegmentLength.Length, Allocator.Persistent);
            {
                int index = 0;
                for (int n = 0; n < SegmentLength.GetLength(1); n++)
                {
                    time[index++] = SegmentLength[0, n];
                    time[index++] = SegmentLength[1, n];
                    time[index++] = SegmentLength[2, n];
                }
            }

            NativeArray<float2> points = new NativeArray<float2>(Points.Count, Allocator.Persistent);
            {
                // By iterating over everything using GetControlPoint to get the point data classes can inherit and
                // modify control points without hacking the base logic
                int current = 0;
                for (int cp = 0; cp < ControlPointCount; cp++)
                {
                    if(cp > 0)
                    {
                        points[current++] = GetControlPoint(cp, SplinePointVariance.Pre);
                        points[current++] = GetControlPoint(cp, SplinePointVariance.PreLeft);
                        points[current++] = GetControlPoint(cp, SplinePointVariance.PreRight);
                    }

                    points[current++] = GetControlPoint(cp, SplinePointVariance.Point);
                    points[current++] = GetControlPoint(cp, SplinePointVariance.PointLeft);
                    points[current++] = GetControlPoint(cp, SplinePointVariance.PointRight);

                    if(cp < ControlPointCount - 1)
                    {
                        points[current++] = GetControlPoint(cp, SplinePointVariance.Post);
                        points[current++] = GetControlPoint(cp, SplinePointVariance.PostLeft);
                        points[current++] = GetControlPoint(cp, SplinePointVariance.PostRight);
                    }
                }
            }

            SplineVarianceEntityData = new Spline2DVarianceData
            {
                Length = length,
                Time = time,
                Points = points,
                ControlPointCount = ControlPointCount,
            };

            return SplineVarianceEntityData.Value;
        }
    }
}