using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Crener.Spline.BaseSpline
{
    /// <summary>
    /// Base implementation which contains base functionality and reusable methods
    /// </summary>
    public abstract class BaseSpline : MonoBehaviour, ISpline
    {
        [SerializeField]
        protected List<float> SegmentLength = new List<float>();
        // this is generated in editor and then reused in built versions.
        [SerializeField, HideInInspector]
        protected float LengthCache;

        /// <summary>
        /// true if Spline Entity Data has been initialized, calling SplineEntityData directly will automatically generate data
        /// </summary>
        public abstract bool hasSplineEntityData { get; }

        /// <summary>
        /// Length of the spline
        /// </summary>
        public float Length() => LengthCache;

        /// <summary>
        /// Amount of control points in the spline
        /// </summary>
        public abstract int ControlPointCount { get; }

        /// <summary>
        /// The amount of points that the internal workings of the spline should think there are.<para/>
        /// This is useful if the curve loops back around on itself as the calculations to setup the curve can take reused points into account
        /// </summary>
        public virtual int SegmentPointCount => ControlPointCount;

        /// <summary>
        /// Is the data in the spline initialized
        /// </summary>
        protected virtual bool DataInitialized => SegmentPointCount > 0 && SegmentLength.Count >= 0;

        /// <summary>
        /// Position of the spline origin in world space
        /// </summary>
        public float3 Position => trans.position;

        /// <summary>
        /// forward direction of the spline origin
        /// </summary>
        public Quaternion Forward
        {
            get => trans.rotation;
            set
            {
                if(Forward != value) trans.rotation = value;
            }
        }

        private Transform trans
        {
            get
            {
                if(m_trans == null) Start();
                return m_trans;
            }
        }

        private Transform m_trans = null;
        protected const int pointDensityI = 13;
        protected const float pointDensityF = pointDensityI;
        protected const int maxPointAmount = 15000;

        private void Start()
        {
            m_trans = transform;
        }

        public abstract SplineType SplineDataType { get; }

        public abstract void RemoveControlPoint(int index);

        public virtual SplineEditMode GetEditMode(int index)
        {
            return SplineEditMode.Standard;
        }

        public virtual void ChangeEditMode(int index, SplineEditMode mode)
        {
            // just needed for interface, is overriden when required
        }

        protected abstract float LengthBetweenPoints(int a, int resolution = 64);

        protected int FindSegmentIndex(float progress)
        {
            int seg = SegmentLength.Count;
            for (int i = 0; i < seg; i++)
            {
                float time = SegmentLength[i];
                if(time >= progress) return i;
            }

            // should never hit this point as the time segment should take care of things
#if UNITY_EDITOR
            throw new Exception($"Segment index is out of range! progress: '{progress}' could not be resolved");
#else
            return SegmentLength.Count - 1;
#endif
        }

        /// <summary>
        /// Calculates the segment progress given the spline progress
        /// </summary>
        /// <param name="progress">spline progress</param>
        /// <param name="index">segment index</param>
        /// <returns>segment progress</returns>
        protected float SegmentProgress(float progress, int index)
        {
            if(SegmentPointCount <= 2) return progress;

            if(index == 0)
            {
                float segmentProgress = SegmentLength[0];
                return progress / segmentProgress;
            }

            float aLn = SegmentLength[index - 1];
            float bLn = SegmentLength[index];

            return (progress - aLn) / (bLn - aLn);
        }

        protected virtual void RecalculateLengthBias()
        {
            ClearData();
            SegmentLength.Clear();

            if(SegmentPointCount <= 1)
            {
                LengthCache = 0f;
                SegmentLength.Add(1f);
                return;
            }

            // calculate the distance that the entire spline covers
            float currentLength = 0f;
            for (int a = 0; a < SegmentPointCount - 1; a++)
            {
                float length = LengthBetweenPoints(a, 128);
                currentLength += length;
            }

            LengthCache = currentLength;

            if(SegmentPointCount == 2)
            {
                SegmentLength.Add(1f);
                return;
            }

            // calculate the distance that a single segment covers
            float segmentCount = 0f;
            for (int a = 0; a < SegmentPointCount - 1; a++)
            {
                float length = LengthBetweenPoints(a, 128);
                segmentCount = (length / LengthCache) + segmentCount;
                SegmentLength.Add(segmentCount);
            }

            // double check that the last point is 1.0 cause sometimes floating point error seeps in
            SegmentLength[SegmentLength.Count - 1] = 1.0f;
        }

        public void Dispose()
        {
            ClearData();
        }

        private void OnDestroy()
        {
            ClearData();
        }

        public virtual void ClearData()
        {
#if UNITY_EDITOR
            ClearPointCache();
#endif
        }

#if UNITY_EDITOR
        // these variables are used in editor to reduce overhead by caching the line segments that need to be rendered in scene until the data is changed
        protected readonly List<Vector3> m_pointCache = new List<Vector3>(30);
        private float3 m_lastTransPos = float3.zero;
        private Quaternion m_lastForward = Quaternion.identity;
#endif

        [Conditional("UNITY_EDITOR"), MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void AddToGizmoPointCache(Vector3 point) => m_pointCache.Add(point);

        [Conditional("UNITY_EDITOR"), MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ClearPointCache() => m_pointCache.Clear();

        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.gray;

            if(!DataInitialized)
            {
                // needs to calculate length as it might not have been saved correctly after saving
                RecalculateLengthBias();
            }

#if UNITY_EDITOR
            bool positionChanged = !Equals(Position, m_lastTransPos);
            bool rotationChanged = positionChanged || !Equals(Forward, m_lastForward);
            if(m_pointCache.Count >= 2 && !rotationChanged)
            {
                // There is existing point data so save the time in recalculating the points
                for (int i = 1; i < m_pointCache.Count; i++)
                {
                    Gizmos.DrawLine(m_pointCache[i - 1], m_pointCache[i]);
                }
            }
            else
#endif
            {
#if UNITY_EDITOR
                if(rotationChanged)
                {
                    m_pointCache.Clear();
                    m_lastTransPos = Position;
                    m_lastForward = Forward;
                }
#endif
                DrawLineGizmos();
            }
        }

        /// <summary>
        /// Draw debug lines and populate the gizmo line cache
        /// </summary>
        protected abstract void DrawLineGizmos();

        public virtual void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            System.Diagnostics.Debugger.Break();
            throw new NotImplementedException("Attempted to convert Spline MonoBehaviour into Spline Entity");
        }
    }
}