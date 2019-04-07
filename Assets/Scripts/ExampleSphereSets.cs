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
        
        public static HitableArray<Sphere> FourWithDielectric(Allocator allocator = Allocator.TempJob)
        {
            return new HitableArray<Sphere>(4, allocator)
            {
                Objects =
                {
                    [0] = new Sphere(new float3(0f, 0f, -1f), 0.5f, 
                        new Material(MaterialType.Lambertian, new float3(0.1f, 0.2f, 0.5f))),
                    [1] = new Sphere(new float3(0f, -100.5f, -1f), 100f,
                        new Material(MaterialType.Lambertian, new float3(0.8f, 0.8f, 0.0f))),
                    [2] = new Sphere(new float3(1f, 0f, -1f), 0.5f,
                        new Material(MaterialType.Metal, new float3(0.8f, 0.6f, 0.2f))),
                    [3] = new Sphere(new float3(-1f, 0f, -1f), 0.5f,
                        new Material(MaterialType.Dielectric, new float3(), 0f, 1.5f)),
                }
            };
        }
        
        // this is the set used at the end of chapter 9
        public static HitableArray<Sphere> FiveWithDielectric(Allocator allocator = Allocator.TempJob)
        {
            return new HitableArray<Sphere>(5, allocator)
            {
                Objects =
                {
                    [0] = new Sphere(new float3(0f, 0f, -1f), 0.5f, 
                        new Material(MaterialType.Lambertian, new float3(0.1f, 0.2f, 0.5f))),
                    [1] = new Sphere(new float3(0f, -100.5f, -1f), 100f,
                        new Material(MaterialType.Lambertian, new float3(0.8f, 0.8f, 0.0f))),
                    [2] = new Sphere(new float3(1f, 0f, -1f), 0.5f,
                        new Material(MaterialType.Metal, new float3(0.8f, 0.6f, 0.2f))),
                    [3] = new Sphere(new float3(-1f, 0f, -1f), 0.5f,
                        new Material(MaterialType.Dielectric, new float3(), 0f, 1.5f)),
                    [4] = new Sphere(new float3(-1f, 0f, -1f), -0.45f,
                        new Material(MaterialType.Dielectric, new float3(), 0f, 1.5f))
                }
            };
        }
        
        public static HitableArray<Sphere> DozenVaryingSizeAndMaterial(Allocator allocator = Allocator.TempJob)
        {
            var smallFuzz = 0.08f;
            return new HitableArray<Sphere>(14, allocator)
            {
                Objects =
                {
                    [0] = new Sphere(new float3(0f, -0.03f, -0.98f), 0.45f, 
                        new Material(MaterialType.Metal, new float3(0.3f, 0.3f, 0.6f), 0.07f)),
                    [1] = new Sphere(new float3(0f, -100.5f, -1f), 100f,
                        new Material(MaterialType.Lambertian, new float3(0.4f, 0.6f, 0.4f))),
                    [2] = new Sphere(new float3(1f, 0f, -1f), 0.3f,
                        new Material(MaterialType.Metal, new float3(0.6f, 0.7f, 0.75f), 0.04f)),
                    [3] = new Sphere(new float3(-1.05f, 0f, -1f), 0.35f,
                        new Material(MaterialType.Metal, new float3(1f, 0.7f, 0.8f), 0.2f)),
                    [4] = new Sphere(new float3(-0.33f, -0.425f, -0.6f), 0.075f,
                        new Material(MaterialType.Metal, new float3(0.1f, 0.4f, 0.8f), smallFuzz)),
                    [5] = new Sphere(new float3(-1.05f, -0.46f, -0.6f), 0.04f,
                        new Material(MaterialType.Metal, new float3(0.1f, 0.6f, 0.7f), smallFuzz)),
                    [6] = new Sphere(new float3(1.06f, -0.47f, -0.6f), 0.03f,
                        new Material(MaterialType.Metal, new float3(0.5f, 0.0f, 0.7f), smallFuzz)),
                    [7] = new Sphere(new float3(0.8f, -0.45f, -0.58f), 0.05f,
                        new Material(MaterialType.Metal, new float3(0.1f, 0.8f, 0.4f), smallFuzz)),
                    [8] = new Sphere(new float3(0.64f, -0.475f, -0.65f), 0.025f,
                        new Material(MaterialType.Metal, new float3(0.0f, 0.2f, 0.8f), smallFuzz)),
                    [9] = new Sphere(new float3(0.5f, -0.475f, -0.75f), 0.025f,
                        new Material(MaterialType.Metal, new float3(0.0f, 0.1f, 0.9f), smallFuzz)),
                    [10] = new Sphere(new float3(-0.65f, -0.485f, -0.7f), 0.015f,
                        new Material(MaterialType.Metal, new float3(0.6f, 0.0f, 0.9f), smallFuzz)),
                    [11] = new Sphere(new float3(0.25f, -0.485f, -0.6f), 0.015f,
                        new Material(MaterialType.Metal, new float3(0.7f, 0.0f, 1f), smallFuzz)),
                    [12] = new Sphere(new float3(-0.7f, -0.4875f, -0.58f), 0.0125f,
                        new Material(MaterialType.Metal, new float3(0.6f, 0.0f, 0.2f), smallFuzz)),
                    [13] = new Sphere(new float3(-0.75f, -0.25f, -0.575f), 0.025f,
                        new Material(MaterialType.Metal, new float3(0.6f, 0.0f, 0.2f), smallFuzz))
                }
            };
        }
    }
}