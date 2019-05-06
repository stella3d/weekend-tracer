using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;

namespace RayTracingWeekend
{
    public static class RayMath
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Reflect(float3 v, float3 n)
        {
            return v - 2 * math.dot(v, n) * n;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Schlick(float cosine, float refractionIndex)
        {
            float r0 = (1f - refractionIndex) / (1f + refractionIndex);
            r0 = r0 * r0;
            return r0 + (1 - r0) * math.pow(1 - cosine, 5);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Refract(float3 v, float3 n, float niOverNt, out float3 refracted)
        {
            float3 uv = math.normalize(v);
            float dt = math.dot(uv, n);
            float discriminant = 1f - niOverNt * niOverNt * (1f - dt * dt);
            if (discriminant > 0f)
            {
                refracted = niOverNt * (uv - n * dt) - n * math.sqrt(discriminant);
                return true;
            }
            
            refracted = default;
            return false;
        }
        
        public static float3 Color(Ray r, HitableArray<Sphere> world, int depth, ref Random rng)
        {
            var rec = new HitRecord();
            if (world.Hit(r, 0.001f, float.MaxValue, ref rec))
            {
                Ray scattered = new Ray();
                float3 attenuation = new float3();
                if (depth < 50)
                {
                    if (r.Scatter(rec, ref attenuation, ref scattered, ref rng))
                        return attenuation * Color(scattered, world, depth + 1, ref rng);
                }
                else
                {
                    // this ray has run out of bounces - draw a black pixel
                    return new float3();
                }
            }

            // the ray didn't hit anything, so draw the background color for this pixel
            return Utils.BackgroundColor(ref r);
        }
    }
}