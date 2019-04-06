using Unity.Collections;

namespace RayTracingWeekend
{
    public interface IGetPixelBuffer<T> where T: struct
    {
        NativeArray<T> GetPixels();
    }
}