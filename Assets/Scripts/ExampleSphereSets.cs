using Unity.Collections;
using Unity.Mathematics;

namespace RayTracingWeekend
{
    public static class ExampleSphereSets
    {
        public static HitableArray<Sphere> FourVaryingSize(Allocator allocator = Allocator.TempJob)
        {
            return new HitableArray<Sphere>(5, allocator)
            {
                Objects =
                {
                    [0] = new Sphere(new float3(0.35f, -0.1f, -2f), 0.4f),
                    [1] = new Sphere(new float3(-1.4f, 0f, -1.5f), 0.5f),
                    [2] = new Sphere(new float3(1.2f, -0.375f, -1.5f), 0.125f),
                    [3] = new Sphere(new float3(1.4f, -0.45f, -1.12f), 0.05f),
                    [4] = new Sphere(new float3(0f, -100.5f, -1f), 100f)
                }
            };
        }
        
        public static HitableArray<Sphere> FourVaryingSizeAndMaterial(Allocator allocator = Allocator.TempJob)
        {
            return new HitableArray<Sphere>(4, allocator)
            {
                Objects =
                {
                    [0] = new Sphere(new float3(0f, 0f, -1f), 0.5f, 
                                     new Material(MaterialType.Lambertian, new float3(0.8f, 0.3f, 0.3f))),
                    [1] = new Sphere(new float3(0f, -100.5f, -1f), 100f,
                                     new Material(MaterialType.Lambertian, new float3(0.8f, 0.8f, 0.0f))),
                    [2] = new Sphere(new float3(1f, 0f, -1f), 0.5f,
                                     new Material(MaterialType.Metal, new float3(0.8f, 0.6f, 0.2f), 0.3f)),
                    [3] = new Sphere(new float3(-1f, 0f, -1f), 0.5f,
                                     new Material(MaterialType.Metal, new float3(0.8f, 0.8f, 0.8f), 0.1f)),
                }
            };
        }
    }
}