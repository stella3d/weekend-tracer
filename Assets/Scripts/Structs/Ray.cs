using System.Runtime.CompilerServices;
using Unity.Mathematics;
using ScatterMethod = RayTracingWeekend.Scatter;

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
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Scatter(HitRecord rec,
            ref float3 attenuation, ref Ray scattered, ref Random rng)
        {
            return ScatterMethod.Generic(this, rec, ref attenuation, ref scattered, ref rng);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Reflect(float3 v, float3 n)
        {
            return v - 2 * math.dot(v, n) * n;
        }
    }
}