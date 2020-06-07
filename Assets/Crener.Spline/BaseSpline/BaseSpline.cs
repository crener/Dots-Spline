using System.Collections.Generic;
using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditor;
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
        protected virtual int SegmentPointCount => ControlPointCount;
        
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

        protected abstract float LengthBetweenPoints(int a, int b, int resolution = 64);

        protected int FindSegmentIndex(float progress)
        {
            int seg = SegmentLength.Count;
            for (int i = 0; i < seg; i++)
            {
                float time = SegmentLength[i];
                if(time >= progress) return i;
            }

            // should never hit this point as the time segment should take care of things
            return 0;
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

        protected void RecalculateLengthBias()
        {
            ClearData();

            if(SegmentPointCount <= 1)
            {
                LengthCache = 0f;
                return;
            }

            // calculate the distance that the entire spline covers
            float currentLength = 0f;
            for (int a = 0; a < SegmentPointCount - 1; a++)
            {
                int b = (a + 1) % SegmentPointCount;
                float length = LengthBetweenPoints(a, b, 128);

                currentLength += length;
            }

            LengthCache = currentLength;

            SegmentLength.Clear();
            if(SegmentPointCount == 2)
            {
                SegmentLength.Add(1f);
                return;
            }

            // calculate the distance that a single segment covers
            float segmentCount = 0f;
            for (int a = 0; a < SegmentPointCount - 1; a++)
            {
                int b = (a + 1) % SegmentPointCount;
                float length = LengthBetweenPoints(a, b);

                segmentCount = (length / LengthCache) + segmentCount;
                SegmentLength.Add(segmentCount);
            }
        }
        
        public void Dispose()
        {
            ClearData();
        }
        
        private void OnDestroy()
        {
            ClearData();
        }

        public abstract void ClearData();

        public abstract void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem);
    }
}