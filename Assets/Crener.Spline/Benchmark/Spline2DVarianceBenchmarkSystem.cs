using System.Collections.Generic;
using Crener.Spline.BezierSpline.Entity;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Crener.Spline.Benchmark
{
    /// <summary>
    /// System for creating benchmark data for the <see cref="Spline2DVarianceTraverserSystem"/> in a controlled manner
    /// </summary>
    [UpdateBefore(typeof(Spline2DVarianceTraverserSystem))]
    public class Spline2DVarianceBenchmarkSystem : ComponentSystem
    {
        protected override void OnStartRunning()
        {
            List<Spline2DVarianceData> splines = new List<Spline2DVarianceData>(20);
            EntityManager.GetAllUniqueSharedComponentData(splines);

            foreach (Spline2DVarianceData spline in splines)
            {
                EntityQuery query = GetEntityQuery(
                    ComponentType.ReadOnly<Spline2DBenchmarkData>(),
                    ComponentType.ReadOnly<Spline2DVarianceData>());
                query.SetSharedComponentFilter(spline);
                if(query.CalculateEntityCount() == 0) continue;

                JobHandle splineHandle;
                NativeArray<Spline2DBenchmarkData> benchmarkData =
                    query.ToComponentDataArray<Spline2DBenchmarkData>(Allocator.TempJob, out splineHandle);
                splineHandle.Complete();

                //create the traversers for the spline
                Random rand = new Random(903);
                EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                foreach (Spline2DBenchmarkData data in benchmarkData)
                {
                    NativeArray<Entity> entities = new NativeArray<Entity>(data.Quantity, Allocator.Temp);
                    entityManager.Instantiate(data.Prefab, entities);

                    for (int i = 0; i < data.Quantity; i++)
                    {
                        Entity entity = entities[i];

                        SplineProgress mover = entityManager.GetComponentData<SplineProgress>(entity);
                        mover.Progress = rand.NextFloat(0f, 1f);
                        entityManager.SetComponentData(entity, mover);

                        SplineVariance var = entityManager.GetComponentData<SplineVariance>(entity);
                        var.Variance = new half(rand.NextFloat(-1f, 1f));
                        entityManager.SetComponentData(entity, var);

                        if(entityManager.HasComponent<Spline2DVarianceData>(entity))
                            entityManager.SetSharedComponentData(entity, spline);
                        else
                            entityManager.AddSharedComponentData(entity, spline);
                    }

                    entities.Dispose();
                }

                benchmarkData.Dispose();
            }
        }

        protected override void OnUpdate() { }

        protected new bool ShouldRunSystem()
        {
            return false;
        }
    }
}