using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace RayTracingWeekend
{
    public class ChapterNineProgressive : Chapter<float4>, IDisposable
    {
        public int numberOfSamples;

        public NativeArray<float4> m_TextureBuffer;

        public NativeArray<float3>[] m_BatchBuffers = new NativeArray<float3>[8];
        
        int m_JobCount = 8;

        NativeArray<JobHandle> m_BatchHandles;
        
        JobHandle m_Handle;
        
        public int canvasScale { get; set; }

        public const int BatchSampleCount = 16;
        public int CompletedSampleCount { get; private set; }
        
        public ChapterNineProgressive()
        {
            Setup();
        }

        ~ChapterNineProgressive()
        {
            Dispose(true);
        }

        internal override void Setup()
        {
            Dispose();
            // TODO - fix the scaling setup
            var i = canvasScale == 0 ? canvasScale = 8 : canvasScale = canvasScale;
            ScaleTexture(canvasScale, TextureFormat.RGBAFloat);
            var length = texture.height * texture.width;
            m_BatchHandles = new NativeArray<JobHandle>(8, Allocator.Persistent);
            m_TextureBuffer = new NativeArray<float4>(length, Allocator.Persistent);

            for (int j = 0; j < m_BatchBuffers.Length; j++)
            {
                m_BatchBuffers[j] = new NativeArray<float3>(length, Allocator.Persistent);
            }

            CompletedSampleCount = 0;
        }

        [BurstCompile]
        public struct SerialJob : IJob
        {
            public int2 size;
            public Random random;
            public CameraFrame camera;
            
            [ReadOnly] public HitableArray<Sphere> World;
            [WriteOnly] public NativeArray<float3> Pixels;

            public void Execute()
            {
                var nx = (float) size.x;
                var ny = (float) size.y;
                for (float j = 0; j < size.y; j++)
                {
                    for (float i = 0; i < size.x; i++)
                    {
                        var index = (int) (j * nx + i);
                        float u = (i + random.NextFloat()) / nx;
                        float v = (j + random.NextFloat()) / ny;
                        Ray r = camera.GetRay(u, v);
                        Pixels[index] = Color(r, World, 0);
                    }
                }
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
                            // TODO - put this switch inside a static Material.Scatter() method ?
                            // also TODO - make the scatter API the same across types
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
                            case MaterialType.Dielectric:
                                if (Utils.DielectricScatter(random, rec.material.refractionIndex,
                                    r, rec, ref attenuation, ref scattered))
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

                return Utils.BackgroundColor(ref r);
            }
        }
        
        [BurstCompile]
        public struct CombineJobEight : IJobParallelFor
        {
            public int CompletedSampleCount;
            
            [ReadOnly] public NativeArray<float3> In1;
            [ReadOnly] public NativeArray<float3> In2;
            [ReadOnly] public NativeArray<float3> In3;
            [ReadOnly] public NativeArray<float3> In4;
            [ReadOnly] public NativeArray<float3> In5;
            [ReadOnly] public NativeArray<float3> In6;
            [ReadOnly] public NativeArray<float3> In7;
            [ReadOnly] public NativeArray<float3> In8;
            
            public NativeArray<float4> Accumulated;
            
            public void Execute(int i)
            {
                var sum = In1[i] + In2[i] + In3[i] + In4[i] + In5[i] + In6[i] + In7[i] + In8[i];
                var sumPixel = new float4(sum.x, sum.y , sum.z, 1f);
                
                var a = Accumulated[i];
                var aWeighted = a * CompletedSampleCount;
                var acc = (sumPixel + aWeighted) / (8 + CompletedSampleCount);
                Accumulated[i] = acc;
            }
        }

        public override void DrawToTexture()
        {
            var spheres = ExampleSphereSets.DozenVaryingSizeAndMaterial();

            for (int i = 0; i < m_JobCount; i++)
            {
                var rand = new Random();
                rand.InitState((uint)i + (uint)CompletedSampleCount + 100);
                var job = new SerialJob()
                {
                    camera = CameraFrame.Default,
                    random = rand,
                    size = Constants.ImageSize * canvasScale,
                    World = spheres,
                    Pixels = m_BatchBuffers[i]
                };

                m_BatchHandles[i] = job.Schedule(m_Handle);
            }

            var combineJob = new CombineJobEight()
            {
                CompletedSampleCount = CompletedSampleCount,
                In1 = m_BatchBuffers[0],
                In2 = m_BatchBuffers[1],
                In3 = m_BatchBuffers[2],
                In4 = m_BatchBuffers[3],
                In5 = m_BatchBuffers[4],
                In6 = m_BatchBuffers[5],
                In7 = m_BatchBuffers[6],
                In8 = m_BatchBuffers[7],
                Accumulated = m_TextureBuffer
            };

            var batchHandle = JobHandle.CombineDependencies(m_BatchHandles);
            
            m_Handle = combineJob.Schedule(combineJob.In1.Length, 1024, batchHandle);
            m_Handle.Complete();

            CompletedSampleCount += m_JobCount;
            texture.LoadAndApply(m_TextureBuffer, false);
            spheres.Dispose();
        }

        void Dispose(bool disposing)
        {
            foreach (var b in m_BatchBuffers)
            {
                if(b.IsCreated)
                    b.Dispose();
            }
            if (disposing)
            {
                if(m_TextureBuffer.IsCreated)
                    m_TextureBuffer.Dispose();
                if(m_BatchHandles.IsCreated)
                    m_BatchHandles.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}