using Crener.Spline.Common.Interfaces;
using Unity.Assertions;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Crener.Spline.Common.DataStructs
{
    /// <summary>
    /// Empty spline struct
    /// </summary>
    [BurstCompile]
    public struct Empty2DPointJob : IJob, ISplineJob2D
    {
        [ReadOnly]
        private SplineProgress m_splineProgress;
        [WriteOnly]
        private float2 m_result;

        #region Interface properties
        public SplineProgress SplineProgress
        {
            get => m_splineProgress;
            set => m_splineProgress = value;
        }

        public float2 Result
        {
            get => m_result;
            set => m_result = value;
        }
        #endregion

        public void Execute()
        {
            m_result = float2.zero;
        }
    }

    /// <summary>
    /// simple point in spline
    /// </summary>
    [BurstCompile]
    public struct SinglePoint2DPointJob : IJob, ISplineJob2D
    {
        [ReadOnly]
        public Spline2DData Spline;
        [ReadOnly]
        private SplineProgress m_splineProgress;
        [WriteOnly]
        private float2 m_result;

        #region Interface properties
        public SplineProgress SplineProgress
        {
            get => m_splineProgress;
            set => m_splineProgress = value;
        }

        public float2 Result
        {
            get => m_result;
            set => m_result = value;
        }
        #endregion

        public void Execute()
        {
#if UNITY_EDITOR
            if(Spline.Points.Length != 1)
            {
                string text = $"{nameof(SinglePoint2DPointJob)} was used when spline had ";
                if(Spline.Points.Length > 1)
                    text = $"more than a single point! It's highly likely that a different {nameof(ISplineJob2D)} should have been used";
                else
                    text += $"no data! {nameof(Empty2DPointJob)} should have been used";
                Assert.IsTrue(false, text);
            }
#endif

            m_result = Spline.Points[0];
        }
    }

    
    /// <summary>
    /// Empty spline struct
    /// </summary>
    [BurstCompile]
    public struct Empty3DPointJob : IJob, ISplineJob3D
    {
        [ReadOnly]
        public Spline3DData Spline;
        [ReadOnly]
        private SplineProgress m_splineProgress;
        [WriteOnly]
        private float3 m_result;

        #region Interface properties
        public SplineProgress SplineProgress
        {
            get => m_splineProgress;
            set => m_splineProgress = value;
        }

        public float3 Result
        {
            get => m_result;
            set => m_result = value;
        }
        #endregion

        public void Execute()
        {
#if UNITY_EDITOR
            Assert.IsFalse(Spline.Points.Length >= 0, $"{nameof(Empty3DPointJob)} was used when spline had data! " +
                                                      $"It's highly likely that a different {nameof(ISplineJob3D)} should have been used");
#endif

            m_result = float3.zero;
        }
    }

    /// <summary>
    /// Single point in spline spline
    /// </summary>
    [BurstCompile]
    public struct SinglePoint3DPointJob : IJob, ISplineJob3D
    {
        [ReadOnly]
        public Spline3DData Spline;
        [ReadOnly]
        private SplineProgress m_splineProgress;
        [WriteOnly]
        private float3 m_result;

        #region Interface properties
        public SplineProgress SplineProgress
        {
            get => m_splineProgress;
            set => m_splineProgress = value;
        }

        public float3 Result
        {
            get => m_result;
            set => m_result = value;
        }
        #endregion

        public void Execute()
        {
#if UNITY_EDITOR
            if(Spline.Points.Length != 1)
            {
                string text = $"{nameof(SinglePoint3DPointJob)} was used when spline had ";
                if(Spline.Points.Length > 1)
                    text = $"more than a single point! It's highly likely that a different {nameof(ISplineJob3D)} should have been used";
                else
                    text += $"no data! {nameof(Empty3DPointJob)} should have been used";
                Assert.IsTrue(false, text);
            }
#endif

            m_result = Spline.Points[0];
        }
    }
}