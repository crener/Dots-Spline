using Crener.Spline.BezierSpline;
using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Crener.Spline
{
    /// <summary>
    /// Moves the game object along a <see cref="BezierSpline2DSimple"/>
    /// </summary>
    public class Spline2DTraverser : MonoBehaviour, IConvertGameObjectToEntity
    {
        public ISpline2D Spline;
        [Range(0f, 1f)]
        public float Progress;
        [Range(0.001f, 3f)]
        public float Speed = 0.4f;

        private float m_speedNorm;

        private void Start()
        {
            m_speedNorm = Speed / Spline.Length();
        }

        private void Update()
        {
            Progress += (m_speedNorm * Time.deltaTime);
            Progress %= 1f;

            float2 pos = Spline.GetPoint(Progress);
            transform.position = new Vector3(pos.x, pos.y, 0f);
        }

        public void Convert(Unity.Entities.Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<SplineProgress>(entity);
            dstManager.SetComponentData(entity, new SplineProgress() {Progress = Progress});

            dstManager.AddComponent<TraversalSpeed>(entity);
            dstManager.SetComponentData(entity, new TraversalSpeed {Speed = Speed});

            if(Spline != null)
            {
                // make sure that there is spline data to look at
                if(!Spline.SplineEntityData.HasValue)
                {
                    Spline.Convert(dstManager.CreateEntity(), dstManager, conversionSystem);
                }

                dstManager.AddSharedComponentData(entity, Spline.SplineEntityData.Value);
            }
            else
            {
                Debug.LogWarning(
                    $"Unable to create functioning Entity Spline Traverser without a set spline! Gameobject: {gameObject.name}");
            }
        }
    }
}