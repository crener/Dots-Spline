using Crener.Spline.BezierSpline.Entity;
using Crener.Spline.Common.Interfaces;
using Unity.Entities;
using UnityEngine;

namespace Crener.Spline.Benchmark
{
    /// <summary>
    /// System for creating benchmark data for the <see cref="Spline2DTraverserSystem"/>
    /// </summary>
    [UpdateBefore(typeof(Spline2DTraverserSystem))]
    public class Spline2DBenchmark : MonoBehaviour, IConvertGameObjectToEntity
    {
        public int Quantity = 1000;
        public MonoBehaviour Spline;
        public GameObject Prefab;
        public GameObject Parent;

        private ISpline2D m_spline;

        private void Start()
        {
            if(Parent == null) Parent = gameObject;
            if(GetComponent<ConvertToEntity>() != null) return;

            m_spline = Spline == null ? null : Spline.GetComponent<ISpline2D>();
            if(m_spline == null)
            {
                Debug.LogError("Please assign a spline");
                enabled = false;
                return;
            }

            for (int i = 0; i < Quantity; i++)
            {
                GameObject example = Instantiate(Prefab, Parent.transform, true);

                Spline2DTraverser mover = example.GetComponent<Spline2DTraverser>();
                mover.Spline = m_spline;
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
                if(!m_spline.SplineEntityData2D.HasValue)
                {
                    m_spline.Convert(dstManager.CreateEntity(), dstManager, conversionSystem);
                }

                dstManager.AddSharedComponentData(entity, m_spline.SplineEntityData2D.Value);
            }
        }
    }
}