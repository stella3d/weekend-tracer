using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;

namespace RayTracingWeekend
{
    public static class ExampleSphereSets
    {
        public static HitableArray<Sphere> FourVaryingSize(Allocator allocator = Allocator.Persistent)
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

        public static HitableArray<Sphere> ChapterSixAndSeven(Allocator allocator = Allocator.Persistent)
        {
            return new HitableArray<Sphere>(2, allocator)
            {
                Objects =
                {
                    [0] = new Sphere(new float3(0f, 0f, -1f), 0.5f),
                    [1] = new Sphere(new float3(0f, -100.5f, -1f), 100f)
                }
            };
        }

        public static HitableArray<Sphere> ChapterEight(Allocator allocator = Allocator.Persistent)
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
        
        // this is the set used at the end of chapter 9
        public static HitableArray<Sphere> FiveWithDielectric(Allocator allocator = Allocator.Persistent)
        {
            return new HitableArray<Sphere>(5, allocator)
            {
                Objects =
                {
                    [0] = new Sphere(new float3(0f, 0f, -1f), 0.5f, 
                        new Material(MaterialType.Lambertian, new float3(0.1f, 0.2f, 0.5f))),
                    [1] = new Sphere(new float3(0f, -100.5f, -1f), 100f,
                        new Material(MaterialType.Lambertian, new float3(0.8f, 0.8f, 0f))),
                    [2] = new Sphere(new float3(1f, 0f, -1f), 0.5f,
                        new Material(MaterialType.Metal, new float3(0.8f, 0.6f, 0.2f), 0.3f)),
                    [3] = new Sphere(new float3(-1f, 0f, -1f), 0.5f,
                        new Material(MaterialType.Dielectric, float3.zero, 0f, 1.5f)),
                    [4] = new Sphere(new float3(-1f, 0f, -1f), -0.45f,
                        new Material(MaterialType.Dielectric, float3.zero, 0f, 1.5f))
                }
            };
        }
        
        public static HitableArray<Sphere> RandomScene(int n, uint seed = default, Allocator allocator = Allocator.Persistent)
        {
            var rng = new Random();
            rng.InitState(seed);
            var list = new HitableArray<Sphere>(n, allocator)
            {
                [0] = new Sphere(new float3(0, -1000, 0), 1000,
                    new Material(MaterialType.Lambertian, new float3(0.5f, 0.5f, 0.5f)))
            };

            var i = 1;
            float3 centerComparePoint = new float3(4f, 0.2f, 0f);
            for (int a = -11; a < 11; a++)
            {
                for (int b = -11; b < 11; b++)
                {
                    float chooseMat = rng.NextFloat();
                    float3 center = new float3(a+0.9f*rng.NextFloat(), 0.2f, b+0.9f*rng.NextFloat());
                    if (!(math.length(center - centerComparePoint) > 0.9f)) 
                        continue;
                    
                    if (chooseMat < 0.8f)        // diffuse
                    {
                        var diffuseMat = new Material(MaterialType.Lambertian, RandomFloat3(ref rng));
                        list[i++] = new Sphere(center, 0.2f, diffuseMat);
                    }
                    else if (chooseMat < 0.95f)        // metal
                    {
                        var metalMat = new Material(MaterialType.Metal, RandomFloat3(ref rng));
                        list[i++] = new Sphere(center, 0.2f, metalMat);
                    }
                    else                         // glass
                    {
                        var metalMat = new Material(MaterialType.Dielectric, RandomFloat3(ref rng));
                        list[i++] = new Sphere(center, 0.2f, metalMat);
                    }
                }
            }
            
            list[i++] = new Sphere(new float3(0, 1, 0), 1f,  
                new Material(MaterialType.Dielectric, float3.zero, 0f, 1.5f));
            list[i++] = new Sphere(new float3(-4, 1, 0), 1f,  
                new Material(MaterialType.Lambertian, new float3(0.4f, 0.2f, 0.1f), 0f, 1.5f));
            list[i] = new Sphere(new float3(4, 1, 0), 1f,  
                new Material(MaterialType.Metal, new float3(0.7f, 0.6f, 0.5f)));

            return list;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float3 RandomFloat3(ref Random rng)
        {
            return new float3(rng.NextFloat() * rng.NextFloat(),
                rng.NextFloat() * rng.NextFloat(),
                rng.NextFloat() * rng.NextFloat());
        }
    }
}