using Unity.Entities;

namespace Code.Spline2.Benchmark
{
    public struct Spline2DBenchmarkData : IComponentData
    {
        public int Quantity;
        public Entity Prefab;
    }
}