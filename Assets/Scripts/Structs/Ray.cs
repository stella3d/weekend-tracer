using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace RayTracingWeekend
{
    public struct Ray
    {
        public float3 origin;
        public float3 direction;

        public Ray(float3 origin, float3 direction)
        {
            this.origin = origin;
            this.direction = direction;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float3 PointAtParameter(float t)
        {
            return origin + t * direction;
        }
    }
}