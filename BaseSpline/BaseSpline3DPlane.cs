using System.Collections.Generic;
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
    public abstract class BaseSpline3DPlane : BaseSpline2D, ISpline3DPlane, ISpline3DPlaneEditor
    {
        public Spline3DData? SplineEntityData3D
        {
            get
            {
                if(!m_splineData3D.HasValue) // access directly to stop infinite loop
                    ConvertData3D();
                return m_splineData3D;
            }
            protected set => m_splineData3D = value;
        }

        private Spline3DData? m_splineData3D = null;

        /// <summary>
        /// true if Spline Entity Data has been initialized, calling <see cref="SplineEntityData3D"/> directly will automatically generate data
        /// </summary>
        public new bool hasSplineEntityData => base.hasSplineEntityData && m_splineData3D.HasValue;

        public virtual float3 Get3DPointWorld(float progress)
        {
            float2 point = GetPointProgress(progress, false);
            return Convert2Dto3D(point, true);
        }

        public virtual float3 Get3DPointLocal(float progress)
        {
            float2 point = GetPointProgress(progress, false);
            return Convert2Dto3D(point, false);
        }

        public float3 Get3DPoint(float progress, int index)
        {
            float2 point = SplineInterpolation(progress, index);
            return Convert2Dto3D(point, true);
        }

        public void AddControlPoint(float3 point)
        {
            float2 converted = Convert3Dto2D(point, false);
            base.AddControlPoint(converted);
        }

        public void InsertControlPointWorldSpace(int index, float3 point)
        {
            float2 converted = Convert3Dto2D(point, true);
            base.InsertControlPoint(index, converted);
        }

        public void InsertControlPointLocalSpace(int index, float3 point)
        {
            base.InsertControlPoint(index, point.xy);
        }

        /// <summary>
        /// Returns the control point relative to the transform of the object
        /// </summary>
        /// <param name="i">control point index to retrieve</param>
        public float3 GetControlPoint3DLocal(int i)
        {
            return new float3(GetControlPoint2DLocal(i), 0f);
        }

        /// <summary>
        /// Returns the control point relative to the world origin
        /// </summary>
        /// <param name="i">control point index to retrieve</param>
        public float3 GetControlPoint3DWorld(int i)
        {
            return Convert2Dto3D(GetControlPoint2DLocal(i), true);
        }

        public void UpdateControlPointWorld(int index, float3 point, SplinePoint mode)
        {
            base.UpdateControlPointLocal(index, Convert3Dto2D(point), mode);
        }

        public void UpdateControlPointLocal(int index, float3 point, SplinePoint mode)
        {
            base.UpdateControlPointLocal(index, Convert3Dto2D(point, false), mode);
        }

        public void MoveControlPoints(float3 delta)
        {
            base.MoveControlPoints(Convert3Dto2D(delta, false));
        }

        protected float3 Convert2Dto3D(float2 point, bool translate = true, bool rotate = true)
        {
            float3 convertedPoint = new float3(point, 0f);
            if(rotate) convertedPoint = Forward * convertedPoint;
            if(translate) convertedPoint += Position;
            return convertedPoint;
        }

        protected float2 Convert3Dto2D(float3 point, bool translate = true)
        {
            float3 convertedPoint = point;
            if(translate) convertedPoint -= Position;
            convertedPoint = Quaternion.Inverse(Forward) * convertedPoint;
            return convertedPoint.xy;
        }

        /// <summary>
        /// Relieve a point on the spline
        /// </summary>
        /// <returns>point on spline</returns>
        public override float2 Get2DPointWorld(float progress)
        {
            return GetPointProgress(progress, true);
        }

        protected virtual float2 GetPointProgress(float progress, bool translate)
        {
            float2 translation = translate ? Position.xy : float2.zero;
            if(ControlPointCount == 0)
                return translation;
            else if(progress <= 0f || ControlPointCount == 1)
                return translation + GetControlPoint2DLocal(0);
            else if(progress >= 1f)
            {
                if(this is ILoopingSpline looped && looped.Looped)
                    return translation + GetControlPoint2DLocal(0);
                return translation + GetControlPoint2DLocal((ControlPointCount - 1));
            }

            int aIndex = FindSegmentIndex(progress);
            float pointProgress = SegmentProgress(progress, aIndex);
            return translation + SplineInterpolation(pointProgress, aIndex);
        }

        protected override void OnDrawGizmosSelected()
        {
            float2 bottomLeft = float.NaN, topRight = float.NaN;
            for (int i = 0; i < Points.Count; i++)
            {
                float2 cp = GetControlPoint2DLocal(i);
                bottomLeft.x = math.min(cp.x, bottomLeft.x);
                bottomLeft.y = math.min(cp.y, bottomLeft.y);
                topRight.x = math.max(cp.x, topRight.x);
                topRight.y = math.max(cp.y, topRight.y);
            }

            // draw the outer rect
            {
                const float margin = 2f;
                Gizmos.color = new Color(0.35f, 0.35f, 0.35f);
                float3 bottomLeftPoint = Convert2Dto3D(bottomLeft - margin),
                    topLeftPoint = Convert2Dto3D(new float2(bottomLeft.x - margin, topRight.y + margin)),
                    topRightPoint = Convert2Dto3D(topRight + margin),
                    bottomRightPoint = Convert2Dto3D(new float2(topRight.x + margin, bottomLeft.y - margin));

                Gizmos.DrawLine(bottomLeftPoint, bottomRightPoint);
                Gizmos.DrawLine(bottomRightPoint, topRightPoint);
                Gizmos.DrawLine(topRightPoint, topLeftPoint);
                Gizmos.DrawLine(topLeftPoint, bottomLeftPoint);
            }

            base.OnDrawGizmosSelected();
        }

        protected override void DrawLineGizmos()
        {
            for (int i = 0; i < SegmentPointCount - 1; i++)
            {
                float3 lp = Get3DPoint(0f, i);
                int points = (int) (PointDensityF * (SegmentLength[i] * Length()));
                AddToGizmoPointCache(lp);

                for (int s = 0; s <= points; s++)
                {
                    float progress = s / (float) points;
                    float3 p = Get3DPoint(progress, i);

                    Gizmos.DrawLine(lp, p);
                    AddToGizmoPointCache(lp);
                    lp = p;
                }
            }
        }

        public override void ClearData()
        {
            base.ClearData();
            
            if(m_splineData3D.HasValue) // access directly to stop infinite loop
            {
                SplineEntityData3D.Value.Dispose();
                SplineEntityData3D = null;
            }
        }

        protected virtual Spline3DData ConvertData3D()
        {
            ClearData();

            if(SegmentPointCount >= 2 && this is IArkableSpline arkSpline && arkSpline.ArkParameterization)
            {
                SplineEntityData3D = SplineArkConversion3D(arkSpline.ArkLength);
                return SplineEntityData3D.Value;
            }

            bool looped = this is ILoopingSpline loopSpline && loopSpline.Looped;
            float3[] pointData = new float3[Points.Count + (looped ? 1 : 0)];

            for (int i = 0; i < Points.Count; i++)
            {
                pointData[i] = Convert2Dto3D(Points[i]);
            }

            if(looped) pointData[Points.Count] = pointData[0];

            NativeArray<float3> points = new NativeArray<float3>(pointData, Allocator.Persistent);
            NativeArray<float> time = new NativeArray<float>(SegmentLength.ToArray(), Allocator.Persistent);

            Assert.IsFalse(hasSplineEntityData);
            SplineEntityData3D = new Spline3DData
            {
                Length = Length(),
                Points = points,
                Time = time
            };

            return SplineEntityData3D.Value;
        }

        /// <summary>
        /// Convert the spline into smaller linear segments with an equal distance between each point (see: <see cref="arkLength"/>)
        /// </summary>
        /// <returns>Linear spline data</returns>
        protected Spline3DData SplineArkConversion3D(float arkLength)
        {
            float splineLength = Length();
            int expectedPoints = (int) (splineLength / math.max(0.001f, arkLength));
            float targetSegmentSize = splineLength / expectedPoints;
            List<float3> points = new List<float3>(expectedPoints + 2);
            List<float> times = new List<float>(expectedPoints + 1);

            const float distanceTolerance = 0.00005f;
            const float loopTolerance = float.Epsilon * 3f;

            double creepDistanceCovered = 0;
            double progress = 0;
            float3 previous = Get3DPointWorld(0f);
            points.Add(previous); // directly add the first point

            double searchSegmentIterationSize = (double) 1 / expectedPoints;
            double creepSearchSize = searchSegmentIterationSize / 10;
            double creepMin = progress;
            double creepMax = progress + creepSearchSize;
            float3 lastCreepTest = previous;

            do
            {
                // creep along the spline until the distance is greater than what we are searching for
                float3 creepTest = Get3DPointWorld((float) creepMax);
                if(math.distance(previous, creepTest) - targetSegmentSize > 0)
                {
                    // binary search through the spline to get the next point
                    double searchSize = searchSegmentIterationSize / 2;
                    double searchCurrent = creepMin + ((creepMax - creepMin) / 2);
                    float3 testLocation;
                    do
                    {
                        testLocation = Get3DPointWorld((float) searchCurrent);
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
                points.Add(Get3DPointWorld(1f));
                times.Add(1f);
            }
            
            return new Spline3DData
            {
                Length = splineLength,
                Points = new NativeArray<float3>(points.ToArray(), Allocator.Persistent),
                Time = new NativeArray<float>(times.ToArray(), Allocator.Persistent)
            };
        }
    }
}