using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace RayTracingWeekend
{
    // TODO - rename things so that this powers chapters from 8 to the end
    public class BatchedTracer : Chapter<float4>, IDisposable
    {
        public int numberOfSamples;

        public NativeArray<float4> m_TextureBuffer;

        public NativeArray<float3>[] m_BatchBuffers = new NativeArray<float3>[8];
        
        int m_JobCount = 8;

        NativeArray<JobHandle> m_BatchHandles;
        
        JobHandle m_Handle;
        
        public int canvasScale { get; set; }
        public float fieldOfView { get; set; }

        public const int BatchSampleCount = 16;
        public int CompletedSampleCount { get; private set; }
        
        public BatchedTracer()
        {
            Setup();
        }

        ~BatchedTracer()
        {
            Dispose();
        }

        internal override void Setup()
        {
            Dispose();
            m_Spheres = ExampleSphereSets.FiveWithDielectric(Allocator.Persistent);
            // TODO - fix the scaling setup
            var i = canvasScale == 0 ? canvasScale = 6 : canvasScale = canvasScale;
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
        public struct SerialJobWithFocus : IJob
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
                        Ray r = camera.GetRay(u, v, random);
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

        CameraFrame m_CameraFrame = CameraFrame.Default;
        HitableArray<Sphere> m_Spheres;

        CameraFrame GetChapterTenCamera()
        {
            var lookFrom = new float3(-2f, 2f, 1f);
            var lookAt = new float3(0f, 0f, -1f);
            var up = new float3(0f, 1f, 0f);
            float fov = 90f;
            float aspectRatio = texture.width / (float)texture.height;
            var frame = new CameraFrame(lookFrom, lookAt, up, fov, aspectRatio);
            return frame;
        }
        
        CameraFrame GetChapterElevenCamera()
        {
            var lookFrom = new float3(3f, 3f, 2f);
            var lookAt = new float3(0f, 0f, -1f);
            var distToFocus = math.length(lookFrom - lookAt);
            var aperture = 2f;
            var up = new float3(0f, 1f, 0f);
            float fov = 20f;
            float aspectRatio = texture.width / (float)texture.height;
            var frame = new CameraFrame(lookFrom, lookAt, up, fov, aspectRatio, aperture, distToFocus);
            return frame;
        }

        
        public override void DrawToTexture()
        {
            m_CameraFrame = GetChapterElevenCamera();

            for (int i = 0; i < m_JobCount; i++)
            {
                var rand = new Random();
                rand.InitState((uint)i + (uint)CompletedSampleCount + 100);
                var job = new SerialJobWithFocus()
                {
                    camera = m_CameraFrame,
                    random = rand,
                    size = Constants.ImageSize * canvasScale,
                    World = m_Spheres,
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
        }

        public void Dispose()
        {
            m_Spheres.Dispose();
            foreach (var b in m_BatchBuffers)
            {
                if(b.IsCreated)
                    b.Dispose();
            }
            if(m_TextureBuffer.IsCreated)
                m_TextureBuffer.Dispose();
            if(m_BatchHandles.IsCreated)
                m_BatchHandles.Dispose();
        }
    }
}