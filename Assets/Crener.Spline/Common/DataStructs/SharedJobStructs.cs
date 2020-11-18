using Crener.Spline.Common.Interfaces;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Crener.Spline.Common.DataStructs
{
    public static class SplineHelperMethods
    {
        /// <summary>
        /// Calculates the index that's responsible for the first point
        /// </summary>
        /// <param name="spline">source spline data</param>
        /// <param name="progress">desired progress along the spline</param>
        [BurstCompatible]
        public static int SegmentIndex3D(ref Spline3DData spline, ref SplineProgress progress)
        {
            int seg = spline.Time.Length;
            for (int i = 0; i < seg; i++)
            {
                float time = spline.Time[i];
                if(time >= progress.Progress) return i;
            }
            
#if UNITY_EDITOR && NO_BURST
            if(seg - 1 != spline.Points.Length - 2)
            {
                // if the progress is greater than the spline time it should result in the last point being returned
                throw new IndexOutOfRangeException("Spline time has less data than expected for the requested point range!");
            }
#endif

            return seg - 1;
        }
        
        /// <summary>
        /// Calculate the progress between two points
        /// </summary>
        /// <param name="spline">source spline data</param>
        /// <param name="progress">desired progress along the spline</param>
        /// <param name="index">index of the first point in the spline</param>
        [BurstCompatible]
        public static float SegmentProgress3D(ref Spline3DData spline, ref SplineProgress progress, int index)
        {
            //float tempProgress = math.clamp(progress.Progress, 0f, 1f);

            if(index == 0) return progress.Progress / spline.Time[0];
            if(spline.Time.Length <= 1) return progress.Progress;

            float aLn = spline.Time[index - 1];
            float bLn = spline.Time[index];

            return (progress.Progress - aLn) / (bLn - aLn);
        }
        
        /// <summary>
        /// Calculate the progress between two points
        /// </summary>
        /// <param name="spline">source spline data</param>
        /// <param name="progress">desired progress along the spline</param>
        /// <param name="index">index of the first point in the spline</param>
        [BurstCompatible]
        public static float SegmentProgress3DClamp(ref Spline3DData spline, ref SplineProgress progress, int index)
        {
            float tempProgress = math.clamp(progress.Progress, 0f, 1f);

            if(index == 0) return tempProgress / spline.Time[0];
            if(spline.Time.Length <= 1) return tempProgress;

            float aLn = spline.Time[index - 1];
            float bLn = spline.Time[index];

            return (tempProgress - aLn) / (bLn - aLn);
        }
    }
    
    #region Spline 2D Jobs 
    
    /// <summary>
    /// Empty spline struct
    /// </summary>
    [BurstCompile,BurstCompatible]
    public struct Empty2DPointJob : IJob, ISplineJob2D
    {
        [WriteOnly]
        private NativeReference<float2> m_result;

        #region Interface properties
        /// <summary>
        /// this is really pointless but required for the interface as this job doesn't require this data
        /// </summary>
        public SplineProgress SplineProgress
        {
            get => default;
            set { }
        }

        public float2 Result
        {
            get => m_result.Value;
            set => m_result.Value = value;
        }
        #endregion

        public Empty2DPointJob(Allocator allocator = Allocator.None)
        {
            m_result = new NativeReference<float2>(allocator);
        }

        public void Execute()
        {
            m_result.Value = float2.zero;
        }

        public void Dispose()
        {
            m_result.Dispose();
        }

        public JobHandle Dispose(JobHandle inputDeps)
        {
            return m_result.Dispose(inputDeps);
        }
    }

    /// <summary>
    /// simple point in spline
    /// </summary>
    [BurstCompile,BurstCompatible]
    public struct SinglePoint2DPointJob : IJob, ISplineJob2D
    {
        [ReadOnly]
        public Spline2DData Spline;
        [WriteOnly]
        private NativeReference<float2> m_result;

        #region Interface properties
        public SplineProgress SplineProgress
        {
            get => default(SplineProgress);
            set { }
        }

        public float2 Result
        {
            get => m_result.Value;
            set => m_result.Value = value;
        }
        #endregion

        public SinglePoint2DPointJob(ISpline2D spline, Allocator allocator = Allocator.None)
        {
            Spline = spline.SplineEntityData2D.Value;
            m_result = new NativeReference<float2>(allocator);
        }

        public void Execute()
        {
#if UNITY_EDITOR && NO_BURST
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

            m_result.Value = Spline.Points[0];
        }

        public void Dispose()
        {
            m_result.Dispose();
        }

        public JobHandle Dispose(JobHandle inputDeps)
        {
            return m_result.Dispose(inputDeps);
        }
    }
    
    #endregion

    #region Spline 3D Jobs

    /// <summary>
    /// Empty spline struct
    /// </summary>
    [BurstCompile,BurstCompatible]
    public struct Empty3DPointJob : IJob, ISplineJob3D
    {
        [ReadOnly]
        public Spline3DData Spline;
        [WriteOnly]
        private NativeReference<float3> m_result;

        #region Interface properties
        /// <summary>
        /// this is really pointless but required for the interface as this job doesn't require this data
        /// </summary>
        public SplineProgress SplineProgress
        {
            get => default;
            set { }
        }

        public float3 Result
        {
            get => m_result.Value;
            set => m_result.Value = value;
        }
        #endregion

        public Empty3DPointJob(ISpline3D spline, Allocator allocator = Allocator.None)
        {
            Spline = spline.SplineEntityData3D.Value;
            m_result = new NativeReference<float3>(allocator);
        }

        public void Execute()
        {
#if UNITY_EDITOR && NO_BURST
            if(Spline.Points.Length > 0)
                Assert.IsFalse(true, $"{nameof(Empty3DPointJob)} was used when spline had data! " +
                                     $"It's highly likely that a different {nameof(ISplineJob3D)} should have been used");
#endif

            m_result.Value = float3.zero;
        }

        public static float3 Run(ref Spline3DData Spline)
        {
            
#if UNITY_EDITOR && NO_BURST
            if(Spline.Points.Length > 0)
                Assert.IsFalse(true, $"{nameof(Empty3DPointJob)} was used when spline had data! " +
                                     $"It's highly likely that a different {nameof(ISplineJob3D)} should have been used");
#endif

            return float3.zero;
        }

        public void Dispose()
        {
            m_result.Dispose();
        }

        public JobHandle Dispose(JobHandle inputDeps)
        {
            return m_result.Dispose(inputDeps);
        }
    }

    /// <summary>
    /// Single point in spline spline
    /// </summary>
    [BurstCompile,BurstCompatible]
    public struct SinglePoint3DPointJob : IJob, ISplineJob3D
    {
        [ReadOnly]
        public Spline3DData Spline;
        [WriteOnly]
        private NativeReference<float3> m_result;

        #region Interface properties
        public SplineProgress SplineProgress
        {
            get => default;
            set { }
        }

        public float3 Result
        {
            get => m_result.Value;
            set => m_result.Value = value;
        }
        #endregion

        public SinglePoint3DPointJob(ISpline3D spline, Allocator allocator = Allocator.None)
        {
            Spline = spline.SplineEntityData3D.Value;
            m_result = new NativeReference<float3>(allocator);
        }

        public void Execute()
        {
            m_result.Value = Run(ref Spline);
        }

        public static float3 Run(ref Spline3DData Spline)
        {
#if UNITY_EDITOR && NO_BURST
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

            return Spline.Points[0];
        }

        public void Dispose()
        {
            m_result.Dispose();
        }

        public JobHandle Dispose(JobHandle inputDeps)
        {
            return m_result.Dispose(inputDeps);
        }
    }
    
    #endregion
}