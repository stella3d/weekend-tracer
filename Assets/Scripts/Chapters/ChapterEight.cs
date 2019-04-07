using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace RayTracingWeekend
{
    public class ChapterEight : Chapter<Color24>
    {
        public int numberOfSamples;
        public float absorbRate;

        public float fuzzinessOne;
        public float fuzzinessTwo;
        
        [BurstCompile]
        public struct Job : IJobParallelFor
        {
            public int maxHits;
            public int2 size;
            public int numberOfSamples;
            public Random random;
            public CameraFrame camera;
            
            [ReadOnly] public HitableArray<Sphere> World;
            [WriteOnly] public NativeArray<Color24> Pixels;

            public void Execute(int index)
            {
                var nx = (float) size.x;
                var ny = (float) size.y;
                var i = index % size.x;
                var j = (index - i) / nx;

                float3 col = new float3();
                for (int s = 0; s < numberOfSamples; s++)
                {
                    float u = (i + random.NextFloat()) / nx;
                    float v = (j + random.NextFloat()) / ny;
                    Ray r = camera.GetRay(u, v);
                    recursionCounter = 0;
                    col += Color(r, World, 0);
                }

                col /= (float)numberOfSamples;
                Pixels[index] = col.ToRgb24();
            }
            
            // implementing as a straight translation of the C++ gave me a stack overflow
            int recursionCounter;
            
            public MetalMaterial metal;
            public DiffuseMaterial diffuse;

            public float3 Color(Ray r, HitableArray<Sphere> world, int depth)
            {
                var rec = new HitRecord();
                if (recursionCounter < maxHits && world.Hit(r, 0.001f, float.MaxValue, ref rec))
                {
                    recursionCounter++;
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
            ScaleTexture(canvasScale);
            
            var spheres = ExampleSphereSets.DozenVaryingSizeAndMaterial();
            /*
            var metal1 = spheres.Objects[2];
            metal1.material.fuzziness = fuzzinessOne;
            spheres.Objects[2] = metal1;
            var metal2 = spheres.Objects[3];
            metal2.material.fuzziness = fuzzinessTwo;
            spheres.Objects[3] = metal2;
            */
            
            var rand = new Random();
            rand.InitState();

            Utils.PositionNoise(rand, 0.05f, spheres);
            
            var job = new Job()
            {
                maxHits = 50,
                camera = CameraFrame.Default,
                numberOfSamples = numberOfSamples,
                random = rand,
                size = Constants.ImageSize * canvasScale,
                World = spheres,
                Pixels = GetBuffer(Allocator.TempJob, canvasScale)
            };
            
            //job.Run();
            m_Handle = job.Schedule(job.Pixels.Length, 256, m_Handle);
            m_Handle.Complete();
            
            texture.LoadAndApply(job.Pixels);
            spheres.Dispose();
        }
    }
}