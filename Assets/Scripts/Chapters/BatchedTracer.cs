using System;
using System.Collections;
using Unity.Burst;
using Unity.Collections;
using Unity.EditorCoroutines.Editor;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace RayTracingWeekend
{
    public class BatchedTracer : Chapter<float4>, IDisposable
    {
        public int numberOfSamples;

        public NativeArray<float4> m_TextureBuffer;

        public NativeArray<float3>[] m_BatchBuffers;
        
        int m_JobCount = 10;

        NativeArray<JobHandle> m_BatchHandles;

        public HitableArray<Sphere> Spheres;
        
#if DEBUG_RAYS
        public NativeArray<Ray> InitialRays;
#endif

        public JobHandle m_Handle;
        
        public int canvasScale { get; set; }
        public float fieldOfView { get; set; }
        public CameraFrame camera { get; set; }

        public int CompletedSampleCount { get; private set; }
        
        public BatchedTracer(HitableArray<Sphere> spheres, CameraFrame camera, int canvasScale = 4)
        {
            this.canvasScale = canvasScale;
            this.camera = camera;
            Spheres = spheres;
            Setup();
        }

        ~BatchedTracer()
        {
            Dispose();
        }

        internal override void Setup()
        {
            //Dispose();
            ScaleTexture(canvasScale, TextureFormat.RGBAFloat);
            var length = texture.height * texture.width;

            m_BatchHandles = new NativeArray<JobHandle>(m_JobCount, Allocator.Persistent);
            m_TextureBuffer = new NativeArray<float4>(length, Allocator.Persistent);

            m_BatchBuffers = new NativeArray<float3>[m_JobCount];
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
            
#if DEBUG_RAYS
            [WriteOnly] public NativeArray<Ray> InitialRays;
#endif

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
#if DEBUG_RAYS
                        InitialRays[index] = r;
#endif
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
                                if (MetalMaterial.Scatter(r, rec, random, ref attenuation, ref scattered))
                                {
                                    return attenuation * Color(scattered, world, depth + 1);
                                }
                                break;
                            case MaterialType.Dielectric:
                                // The dark outline bug was fixed by adding this line that changes the state of the RNG.
                                // DON'T REMOVE
                                random.NextFloat();
                                if (Utils.DielectricScatter(random, r, rec, ref attenuation, ref scattered))
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
                        Pixels[index] =  math.sqrt(Color(r, World, 0));
                    }
                }
            }

            public float3 GammaColor(float3 linearColor)
            {
                return math.sqrt(linearColor);
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
                                if (MetalMaterial.Scatter(r, rec, random, ref attenuation, ref scattered))
                                {
                                    return attenuation * Color(scattered, world, depth + 1);
                                }
                                break;
                            case MaterialType.Dielectric:
                                // The dark outline bug was fixed by adding this line that changes the state of the RNG.
                                // IF THIS IS REMOVED IT BREAKS AGAIN EVEN IF YOU CALL .NextFloat() inside Scatter()
                                random.NextFloat();
                                if (Utils.DielectricScatter(random, r, rec, ref attenuation, ref scattered))
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

        struct ClearAccumulatedJob<T> : IJobParallelFor where T: struct
        {
            [WriteOnly] public NativeArray<T> Buffer;
            
            public void Execute(int index)
            {
                Buffer[index] = default;
            }
        }

        public override void DrawToTexture()
        {
            if (clearOnDraw)
            {
                CompletedSampleCount = 0;
                var clearJob = new ClearAccumulatedJob<float4> { Buffer = m_TextureBuffer };
                m_Handle = clearJob.Schedule(m_TextureBuffer.Length, 4096, m_Handle);
            }
            
            for (int i = 0; i < m_JobCount; i++)
            {
                var rand = new Random();
                rand.InitState((uint)i + (uint)CompletedSampleCount + 100);
                var job = new SerialJobWithFocus()
                {
                    camera = camera,
                    random = rand,
                    size = Constants.ImageSize * canvasScale,
                    World = Spheres,
                    Pixels = m_BatchBuffers[i],
#if DEBUG_RAYS
                    InitialRays = InitialRays
#endif
                };

                m_BatchHandles[i] = job.Schedule(m_Handle);
            }

            var combineJob = new CombineJobTen(m_BatchBuffers, m_TextureBuffer, CompletedSampleCount);

            var batchHandle = JobHandle.CombineDependencies(m_BatchHandles);
            
            m_Handle = combineJob.Schedule(combineJob.In1.Length, 1024, batchHandle);
            m_Handle.Complete();

            CompletedSampleCount += m_JobCount;
            texture.LoadAndApply(m_TextureBuffer, false);
        }
        
        public bool clearOnDraw { get; set; }
        
        
        public void DrawToTextureWithoutFocus()
        {
            if (clearOnDraw)
            {
                CompletedSampleCount = 0;
                var clearJob = new ClearAccumulatedJob<float4> { Buffer = m_TextureBuffer };
                m_Handle = clearJob.Schedule(m_TextureBuffer.Length, 4096, m_Handle);
            }

            for (int i = 0; i < m_JobCount; i++)
            {
                var rand = new Random();
                rand.InitState((uint)i + (uint)CompletedSampleCount + 100);
                var job = new SerialJob()
                {
                    camera = camera,
                    random = rand,
                    size = Constants.ImageSize * canvasScale,
                    World = Spheres,
                    Pixels = m_BatchBuffers[i]
                };

                m_BatchHandles[i] = job.Schedule(m_Handle);
            }

            var combineJob = new CombineJobTen(m_BatchBuffers, m_TextureBuffer, CompletedSampleCount);

            var batchHandle = JobHandle.CombineDependencies(m_BatchHandles);
            
            m_Handle = combineJob.Schedule(combineJob.In1.Length, 1024, batchHandle);
            m_Handle.Complete();

            CompletedSampleCount += m_JobCount;
            texture.LoadAndApply(m_TextureBuffer, false);
        }

        float lastBatchTime;
        public EditorCoroutine Routine { get; set; }
        
        // TODO - refactor this to not wait synchronously on the jobs
        public IEnumerator BatchCoroutine(int count, Action onBatchComplete, float cycleTime = 0.64f)
        {
            // TODO - name the magic number
            if (Time.time - lastBatchTime < cycleTime)
                yield return null;

            for (int i = 0; i < count / m_JobCount; i++)
            {
                DrawToTexture();
                onBatchComplete();
                lastBatchTime = Time.time;
                yield return null;
            }
            
            if(Routine != null)
                EditorCoroutineUtility.StopCoroutine(Routine);
        }
        
        public IEnumerator BatchCoroutineNoFocus(int count, Action onBatchComplete, float cycleTime = 0.64f)
        {
            // TODO - name the magic number
            if (Time.time - lastBatchTime < cycleTime)
                yield return null;

            for (int i = 0; i < count / m_JobCount; i++)
            {
                DrawToTextureWithoutFocus();
                onBatchComplete();
                lastBatchTime = Time.time;
                yield return null;
            }
            
            if(Routine != null)
                EditorCoroutineUtility.StopCoroutine(Routine);
        }

        public void Dispose()
        {
            Spheres.Dispose();
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