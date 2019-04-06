using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace RayTracingWeekend
{
    public struct CameraFrame
    {
        public float3 origin;
        public float3 lowerLeftCorner;
        public float3 horizontal;
        public float3 vertical;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Ray GetRay(float u, float v)
        {
            return new Ray(origin, lowerLeftCorner + u * horizontal + v * vertical);
        }
        
        public static CameraFrame Default =>
            new CameraFrame
            {
                lowerLeftCorner = new float3(-2, -1, -1),
                horizontal = new float3(4, 0, 0),
                vertical = new float3(0, 2, 0),
                origin = new float3()
            };
    }
}