using Crener.Spline.Common.Interfaces;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Crener.Spline.Benchmark
{
    public class Spline2DVarianceBenchmark : MonoBehaviour, IConvertGameObjectToEntity
    {
        public int Quantity = 10000;
        public ISimpleSpline2DVariance BezierSpline;
        public GameObject Prefab;
        public GameObject Parent;

        private void Start()
        {
            if(Parent == null) Parent = gameObject;
            if(GetComponent<ConvertToEntity>() != null) return;

            Random rand = new Random(234);
            for (int i = 0; i < Quantity; i++)
            {
                GameObject example = Instantiate(Prefab, Parent.transform, true);

                Spline2DVarianceTraverser mover = example.GetComponent<Spline2DVarianceTraverser>();
                mover.Spline = BezierSpline;
                mover.Progress = rand.NextFloat(0f, 1f);
#if UNITY_EDITOR
                mover.transform.name = "Mover " + (i + 1);
#endif
                mover.Variance = new half(rand.NextFloat(-1f, 1f));
            }
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<Spline2DBenchmarkData>(entity);

            // convert prefab to entity
            World defaultWorld = World.DefaultGameObjectInjectionWorld;
            GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(defaultWorld, null);
            Entity converted = GameObjectConversionUtility.ConvertGameObjectHierarchy(Prefab, settings);

            dstManager.SetComponentData(entity, new Spline2DBenchmarkData
            {
                Quantity = Quantity,
                Prefab = converted
            });

            if(BezierSpline != null)
            {
                if(!BezierSpline.SplineVarianceEntityData.HasValue)
                {
                    BezierSpline.Convert(dstManager.CreateEntity(), dstManager, conversionSystem);
                }

                dstManager.AddSharedComponentData(entity, BezierSpline.SplineVarianceEntityData.Value);
            }
        }
    }
}