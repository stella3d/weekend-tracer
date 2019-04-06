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
        public struct Job : IJob
        {
            public float absorbRate;
            
            public int maxHits;
            
            public int2 size;

            public int numberOfSamples;

            public Random random;

            public CameraFrame camera;
            
            [ReadOnly] public HitableArray<Sphere> World;
            
            [WriteOnly] public NativeArray<Color24> Pixels;

            public void Execute()
            {
                var nx = (float) size.x;
                var ny = (float) size.y;
                for (float j = 0; j < size.y; j++)
                {
                    for (float i = 0; i < size.x; i++)
                    {
                        var index = (int) (j * nx + i);
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
                }
            }

            float3 RandomInUnitSphere()
            {
                var r = random;
                float3 p;
                float3 one = new float3(1f, 1f, 1f);
                do
                {
                    p = 2f * new float3(r.NextFloat(), r.NextFloat(), r.NextFloat()) - one;
                } 
                while (p.x * p.x + p.y * p.y + p.z * p.z >= 1.0f);
                return p;
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

        int m_CanvasScale = 4;

        public override void DrawToTexture()
        {
            ScaleTexture(m_CanvasScale);
            var spheres = ExampleSphereSets.FourVaryingSizeAndMaterial();
            var metal1 = spheres.Objects[2];
            metal1.material.fuzziness = fuzzinessOne;
            spheres.Objects[2] = metal1;
            var metal2 = spheres.Objects[3];
            metal2.material.fuzziness = fuzzinessTwo;
            spheres.Objects[3] = metal2;
            
            var rand = new Random();
            rand.InitState();

            var job = new Job()
            {
                absorbRate = absorbRate,
                maxHits = 50,
                camera = CameraFrame.Default,
                numberOfSamples = numberOfSamples,
                random = rand,
                size = Constants.ImageSize * m_CanvasScale,
                World = spheres,
                Pixels = GetBuffer(Allocator.TempJob, m_CanvasScale)
            };
            
            job.Run();
            texture.LoadAndApply(job.Pixels);
            spheres.Dispose();
        }
    }
}