using Crener.Spline.BezierSpline;
using Crener.Spline.BezierSpline.Entity;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Crener.Spline.Benchmark
{
    /// <summary>
    /// System for creating benchmark data for the <see cref="Spline2DTraverserSystem"/>
    /// </summary>
    [UpdateBefore(typeof(Spline2DTraverserSystem))]
    public class Spline2DBenchmark : MonoBehaviour, IConvertGameObjectToEntity
    {
        public int Quantity = 1000;
        [FormerlySerializedAs("BezierSpline")]
        public ISpline2D Spline;
        public GameObject Prefab;
        public GameObject Parent;

        private void Start()
        {
            if(Parent == null) Parent = gameObject;
            if(GetComponent<ConvertToEntity>() != null) return;

            for (int i = 0; i < Quantity; i++)
            {
                GameObject example = Instantiate(Prefab, Parent.transform, true);

                Spline2DTraverser mover = example.GetComponent<Spline2DTraverser>();
                mover.Spline = Spline;
                mover.Progress = Random.Range(0f, 1f);
#if UNITY_EDITOR
                mover.transform.name = "Mover " + (i + 1);
#endif
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

            if(Spline != null)
            {
                if(!Spline.SplineEntityData.HasValue)
                {
                    Spline.Convert(dstManager.CreateEntity(), dstManager, conversionSystem);
                }

                dstManager.AddSharedComponentData(entity, Spline.SplineEntityData.Value);
            }
        }
    }
}