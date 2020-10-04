using System.Collections.Generic;
using Crener.Spline.BezierSpline.Jobs;
using Crener.Spline.Common;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Crener.Spline.BezierSpline.Entity
{
    /// <summary>
    /// Moves entities with <see cref="SplineProgress"/>, <see cref="TraversalSpeed"/>, <see cref="SplineVariance"/> and
    /// <see cref="Spline2DVarianceData"/> along a spline at a given speed. All movement should be in game units as
    /// <see cref="SplineProgress"/> increments are controlled by the splines length at the given <see cref="SplineVariance"/> and
    /// <see cref="TraversalSpeed"/>.
    /// </summary>
    [AlwaysSynchronizeSystem]
    public class Spline2DVarianceTraverserSystem : ComponentSystem
    {
        private EntityQuery m_splineQuery;
        private List<Spline2DVarianceData> m_splines;

        protected override void OnStartRunning()
        {
            m_splines = new List<Spline2DVarianceData>(20);
            EntityManager.GetAllUniqueSharedComponentData(m_splines);

            m_splineQuery = GetEntityQuery(
                ComponentType.ReadWrite<SplineProgress>(),
                ComponentType.ReadOnly<TraversalSpeed>(),
                ComponentType.ReadOnly<SplineVariance>(),
                ComponentType.ReadOnly<Spline2DVarianceData>());
        }

        protected override void OnUpdate()
        {
            JobHandle worker = new JobHandle();
            foreach (Spline2DVarianceData spline in m_splines)
            {
                m_splineQuery.SetSharedComponentFilter(spline);
                int entity = m_splineQuery.CalculateEntityCount();
                if(entity == 0) continue;

                // Move entity along the spline (by updating progress)
                ProgressUpdate update = new ProgressUpdate()
                {
                    SplineLengthCenter = spline.Length[0],
                    SplineLengthLeft = spline.Length[1],
                    SplineLengthRight = spline.Length[2],
                    DeltaTime = Time.DeltaTime,
                    MoverDef = GetComponentTypeHandle<SplineProgress>(false),
                    SpeedDef = GetComponentTypeHandle<TraversalSpeed>(true),
                    VarianceDef = GetComponentTypeHandle<SplineVariance>(true),
                };
                JobHandle progressHandle = update.Schedule(m_splineQuery);

                // Calculate the new position
                MoveTraversers move = new MoveTraversers()
                {
                    Spline = spline,
                    MoverDef = GetComponentTypeHandle<SplineProgress>(true),
                    VarianceDef = GetComponentTypeHandle<SplineVariance>(true),
                    TransDef = GetComponentTypeHandle<Translation>(false)
                };
                JobHandle moveHandle = move.Schedule(m_splineQuery, progressHandle);

                worker = JobHandle.CombineDependencies(moveHandle, worker);
            }

            worker.Complete();
        }

        protected override void OnDestroy()
        {
            /*if(m_splines != null && m_splines.Count > 0)
            {
                foreach (Spline2DVarianceData spline in m_splines) spline.Dispose();

                m_splines.Clear();
                m_splines = null;
            }*/
        }

        /// <summary>
        /// Updates the progress of the mover
        /// </summary>
        [BurstCompile]
        private struct ProgressUpdate : IJobChunk
        {
            [ReadOnly]
            public float SplineLengthCenter, SplineLengthLeft, SplineLengthRight;
            [ReadOnly]
            public float DeltaTime;

            public ComponentTypeHandle<SplineProgress> MoverDef;
            [ReadOnly]
            public ComponentTypeHandle<TraversalSpeed> SpeedDef;
            [ReadOnly]
            public ComponentTypeHandle<SplineVariance> VarianceDef;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<SplineProgress> movers = chunk.GetNativeArray(MoverDef);
                NativeArray<TraversalSpeed> speed = chunk.GetNativeArray(SpeedDef);
                NativeArray<SplineVariance> variance = chunk.GetNativeArray(VarianceDef);

                for (int i = chunk.Count - 1; i >= 0; i--)
                {
                    float length = SplineLengthCenter;
                    if(variance[i].Variance > new half(0f))
                        length = math.lerp(SplineLengthCenter, SplineLengthRight, variance[i].Variance);
                    else if(variance[i].Variance < new half(0f))
                        length = math.lerp(SplineLengthCenter, SplineLengthLeft, math.abs(variance[i].Variance));

                    float progress = movers[i].Progress;
                    progress += ((speed[i].Speed / length) * DeltaTime);
                    progress %= 1f;
                    movers[i] = new SplineProgress() {Progress = progress};
                }
            }
        }

        /// <summary>
        /// Moves objects according to their spline progress
        /// </summary>
        [BurstCompile]
        private struct MoveTraversers : IJobChunk
        {
            [ReadOnly]
            public Spline2DVarianceData Spline;
            [ReadOnly]
            public ComponentTypeHandle<SplineProgress> MoverDef;
            [ReadOnly]
            public ComponentTypeHandle<SplineVariance> VarianceDef;
            public ComponentTypeHandle<Translation> TransDef;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<SplineProgress> movers = chunk.GetNativeArray(MoverDef);
                NativeArray<SplineVariance> variance = chunk.GetNativeArray(VarianceDef);
                NativeArray<Translation> transes = chunk.GetNativeArray(TransDef);

                BezierSpline2DVariancePointJob point = new BezierSpline2DVariancePointJob()
                {
                    Spline = Spline
                };


                for (int i = chunk.Count - 1; i >= 0; i--)
                {
                    point.SplineProgress = movers[i];
                    point.SplineVariance = variance[i];
                    point.Execute();

                    transes[i] = new Translation()
                    {
                        Value = new float3(point.Result, 0f)
                    };
                }
            }
        }
    }
}