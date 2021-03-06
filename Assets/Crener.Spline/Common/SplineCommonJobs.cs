using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Crener.Spline.Common
{
    /// <summary>
    /// Updates the progress by the given speed in order to make progress iterate at a constant speed
    /// </summary>
    [BurstCompile]
    public struct ProgressUpdate : IJobChunk
    {
        [ReadOnly]
        public float SplineLength;
        [ReadOnly]
        public float DeltaTime;

        public ComponentTypeHandle<SplineProgress> MoverDef;
        [ReadOnly]
        public ComponentTypeHandle<TraversalSpeed> SpeedDef;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<SplineProgress> movers = chunk.GetNativeArray(MoverDef);
            NativeArray<TraversalSpeed> speed = chunk.GetNativeArray(SpeedDef);
            for (int i = chunk.Count - 1; i >= 0; i--)
            {
                float progress = movers[i].Progress;
                progress += ((speed[i].Speed / SplineLength) * DeltaTime);
                progress %= 1f;
                movers[i] = new SplineProgress() {Progress = progress};
            }
        }
    }
}