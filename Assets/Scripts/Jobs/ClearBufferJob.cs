using Unity.Collections;
using Unity.Jobs;

namespace RayTracingWeekend
{
    public struct ClearAccumulatedJob<T> : IJobParallelFor where T: struct
    {
        [WriteOnly] public NativeArray<T> Buffer;
            
        public void Execute(int index)
        {
            Buffer[index] = default;
        }
    }
}