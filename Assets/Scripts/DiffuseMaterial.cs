using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace RayTracingWeekend
{
    public struct DiffuseMaterial
    {
        public float3 albedo;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Scatter(Random rand, float3 albedo, Ray r, HitRecord rec, ref float3 attenuation, ref Ray scattered)
        {
            var target = rec.p + rec.normal + Utils.RandomInUnitSphere(rand);
            scattered = new Ray(rec.p, target - rec.p);
            attenuation = albedo;
            return true;
        }
    }
    
    public struct MetalMaterial
    {
        public float3 albedo;
        
        public bool Scatter(Ray r, HitRecord rec, ref float3 attenuation, ref Ray scattered)
        {
            float3 reflected = Reflect(math.normalize(r.direction), rec.normal);
            scattered = new Ray(rec.p, reflected);
            attenuation = albedo;
            return math.dot(scattered.direction, rec.normal) > 0;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Scatter(Ray r, HitRecord rec, Random rand, ref float3 attenuation, ref Ray scattered)
        {
            var m = rec.material;
            float3 reflected = Reflect(math.normalize(r.direction), rec.normal);
            scattered = new Ray(rec.p, reflected + m.fuzziness * Utils.RandomInUnitSphere(rand));
            attenuation = m.albedo;
            return math.dot(scattered.direction, rec.normal) > 0;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Reflect(float3 v, float3 n)
        {
            return v - 2 * math.dot(v, n) * n;
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
    }
}