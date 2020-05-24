using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Crener.Spline.BezierSpline.Entity;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

namespace Crener.Spline.PointToPoint
{
    /// <summary>
    /// Simple spline which directly follows a set of points
    /// </summary>
    public class PointToPoint2DSpline : MonoBehaviour, ISpline2D
    {
        [SerializeField]
        protected List<float2> Points = new List<float2>();
        [SerializeField]
        protected List<float> SegmentLength = new List<float>();

        // this is generated in editor and then reused in built versions.
        [SerializeField, HideInInspector]
        protected float LengthCache;

        /// <summary>
        /// Amount of control points in the spline
        /// </summary>
        public int ControlPointCount => Points.Count;

        /// <summary>
        /// Length of the spline
        /// </summary>
        public float Length() => LengthCache;

        public Spline2DData? SplineEntityData { get; private set; }
        public SplineType SplineDataType => SplineType.PointToPoint;

        /// <summary>
        /// Retrieve a point on the spline at a specific control point
        /// </summary>
        /// <returns>point on spline segment</returns>
        public float2 GetPoint(float progress, int index)
        {
            return math.lerp(Points[index], Points[(index + 1) % ControlPointCount], progress);
        }

        /// <summary>
        /// Relieve a point on the spline
        /// </summary>
        /// <param name="progress"></param>
        /// <returns>point on spline</returns>
        public float2 GetPoint(float progress)
        {
            if(progress == 0f || ControlPointCount <= 1)
                return Points[0];
            else if(progress >= 1f)
                return Points[ControlPointCount - 1];

            int aIndex = FindSegmentIndex(progress);
            float pointProgress = SegmentProgress(progress, aIndex);

            int bIndex = (aIndex + 1) % ControlPointCount;
            return math.lerp(Points[aIndex], Points[bIndex], pointProgress);
        }

        private float LengthBetweenPoints(int a, int b, int resolution = 64)
        {
            float currentLength = 0;

            float2 aPoint = math.lerp(Points[a], Points[b], 0f);
            for (float i = 1; i <= resolution; i++)
            {
                float2 bPoint = math.lerp(Points[a], Points[b], i / resolution);
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
            if(Points.Count == 0) Points.Add(point);
            else Points.Add(point);

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

            Points[index] = point;
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
                Points.Insert(0, point);
            }
            else
            {
                Points.Insert(index, point);
            }

            RecalculateLengthBias();
        }

        /// <summary>
        /// Remove existing control points data
        /// </summary>
        /// <param name="index">control point index</param>
        public void RemoveControlPoint(int index)
        {
            if(ControlPointCount == 0) return;

            Points.RemoveAt(index);
            RecalculateLengthBias();
        }

        public SplineEditMode GetEditMode(int index)
        {
            return SplineEditMode.Standard;
        }

        public void ChangeEditMode(int index, SplineEditMode mode)
        {
            // just needed for interface
        }

        /// <summary>
        /// Gets the given point from a point segment
        /// </summary>
        /// <param name="i">index of the segment</param>
        /// <returns>World Space position for the point</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float2 GetControlPoint(int i)
        {
            return Points[i];
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

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            return;
            
            //todo implement this for point to point
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