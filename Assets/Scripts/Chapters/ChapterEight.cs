using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace RayTracingWeekend
{
    public class ChapterEight : Chapter<float4>
    {
        public int numberOfSamples;

        [BurstCompile]
        public struct Job : IJobParallelFor
        {
            public int2 size;
            public int numberOfSamples;
            public Random random;
            public CameraFrame camera;
            
            [ReadOnly] public HitableArray<Sphere> World;
            [WriteOnly] public NativeArray<float4> Pixels;

            public void Execute(int index)
            {
                var nx = (float) size.x;
                var ny = (float) size.y;
                var x = index % size.x;
                var y = (index - x) / nx;
                float3 col = new float3();
                float previousRandomV = random.NextFloat();
                for (var s = 0; s < numberOfSamples; s++)
                {
                    float u = (x + previousRandomV) / nx;
                    var randomV = random.NextFloat();
                    float v = (y + randomV) / ny;
                    previousRandomV = randomV;
                    Ray r = camera.GetRay(u, v);
                    col += Color(r, World, 0);
                }

                col /= numberOfSamples;
                Pixels[index] = new float4(col.x, col.y, col.z, 1f);
            }
            
            
            public float3 Color(Ray r, HitableArray<Sphere> world, int depth)
            {
                var rec = new HitRecord();
                if (world.Hit(r, 0.001f, float.MaxValue, ref rec))
                {
                    Ray scattered = new Ray();
                    float3 attenuation = new float3();
                    var albedo = rec.material.albedo;
                    if (depth < 50)
                    {
                        switch (rec.material.type)
                        {
                            case MaterialType.Lambertian:
                                if (DiffuseMaterial.Scatter(random, albedo,
                                    r, rec, ref attenuation, ref scattered))
                                {
                                    return attenuation * Color(scattered, world, depth + 1);
                                }
                                break;
                            case MaterialType.Metal:
                                if (MetalMaterial.Scatter(rec.material,
                                    r, rec, random, ref attenuation, ref scattered))
                                {
                                    return attenuation * Color(scattered, world, depth + 1);
                                }
                                break;
                        }
                    }
                    else
                    {
                        return new float3();
                    }
                }
                
                float3 unitDirection = math.normalize(r.direction);
                var t = 0.5f * (unitDirection.y + 1f);
                return (1f - t) * Constants.one + t * Constants.blueGradient;
            }
        }

        public int canvasScale { get; set; }

        JobHandle m_Handle;
        
        public override void DrawToTexture()
        {
            ScaleTexture(canvasScale, TextureFormat.RGBAFloat);
            
            var spheres = ExampleSphereSets.DozenVaryingSizeAndMaterial();
            var rand = new Random();
            rand.InitState();

            var job = new Job()
            {
                camera = CameraFrame.Default,
                numberOfSamples = numberOfSamples,
                random = rand,
                size = Constants.ImageSize * canvasScale,
                World = spheres,
                Pixels = GetBuffer(Allocator.Persistent, canvasScale)
            };
            
            m_Handle = job.Schedule(job.Pixels.Length, 256, m_Handle);
            m_Handle.Complete();
            
            texture.LoadAndApply(job.Pixels);
            spheres.Dispose();
        }
    }
}