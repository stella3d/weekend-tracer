using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace RayTracingWeekend
{
    public struct DiffuseMaterial
    {
        public float3 albedo;
        
        public bool Scatter(Random rand, Ray r, HitRecord rec, ref float3 attenuation, ref Ray scattered)
        {
            var target = rec.p + rec.normal + Utils.RandomInUnitSphere(rand);
            scattered = new Ray(rec.p, target - rec.p);
            attenuation = albedo;
            return true;
        }

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
        
        public static bool Scatter(Material m, Ray r, HitRecord rec, Random rand, ref float3 attenuation, ref Ray scattered)
        {
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
    }
}