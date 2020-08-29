using System;
using System.Linq;
using Crener.Spline.Common.DataStructs;
using Crener.Spline.Common.Interfaces;
using Unity.Assertions;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Color = UnityEngine.Color;

namespace Crener.Spline.BaseSpline
{
    /// <summary>
    /// Base implementation which contains base functionality and reusable methods
    /// </summary>
    public abstract class BaseSpline3DPlain : BaseSpline2D, ISpline3DPlain
    {
        [SerializeField]
        protected Quaternion m_forward = Quaternion.identity;

        public Quaternion Forward
        {
            get => m_forward;
            set
            {
                m_forward = value;
                RecalculateLengthBias();
            }
        }
        public Spline3DData? SplineEntityData3D
        {
            get
            {
                if(!hasSplineEntityData) ConvertData3D();
                return m_splineData3D;
            }
            protected set => m_splineData3D = value;
        }

        private Spline3DData? m_splineData3D = null;
        private Transform trans;

        private void Start()
        {
            trans = GetComponent<Transform>();
        }

        /// <summary>
        /// true if Spline Entity Data has been initialized, calling <see cref="SplineEntityData3D"/> directly will automatically generate data
        /// </summary>
        public override bool hasSplineEntityData => base.hasSplineEntityData && m_splineData3D.HasValue;

        public float3 Get3DPoint(float progress)
        {
            return Convert2Dto3D(base.Get2DPoint(progress));
        }

        public float3 Get3DPoint(float progress, int index)
        {
            return Convert2Dto3D(base.Get2DPoint(progress, index));
        }

        public void AddControlPoint(float3 point)
        {
            base.AddControlPoint(Convert3Dto2D(point));
        }

        public void InsertControlPoint(int index, float3 point)
        {
            base.InsertControlPoint(index, Convert3Dto2D(point));
        }

        protected float3 Convert2Dto3D(float2 point)
        {
            return (trans.rotation * new float3(point, 0f)) + trans.position;
        }

        protected float2 Convert3Dto2D(float3 point)
        {
            float3 convertedPoint = (Quaternion.Inverse(trans.rotation) * point) - trans.position;
            Assert.AreEqual(convertedPoint.z, 0f, "3D to 2D conversion failed");
            return new float2(convertedPoint.x, convertedPoint.y);
        }

        protected override void OnDrawGizmosSelected()
        {
            if(trans == null) Start();

            float2 bottomLeft = float.NaN, topRight = float.NaN;
            for (int i = 0; i < Points.Count; i++)
            {
                bottomLeft.x = math.min(Points[i].x, bottomLeft.x);
                bottomLeft.y = math.min(Points[i].y, bottomLeft.y);
                topRight.x = math.max(Points[i].x, topRight.x);
                topRight.y = math.max(Points[i].y, topRight.y);
            }

            // drag the outer rect
            {
                const float margin = 2f;
                Gizmos.color = new Color(0.3f, 0.3f, 0.3f);
                float3 bottomLeftPoint = Convert2Dto3D(bottomLeft - margin),
                    topLeftPoint = Convert2Dto3D(new float2(bottomLeft.x - margin, topRight.y + margin)),
                    topRightPoint = Convert2Dto3D(topRight + margin),
                    bottomRightPoint = Convert2Dto3D(new float2(topRight.x + margin, bottomLeft.y - margin));

                Gizmos.DrawLine(bottomLeftPoint, bottomRightPoint);
                Gizmos.DrawLine(bottomRightPoint, topRightPoint);
                Gizmos.DrawLine(topRightPoint, topLeftPoint);
                Gizmos.DrawLine(topLeftPoint, bottomLeftPoint);
            }

            DrawLineGizmos();
        }

        protected virtual void DrawLineGizmos()
        {
            Gizmos.color = Color.gray;
            const float pointDensity = 13;

            for (int i = 0; i < SegmentPointCount - 1; i++)
            {
                float3 lp = Get3DPoint(0f, i);
                int points = (int) (pointDensity * (SegmentLength[i] * Length()));

                for (int s = 0; s <= points; s++)
                {
                    float progress = s / (float) points;
                    float3 p = Get3DPoint(progress, i);

                    Gizmos.DrawLine(lp, p);
                    lp = p;
                }
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

            float3[] pointData;
            if(this is ILoopingSpline loopSpline && loopSpline.Looped)
            {
                // add an extra point to the end of the array
                pointData = new float3[Points.Count + 1];
                float3[] originalData = Points.Select(p => Convert2Dto3D(p)).ToArray();
                Array.Copy(originalData, pointData, Points.Count);
                pointData[Points.Count] = Convert2Dto3D(Points[0]);
            }
            else pointData = Points.Select(p => Convert2Dto3D(p)).ToArray();

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
        protected virtual Spline3DData SplineArkConversion3D(float arkLength)
        {
            // yes, getting the data from here is kinda cheating but... but it's kinda clean ;)
            Spline2DData spline2D = base.SplineEntityData2D.Value;

            return new Spline3DData
            {
                Length = Length(),
                Points = new NativeArray<float3>(spline2D.Points.Select(p => Convert2Dto3D(p)).ToArray(), Allocator.Persistent),
                Time = new NativeArray<float>(spline2D.Time.ToArray(), Allocator.Persistent)
            };
        }
    }
}