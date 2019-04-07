using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace RayTracingWeekend
{
    public class ChapterEightProgressive : Chapter<float4>, IDisposable
    {
        public int numberOfSamples;

        public NativeArray<float4> m_TextureBuffer;

        public NativeArray<float4>[] m_BatchBuffers = new NativeArray<float4>[8];

        public Texture2D BatchTexture;

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
                random.InitState(random.NextUInt());
                var nx = (float) size.x;
                var ny = (float) size.y;
                float x = index % size.x;
                float y = (index - x) / nx;
                float3 col = new float3();
                for (var s = 0; s < numberOfSamples; s++)
                {
                    float u = (x + random.NextFloat()) / nx;
                    float v = (y + random.NextFloat()) / ny;
                    Ray r = camera.GetRay(u, v);
                    col += Color(r, World, 0);
                }

                col *= (1f / numberOfSamples);
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
        
        [BurstCompile]
        public struct SerialJob : IJob
        {
            public int2 size;
            public Random random;
            public CameraFrame camera;
            
            [ReadOnly] public HitableArray<Sphere> World;
            [WriteOnly] public NativeArray<float4> Pixels;

            public void Execute()
            {
                random.InitState(random.NextUInt());
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
                        var col = Color(r, World, 0);
                        Pixels[index] = new float4(col.x, col.y, col.z, 1f);
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
        
        [BurstCompile]
        public struct CombineJob : IJobParallelFor
        {
            public int batchSampleCount;
            public int CompletedSampleCount;
            
            [ReadOnly]
            public NativeArray<float4> Current;
            public NativeArray<float4> Accumulated;
            
            public void Execute(int i)
            {
                var c = Current[i];
                var a = Accumulated[i];
                var cWeighted = c * batchSampleCount;
                var aWeighted = a * CompletedSampleCount / 2;
                var acc = (cWeighted + aWeighted) / (batchSampleCount + CompletedSampleCount / 2);
                Accumulated[i] = acc;
            }
        }
        
        [BurstCompile]
        public struct CombineJobEight : IJobParallelFor
        {
            public int CompletedSampleCount;
            
            [ReadOnly] public NativeArray<float4> In1;
            [ReadOnly] public NativeArray<float4> In2;
            [ReadOnly] public NativeArray<float4> In3;
            [ReadOnly] public NativeArray<float4> In4;
            [ReadOnly] public NativeArray<float4> In5;
            [ReadOnly] public NativeArray<float4> In6;
            [ReadOnly] public NativeArray<float4> In7;
            [ReadOnly] public NativeArray<float4> In8;
            
            public NativeArray<float4> Accumulated;
            
            public void Execute(int i)
            {
                var s1 = In1[i];
                var s2 = In2[i];
                var s3 = In3[i];
                var s4 = In4[i];
                var s5 = In5[i];
                var s6 = In6[i];
                var s7 = In7[i];
                var s8 = In8[i];
                var a = Accumulated[i];
                var aWeighted = a * CompletedSampleCount;
                var acc = (s1 + s2 + s3 + s4 + s5 + s6 + s7 + s8 + aWeighted) / (8 + CompletedSampleCount);
                Accumulated[i] = acc;
            }
        }

        public int canvasScale { get; set; }


        public const int BatchSampleCount = 16;
        int m_CompletedSampleCount;
        
        JobHandle m_Handle;

        public ChapterEightProgressive()
        {
            Setup();
        }

        ~ChapterEightProgressive()
        {
            Dispose(true);
        }

        internal override void Setup()
        {
            var i = canvasScale == 0 ? canvasScale = 12 : canvasScale = canvasScale;
            ScaleTexture(canvasScale, TextureFormat.RGBAFloat);
            BatchTexture = new Texture2D(texture.width, texture.height, texture.format, false);
            var length = texture.height * texture.width;
            m_BatchHandles = new NativeArray<JobHandle>(8, Allocator.Persistent);
            m_TextureBuffer = new NativeArray<float4>(length, Allocator.Persistent);

            for (int j = 0; j < m_BatchBuffers.Length; j++)
            {
                m_BatchBuffers[j] = new NativeArray<float4>(length, Allocator.Persistent);
            }

            m_CompletedSampleCount = 0;
        }

        int m_JobCount = 8;

        NativeArray<JobHandle> m_BatchHandles;

        public override void DrawToTexture()
        {
            var spheres = ExampleSphereSets.DozenVaryingSizeAndMaterial();

            for (int i = 0; i < m_JobCount; i++)
            {
                var rand = new Random();
                rand.InitState();
                rand.InitState((uint)i + (uint)m_CompletedSampleCount + 100);
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
                CompletedSampleCount = m_CompletedSampleCount,
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
            
            //m_Handle = job.Schedule(job.Pixels.Length, 1024, m_Handle);
            //m_Handle = combineJob.Schedule(combineJob.Current.Length, 1024, m_Handle);
            m_Handle = combineJob.Schedule(combineJob.In1.Length, 1024, batchHandle);
            m_Handle.Complete();

            m_CompletedSampleCount += m_JobCount;
            //BatchTexture.LoadAndApply(m_BatchBuffers[0], false);
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
                m_TextureBuffer.Dispose();
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