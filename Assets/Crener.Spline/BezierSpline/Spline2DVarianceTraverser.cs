using Crener.Spline.BezierSpline.Entity;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Crener.Spline.BezierSpline
{
    /// <summary>
    /// Moves the game object along a <see cref="Spline2DVariance"/>
    /// </summary>
    public class Spline2DVarianceTraverser : MonoBehaviour, IConvertGameObjectToEntity
    {
        public Spline2DVariance Spline;
        [Range(0f, 1f)]
        public float Progress;
        [Range(0.001f, 3f)]
        public float Speed = 0.4f;
        [Range(-1f, 1f)]
        public float Variance;

        private float m_speedNorm;
        private half m_variance;

        private void Start()
        {
            m_variance = new half(Variance);
            m_speedNorm = Speed / Spline.Length(m_variance);
        }

        private void Update()
        {
            Progress += (m_speedNorm * Time.deltaTime);
            Progress %= 1f;

            float2 pos = Spline.GetPoint(Progress, m_variance);
            transform.position = new Vector3(pos.x, pos.y, 0f);
        }

        public void Convert(Unity.Entities.Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<SplineProgress>(entity);
            dstManager.SetComponentData(entity, new SplineProgress() {Progress = Progress});

            dstManager.AddComponent<TraversalSpeed>(entity);
            dstManager.SetComponentData(entity, new TraversalSpeed {Speed = Speed});

            dstManager.AddComponent<SplineVariance>(entity);
            dstManager.SetComponentData(entity, new SplineVariance() {Variance = m_variance});

            if(Spline != null)
            {
                if(!Spline.SplineEntityData.HasValue)
                {
                    // make sure that there is spline data to look reference
                    Spline.Convert(dstManager.CreateEntity(), dstManager, conversionSystem);
                }

                dstManager.AddSharedComponentData(entity, Spline.SplineEntityData.Value);
            }
        }
    }
}