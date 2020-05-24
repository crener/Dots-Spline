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
    /// Moves entities with <see cref="SplineProgress"/>, <see cref="TraversalSpeed"/>, <see cref="Spline2DData"/> along a spline at a given
    /// speed. All movement should be in game units as <see cref="SplineProgress"/> increments are controlled by the splines length and
    /// <see cref="TraversalSpeed"/>.
    /// </summary>
    [AlwaysSynchronizeSystem]
    public class Spline2DTraverserSystem : ComponentSystem
    {
        private EntityQuery m_splineQuery;
        private List<Spline2DData> m_splines;

        protected override void OnStartRunning()
        {
            m_splines = new List<Spline2DData>(20);
            EntityManager.GetAllUniqueSharedComponentData(m_splines);

            m_splineQuery = GetEntityQuery(
                ComponentType.ReadWrite<SplineProgress>(),
                ComponentType.ReadOnly<TraversalSpeed>(),
                ComponentType.ReadOnly<Spline2DData>());
        }

        protected override void OnUpdate()
        {
            JobHandle worker = new JobHandle();
            foreach (Spline2DData spline in m_splines)
            {
                m_splineQuery.SetSharedComponentFilter(spline);
                int entity = m_splineQuery.CalculateEntityCount();
                if(entity == 0) continue;

                // Move entity along the spline (by updating progress)
                ProgressUpdate update = new ProgressUpdate()
                {
                    SplineLength = spline.Length,
                    DeltaTime = Time.DeltaTime,
                    MoverDef = GetArchetypeChunkComponentType<SplineProgress>(false),
                    SpeedDef = GetArchetypeChunkComponentType<TraversalSpeed>(true),
                };
                JobHandle progressHandle = update.Schedule(m_splineQuery);

                // Calculate the new position
                Move2DTraversers move = new Move2DTraversers()
                {
                    Spline = spline,
                    MoverDef = GetArchetypeChunkComponentType<SplineProgress>(true),
                    TransDef = GetArchetypeChunkComponentType<Translation>(false)
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
                foreach (Spline2DData spline in m_splines) spline.Dispose();

                m_splines.Clear();
                m_splines = null;
            }*/
        }

        /// <summary>
        /// Moves objects according to their <see cref="SplineProgress"/> along <see cref="Spline2DData"/> 
        /// </summary>
        [BurstCompile]
        private struct Move2DTraversers : IJobChunk
        {
            [ReadOnly]
            public Spline2DData Spline;
            [ReadOnly]
            public ArchetypeChunkComponentType<SplineProgress> MoverDef;
            public ArchetypeChunkComponentType<Translation> TransDef;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<SplineProgress> movers = chunk.GetNativeArray(MoverDef);
                NativeArray<Translation> transes = chunk.GetNativeArray(TransDef);

                BezierSpline2DPointJob point = new BezierSpline2DPointJob() {Spline = Spline};


                for (int i = chunk.Count - 1; i >= 0; i--)
                {
                    SplineProgress move = movers[i];

                    point.SplineProgress = move;
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