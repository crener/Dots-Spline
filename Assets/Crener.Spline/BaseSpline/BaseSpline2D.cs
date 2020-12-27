using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Crener.Spline.Common;
using Crener.Spline.Common.DataStructs;
using Crener.Spline.Common.Interfaces;
using Unity.Assertions;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Crener.Spline.BaseSpline
{
    /// <summary>
    /// Base implementation which contains base functionality and reusable methods
    /// </summary>
    public abstract class BaseSpline2D : BaseSpline, ISpline2DEditor
    {
        [SerializeField]
        protected List<float2> Points = new List<float2>();

        public Spline2DData? SplineEntityData2D
        {
            get
            {
                if(!m_splineData2D.HasValue) ConvertData();
                return m_splineData2D;
            }
            protected set => m_splineData2D = value;
        }

        /// <summary>
        /// true if Spline Entity Data has been initialized, calling <see cref="SplineEntityData2D"/> directly will automatically generate data
        /// </summary>
        public override bool hasSplineEntityData => m_splineData2D.HasValue;

        /// <summary>
        /// Amount of control points in the spline
        /// </summary>
        public override int ControlPointCount => Points.Count;

        private Spline2DData? m_splineData2D = null;

        /// <summary>
        /// Retrieve a point on the spline at a specific control point
        /// </summary>
        /// <returns>point on spline segment</returns>
        public virtual float2 Get2DPointLocal(float progress, int index)
        {
            return SplineInterpolation(progress, index);
        }

        /// <summary>
        /// Adds a point to the end of the spline
        /// </summary>
        public virtual void AddControlPoint(float2 point)
        {
            Points.Add(point);
            RecalculateLengthBias();
        }

        /// <summary>
        /// inserts a point before the specified segment index
        /// </summary>
        /// <param name="index">segment index</param>
        /// <param name="point">location to insert</param>
        public virtual void InsertControlPoint(int index, float2 point)
        {
            if(Points.Count < 1 || index >= ControlPointCount)
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
            if(ControlPointCount == 0 || index < 0) return;

            Points.RemoveAt(math.min(index, ControlPointCount - 1));
            RecalculateLengthBias();
        }

        /// <summary>
        /// Update an existing control points data
        /// </summary>
        /// <param name="index">control point index</param>
        /// <param name="point">location of the point</param>
        /// <param name="mode">type of point to update</param>
        public virtual void UpdateControlPointLocal(int index, float2 point, SplinePoint mode)
        {
            Assert.IsTrue(index <= ControlPointCount);

            Points[index] = point;
            RecalculateLengthBias();
        }

        /// <summary>
        /// Move all points by <paramref name="delta"/> amount
        /// </summary>
        /// <param name="delta">amount to move all point by</param>
        public virtual void MoveControlPoints(float2 delta)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                // float2 is struct so it needs to override the stored value
                float2 local = Points[i] += delta;
                Points[i] = local;
            }
        }

        /// <inheritdoc/>
        public virtual float2 Get2DPointWorld(float progress)
        {
            return Position.xy + Get2DPointLocal(progress);
        }
        
        public virtual float2 Get2DPointWorld(float progress, int index)
        {
            return Position.xy + Get2DPointLocal(progress, index);
        }

        /// <inheritdoc/>
        public virtual float2 Get2DPointLocal(float progress)
        {
            if(ControlPointCount == 0)
                return float2.zero;
            else if(progress <= 0f || ControlPointCount == 1)
                return GetControlPoint2DLocal(0);
            else if(progress >= 1f)
            {
                if(this is ILoopingSpline looped && looped.Looped)
                    return GetControlPoint2DLocal(0);
                return GetControlPoint2DLocal((ControlPointCount - 1));
            }

            int aIndex = FindSegmentIndex(progress);
            float pointProgress = SegmentProgress(progress, aIndex);
            return SplineInterpolation(pointProgress, aIndex);
        }

        protected override float LengthBetweenPoints(int a, int resolution = LengthSampleCount)
        {
            float currentLength = 0;

            float2 aPoint = SplineInterpolation(0f, a);
            for (float i = 1; i <= resolution; i++)
            {
                float2 bPoint = SplineInterpolation(i / resolution, a);
                currentLength += math.distance(aPoint, bPoint);
                aPoint = bPoint;
            }

            return currentLength;
        }

        /// <summary>
        /// Take the <paramref name="position"/> in local space and return the world space location
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected float2 ConvertToWorldSpace(float2 position)
        {
            return (Position + (float3) (Forward * new float3(position, 0f))).xy;
        }

        /// <summary>
        /// Take the <paramref name="position"/>s in local space and convert all to world space locations
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected IEnumerable<float2> ConvertToWorldSpace(IEnumerable<float2> position)
        {
            float2 transPosCache = Position.xy;
            foreach (float2 pos in position)
            {
                yield return transPosCache + ((float3) (Forward * new float3(pos, 0f))).xy;
            }
        }

        /// <summary>
        /// Take the <paramref name="position"/> in world space and return the local space location
        /// </summary>
        protected float2 ConvertToLocalSpace(float2 position)
        {
            return (Position - (float3) (Quaternion.Inverse(Forward) * new float3(position, 0f))).xy;
        }

        /// <summary>
        /// Gets the given point from a point segment
        /// </summary>
        /// <param name="i">index of the segment</param>
        /// <returns>World Space position for the point</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual float2 GetControlPoint2DLocal(int i)
        {
            return Points[i];
        }

        /// <summary>
        /// Gets the given point from a point segment
        /// </summary>
        /// <param name="i">index of the segment</param>
        /// <returns>World Space position for the point</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual float2 GetControlPoint2DWorld(int i)
        {
            return Points[i] + Position.xy;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract float2 SplineInterpolation(float t, int a);

        public override void ClearData()
        {
            base.ClearData();
            
            if(m_splineData2D.HasValue) // access directly to stop possible infinite loop
            {
                SplineEntityData2D.Value.Dispose();
                SplineEntityData2D = null;
            }
        }

        private void Reset()
        {
            Points.Clear();

            Vector3 position = transform.position;
            AddControlPoint(new float2(position.x - 2f, position.y));
            AddControlPoint(new float2(position.x + 2f, position.y));
        }

        protected override void DrawLineGizmos()
        {
            for (int i = 0; i < SegmentPointCount - 1; i++)
            {
                float2 f = Get2DPointWorld(0f, i);
                Vector3 lp = new Vector3(f.x, f.y, 0f);
                int points = math.min((int) (PointDensityF * (SegmentLength[i] * Length())), MaxPointAmount);
                AddToGizmoPointCache(lp);

                for (int s = 1; s <= points; s++)
                {
                    float progress = s / (float) points;
                    float2 p = Get2DPointWorld(progress, i);
                    Vector3 point = new Vector3(p.x, p.y, 0f);

                    Gizmos.DrawLine(lp, point);
                    AddToGizmoPointCache(point);
                    lp = point;
                }
            }
        }

        protected virtual Spline2DData ConvertData()
        {
            ClearData();

            if(SegmentPointCount >= 2 && this is IArkableSpline arkSpline && arkSpline.ArkParameterization)
            {
                SplineEntityData2D = SplineArkConversion(arkSpline.ArkLength);
                return SplineEntityData2D.Value;
            }

            bool looped = this is ILoopingSpline loopSpline && loopSpline.Looped;
            float2[] pointData = new float2[Points.Count + (looped ? 1 : 0)];

            float3 position = Position;
            for (int i = 0; i < Points.Count; i++)
            {
                pointData[i] = position.xy + Points[i];
            }

            if(looped) pointData[Points.Count] = pointData[0];

            NativeArray<float2> points = new NativeArray<float2>(pointData, Allocator.Persistent);
            NativeArray<float> time = new NativeArray<float>(SegmentLength.ToArray(), Allocator.Persistent);

            Assert.IsFalse(hasSplineEntityData);
            SplineEntityData2D = new Spline2DData
            {
                Length = Length(),
                Points = points,
                Time = time
            };

            return SplineEntityData2D.Value;
        }

        /// <summary>
        /// Convert the spline into smaller linear segments with an equal distance between each point (see: <see cref="arkLength"/>)
        /// </summary>
        /// <returns>Linear spline data</returns>
        protected Spline2DData SplineArkConversion(float arkLength)
        {
            float splineLength = Length();
            int expectedPoints = (int) (splineLength / math.max(0.001f, arkLength));
            float targetSegmentSize = splineLength / expectedPoints;
            List<float2> points = new List<float2>(expectedPoints + 2);
            List<float> times = new List<float>(expectedPoints + 1);

            const float distanceTolerance = 0.00005f;
            const float loopTolerance = float.Epsilon * 3f;

            double creepDistanceCovered = 0;
            double progress = 0;
            float2 previous = Get2DPointWorld(0f);
            points.Add(previous); // directly add the first point

            double searchSegmentIterationSize = (double) 1 / expectedPoints;
            double creepSearchSize = searchSegmentIterationSize / 10;
            double creepMin = progress;
            double creepMax = progress + creepSearchSize;
            float2 lastCreepTest = previous;

            do
            {
                // creep along the spline until the distance is greater than what we are searching for
                float2 creepTest = Get2DPointWorld((float) creepMax);
                if(math.distance(previous, creepTest) - targetSegmentSize > 0)
                {
                    // binary search through the spline to get the next point
                    double searchSize = searchSegmentIterationSize / 2;
                    double searchCurrent = creepMin + ((creepMax - creepMin) / 2);
                    float2 testLocation;
                    do
                    {
                        testLocation = Get2DPointWorld((float) searchCurrent);
                        float distance = math.distance(previous, testLocation);
                        float delta = distance - targetSegmentSize;

                        if(math.abs(delta) < distanceTolerance)
                            break;

                        searchSize /= 2;
                        if(delta > 0) searchCurrent -= searchSize;
                        else searchCurrent += searchSize;
                    }
                    while (searchSize > loopTolerance);

                    creepDistanceCovered += math.distance(lastCreepTest, testLocation);
                    points.Add(testLocation);
                    previous = testLocation;
                    progress = searchCurrent;

                    times.Add(((float) creepDistanceCovered) / splineLength);
                    
                    creepMin = progress;
                    creepMax = progress + creepSearchSize;
                    lastCreepTest = testLocation;
                }
                else
                {
                    creepDistanceCovered += math.distance(lastCreepTest, creepTest);
                    creepMin = creepMax;
                    creepMax += creepSearchSize;

                    lastCreepTest = creepTest;
                }
            }
            while (progress <= 1 && creepMax < 1.2);

            // make sure that the last point is really the last one in the source spline
            if(times[times.Count - 1] != 1f)
            {
                points.Add(Get2DPointWorld(1f));
                times.Add(1f);
            }

            return new Spline2DData
            {
                Length = Length(),
                Points = new NativeArray<float2>(points.ToArray(), Allocator.Persistent),
                Time = new NativeArray<float>(times.ToArray(), Allocator.Persistent)
            };
        }
    }
}