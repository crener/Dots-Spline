using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Code.Spline2.BezierSpline.Entity;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

namespace Code.Spline2.BezierSpline
{
    /// <summary>
    /// Standard 2D spline along the XY axis
    /// </summary>
    public class Spline2DSimple : MonoBehaviour, IConvertGameObjectToEntity, ISimpleSpline2D
    {
        private const int c_floatsPerControlPoint = 3;

        [SerializeField]
        protected List<float2> Points = new List<float2>();
        [SerializeField]
        protected List<SplineEditMode> PointEdit = new List<SplineEditMode>();
        [SerializeField]
        protected List<float> SegmentLength = new List<float>();
        [SerializeField, HideInInspector]
        protected float LengthCache;

        /// <summary>
        /// Amount of control points in the spline
        /// </summary>
        public int ControlPointCount => Points.Count == 0 ? 0 : (int) math.ceil(Points.Count / 3f);

        /// <summary>
        /// Length of the spline
        /// </summary>
        public float Length() => LengthCache;

        /// <summary>
        /// Retrieve a point on the spline at a specific control point
        /// </summary>
        /// <returns>point on spline segment</returns>
        public float2 GetPoint(float t, int index)
        {
            return CubicBezierPoint(t, index, (index + 1) % ControlPointCount);
        }

        /// <summary>
        /// Relieve a point on the spline
        /// </summary>
        /// <param name="progress"></param>
        /// <returns>point on spline</returns>
        public float2 GetPoint(float progress)
        {
            if(progress == 0f || ControlPointCount <= 1)
                return GetControlPoint(0, SplinePoint.Point);
            else if(progress >= 1f)
                return GetControlPoint(ControlPointCount - 1, SplinePoint.Point);

            int aIndex = FindSegmentIndex(progress);
            float pointProgress = SegmentProgress(progress, aIndex);

            int bIndex = (aIndex + 1) % ControlPointCount;
            return CubicBezierPoint(pointProgress, aIndex, bIndex);
        }

        private float LengthBetweenPoints(int a, int b, int resolution = 64)
        {
            float currentLength = 0;

            float2 aPoint = CubicBezierPoint(0f, a, b);
            for (float i = 1; i <= resolution; i++)
            {
                float2 bPoint = CubicBezierPoint(i / resolution, a, b);
                currentLength += math.distance(aPoint, bPoint);
                aPoint = bPoint;
            }

            return currentLength;
        }

        private int FindSegmentIndex(float progress)
        {
            int seg = SegmentLength.Count;
            for (int i = 0; i < seg; i++)
            {
                float time = SegmentLength[i];
                if(time >= progress) return i;
            }

            return 0;
        }

        /// <summary>
        /// Calculates the segment progress given the spline progress
        /// </summary>
        /// <param name="progress">spline progress</param>
        /// <returns>segment progress</returns>
        private float SegmentProgress(float progress)
        {
            return SegmentProgress(progress, FindSegmentIndex(progress));
        }

        /// <summary>
        /// Calculates the segment progress given the spline progress
        /// </summary>
        /// <param name="progress">spline progress</param>
        /// <param name="index">segment index</param>
        /// <returns>segment progress</returns>
        private float SegmentProgress(float progress, int index)
        {
            if(ControlPointCount <= 2) return progress;

            if(index == 0)
            {
                float segmentProgress = SegmentLength[0];

                return progress / segmentProgress;
            }

            float aLn = SegmentLength[index - 1];
            float bLn = SegmentLength[index];

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
            }
            else
            {
                float2 lastPoint = Points[Points.Count - 1];
                float2 delta = lastPoint - point;
                float angle = math.atan2(delta.y, delta.x) - (math.PI / 2);

                float2 lastPost = new float2(
                    lastPoint.x + math.sin(angle),
                    lastPoint.y + math.cos(angle));
                float2 newPre = new float2(
                    point.x + math.sin(-angle),
                    point.y + math.cos(-angle));

                Points.Add(lastPost);
                Points.Add(newPre);
                Points.Add(point);
            }

            PointEdit.Add(SplineEditMode.Standard);
            RecalculateLengthBias();
        }

        /// <summary>
        /// Update an existing control points data
        /// </summary>
        /// <param name="index">control point index</param>
        /// <param name="point">location of the point</param>
        /// <param name="mode">type of point to update</param>
        public void UpdateControlPoint(int index, float2 point, SplinePoint mode)
        {
            Assert.IsTrue(index <= ControlPointCount);

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
                Points.Insert(1, pre);
                Points.Insert(2, post);
            }
            else
            {
                int i = IndexMode(index, SplinePoint.Point);

                float2 lastPoint = Points[IndexMode(index - 1, SplinePoint.Point)];
                float2 delta = point - lastPoint;
                float angle = math.atan2(delta.x, delta.y);

                float2 nextPoint = Points[IndexMode(index, SplinePoint.Point)];
                delta = point - nextPoint;
                float angle2 = math.atan2(delta.x, delta.y);
                float superAngle = (angle + angle2 + math.PI) / 2f;

                float2 pre = new float2(
                    point.x - math.sin(superAngle),
                    point.y - math.cos(superAngle));
                float2 post = new float2(
                    point.x + math.sin(superAngle),
                    point.y + math.cos(superAngle));

                Points.Insert(i - 1, pre);
                Points.Insert(i, point);
                Points.Insert(i + 1, post);
            }

            PointEdit.Insert(index, SplineEditMode.Standard);
            RecalculateLengthBias();
        }

        /// <summary>
        /// Remove existing control points data
        /// </summary>
        /// <param name="index">control point index</param>
        public void RemoveControlPoint(int index)
        {
            if(ControlPointCount == 0) return;
            if(ControlPointCount == 1)
            {
                Points.RemoveAt(0);
                PointEdit.RemoveAt(0);
                return;
            }

            if(index == 0)
            {
                Points.RemoveRange(0, c_floatsPerControlPoint);
            }
            else if(index == ControlPointCount - 1)
            {
                int startIndex = math.max(0, IndexMode(ControlPointCount - 1, SplinePoint.Pre) - 1);
                Points.RemoveRange(startIndex, c_floatsPerControlPoint);
            }
            else
            {
                int startIndex = math.max(0, IndexMode(index, SplinePoint.Pre));
                int endIndex = math.max(0, IndexMode(index, SplinePoint.Post));
                Points.RemoveRange(startIndex, (endIndex - startIndex) + 1);
            }

            PointEdit.RemoveAt(index);
            RecalculateLengthBias();
        }


        /// <summary>
        /// Get the edit mode for a control point 
        /// </summary>
        /// <param name="index"> control point index</param>
        /// <returns>edit mode for the control point</returns>
        public SplineEditMode GetEditMode(int index)
        {
            return PointEdit[index];
        }

        /// <summary>
        /// Change the edit mode of a control point
        /// </summary>
        /// <param name="index">control point index</param>
        /// <param name="mode">new control point edit mode</param>
        public void ChangeEditMode(int index, SplineEditMode mode)
        {
            PointEdit[index] = mode;
        }

        /// <summary>
        /// Gets the given point from a point segment
        /// </summary>
        /// <param name="i">index of the segment</param>
        /// <param name="point">type of point to get</param>
        /// <returns>World Space position for the point</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float2 GetControlPoint(int i, SplinePoint point)
        {
            int index = IndexMode(i, point);
            return Points[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int IndexMode(int i, SplinePoint point)
        {
            switch (point)
            {
                case SplinePoint.Point:
                    return (i * c_floatsPerControlPoint);
                case SplinePoint.Post:
                    return (i * c_floatsPerControlPoint) + 1;
                case SplinePoint.Pre:
                    return (i * c_floatsPerControlPoint) - 1;
            }

            throw new ArgumentException("Unexpected enum value", nameof(point));
        }

        private void RecalculateLengthBias()
        {
            ClearData();

            if(ControlPointCount <= 1)
            {
                LengthCache = 0f;
                return;
            }

            float currentLength = 0f;

            for (int a = 0; a < ControlPointCount - 1; a++)
            {
                int b = (a + 1) % ControlPointCount;
                float length = LengthBetweenPoints(a, b);

                currentLength += length;
            }

            LengthCache = currentLength;

            SegmentLength.Clear();
            if(ControlPointCount == 2)
            {
                SegmentLength.Add(1f);
                return;
            }

            float segmentCount = 0f;
            for (int a = 0; a < ControlPointCount - 1; a++)
            {
                int b = (a + 1) % ControlPointCount;
                float length = LengthBetweenPoints(a, b);

                segmentCount = (length / LengthCache) + segmentCount;
                SegmentLength.Add(segmentCount);
            }
        }

        public void ClearData()
        {
            if(SplineEntityData.HasValue)
            {
                SplineEntityData.Value.Dispose();
                SplineEntityData = null;
            }
        }

        private float2 CubicBezierPoint(float t, int a, int b)
        {
            float2 p0 = GetControlPoint(a, SplinePoint.Point);
            float2 p1 = GetControlPoint(a, SplinePoint.Post);
            float2 p2 = GetControlPoint(b, SplinePoint.Pre);
            float2 p3 = GetControlPoint(b, SplinePoint.Point);

            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;
            return
                (oneMinusT * oneMinusT * oneMinusT * p0) +
                (3f * oneMinusT * oneMinusT * t * p1) +
                (3f * oneMinusT * t * t * p2) +
                (t * t * t * p3);
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
            Gizmos.color = Color.gray;
            const float pointDensity = 13;

            for (int i = 0; i < ControlPointCount - 1; i++)
            {
                float2 f = GetPoint(0f, i);
                Vector3 lp = new Vector3(f.x, f.y, 0f);
                int points = (int) (pointDensity * (SegmentLength[i] * Length()));

                for (int s = 0; s <= points; s++)
                {
                    float progress = s / (float) points;
                    float2 p = GetPoint(progress, i);
                    Vector3 point = new Vector3(p.x, p.y, 0f);

                    Gizmos.DrawLine(lp, point);
                    lp = point;
                }
            }
        }

        public Spline2DData? SplineEntityData { get; private set; }

        public void Convert(Unity.Entities.Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<Spline2DData>(entity);
            Spline2DData splineData = ConvertData();
            SplineEntityData = splineData;
            dstManager.SetSharedComponentData(entity, splineData);
        }

        protected Spline2DData ConvertData()
        {
            NativeArray<float2> points = new NativeArray<float2>(Points.ToArray(), Allocator.Persistent);
            NativeArray<float> time = new NativeArray<float>(SegmentLength.ToArray(), Allocator.Persistent);

            SplineEntityData = new Spline2DData
            {
                Length = Length(),
                Points = points,
                Time = time
            };

            return SplineEntityData.Value;
        }
    }
}