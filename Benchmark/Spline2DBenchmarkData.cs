using Unity.Entities;

namespace Crener.Spline.Benchmark
{
    public struct Spline2DBenchmarkData : IComponentData
    {
        public int Quantity;
        public Entity Prefab;
    }
}