using System.Collections.Generic;
using Code.Spline2.BezierSpline.Entity;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Code.Spline2.Benchmark
{
    /// <summary>
    /// Create certain amount of prefabs as defined by <see cref="Spline2DBenchmarkData"/> and set them up for movement with the <seealso cref="Spline2DTraverserSystem"/>
    /// </summary>
    [UpdateBefore(typeof(Spline2DTraverserSystem))]
    public class Spline2DBenchmarkSystem : ComponentSystem
    {
        protected override void OnStartRunning()
        {
            List<Spline2DData> splines = new List<Spline2DData>(20);
            EntityManager.GetAllUniqueSharedComponentData(splines);

            foreach (Spline2DData spline in splines)
            {
                EntityQuery query = GetEntityQuery(
                    ComponentType.ReadOnly<Spline2DBenchmarkData>(),
                    ComponentType.ReadOnly<Spline2DData>());
                query.SetSharedComponentFilter(spline);
                if(query.CalculateEntityCount() == 0) continue;

                JobHandle splineHandle;
                NativeArray<Spline2DBenchmarkData> benchData =
                    query.ToComponentDataArray<Spline2DBenchmarkData>(Allocator.TempJob, out splineHandle);
                splineHandle.Complete();

                //create the traverser for the spline
                Random rand = new Random(903);
                EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                foreach (Spline2DBenchmarkData data in benchData)
                {
                    NativeArray<Entity> entities = new NativeArray<Entity>(data.Quantity, Allocator.Temp);
                    entityManager.Instantiate(data.Prefab, entities);

                    for (int i = 0; i < data.Quantity; i++)
                    {
                        Entity entity = entities[i];

                        SplineProgress mover = entityManager.GetComponentData<SplineProgress>(entity);
                        mover.Progress = rand.NextFloat(0f, 1f);
                        entityManager.SetComponentData(entity, mover);

                        if(entityManager.HasComponent<Spline2DData>(entity))
                            entityManager.SetSharedComponentData(entity, spline);
                        else
                            entityManager.AddSharedComponentData(entity, spline);
                    }

                    entities.Dispose();
                }

                benchData.Dispose();
            }
        }

        protected override void OnUpdate() { }

        protected new bool ShouldRunSystem()
        {
            return false;
        }
    }
}