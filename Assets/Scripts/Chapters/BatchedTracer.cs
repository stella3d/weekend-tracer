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
        
        // TODO - make this an option with 2 / 4 / 6 / 8
        int m_JobCount = 10;

        NativeArray<JobHandle> m_BatchHandles;

        public HitableArray<Sphere> Spheres;
        
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

            public void Execute()
            {
                var nx = (float) size.x;
                var ny = (float) size.y;
                for (float j = 0; j < size.y; j++)
                {
                    var rowIndex = j * nx;
                    for (float i = 0; i < size.x; i++)
                    {
                        var index = (int) (rowIndex + i);
                        float u = (i + random.NextFloat()) / nx;
                        float v = (j + random.NextFloat()) / ny;
                        Ray r = camera.GetRay(u, v, random);
                        Pixels[index] = math.sqrt(Color(r, World, 0));
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
                    if (depth < 50)
                    {
                        if (r.Scatter(rec, ref attenuation, ref scattered, ref random))
                            return attenuation * Color(scattered, world, depth + 1);
                    }
                    else
                    {
                        // this ray has run out of bounces - draw a black pixel
                        return new float3();
                    }
                }

                // the ray didn't hit anything, so draw the background color for this pixel
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
                    var rowIndex = j * nx;
                    for (float i = 0; i < size.x; i++)
                    {
                        var index = (int) (rowIndex + i);
                        float u = (i + random.NextFloat()) / nx;
                        float v = (j + random.NextFloat()) / ny;
                        Ray r = camera.GetRay(u, v);
                        Pixels[index] =  math.sqrt(Color(r, World, 0));
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
                    if (depth < 50)
                    {
                        if (r.Scatter(rec, ref attenuation, ref scattered, ref random))
                            return attenuation * Color(scattered, world, depth + 1);
                    }
                    else
                    {
                        return new float3();
                    }
                }

                return Utils.BackgroundColor(ref r);
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
                    size = Constants.DefaultImageSize * canvasScale,
                    World = Spheres,
                    Pixels = m_BatchBuffers[i],
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
                    size = Constants.DefaultImageSize * canvasScale,
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