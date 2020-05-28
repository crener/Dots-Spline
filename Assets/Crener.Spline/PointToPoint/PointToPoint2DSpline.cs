using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Crener.Spline.BezierSpline.Entity;
using Crener.Spline.Common;
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
    public class PointToPoint2DSpline : BaseSpline2D
    {
        public override SplineType SplineDataType => SplineType.PointToPoint;
        
        /// <summary>
        /// Adds a point to the end of the spline
        /// </summary>
        public override void AddControlPoint(float2 point)
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
        public override void InsertControlPoint(int index, float2 point)
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
        public override void RemoveControlPoint(int index)
        {
            if(ControlPointCount == 0) return;

            Points.RemoveAt(index);
            RecalculateLengthBias();
        }

        public override SplineEditMode GetEditMode(int index)
        {
            return SplineEditMode.Standard;
        }

        public override void ChangeEditMode(int index, SplineEditMode mode)
        {
            // just needed for interface
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override float2 SplineInterpolation(float t, int a, int b)
        {
            return math.lerp(Points[a], Points[b], t);
        }

        /// <summary>
        /// Gets the given point from a point segment
        /// </summary>
        /// <param name="i">index of the segment</param>
        /// <returns>World Space position for the point</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override float2 GetControlPoint(int i)
        {
            return Points[i];
        }

        public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            return;
            
            //todo implement this for point to point
            dstManager.AddComponent<Spline2DData>(entity);
            Spline2DData splineData = ConvertData();
            SplineEntityData = splineData;
            dstManager.SetSharedComponentData(entity, splineData);
        }

        protected override Spline2DData ConvertData()
        {
            ClearData();
            NativeArray<float2> points = new NativeArray<float2>(Points.ToArray(), Allocator.Persistent);
            NativeArray<float> time = new NativeArray<float>(SegmentLength.ToArray(), Allocator.Persistent);

            Assert.IsFalse(hasSplineEntityData);
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