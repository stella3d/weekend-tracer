using System;
using System.Collections;
using System.Linq;
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
        const int k_MaxJobsPerBatch = 10;
        static readonly string k_MaxJobsMessage =
            $"A max of {k_MaxJobsPerBatch} jobs per batch is supported by the batched tracer";
        
        static readonly int[] k_JobsPerBatchOptions = {1, 2, 4, 6, 8, 10};
        static readonly string k_AvailableOptionsMessage = "job count per batch must be one of: 1, 2, 4, 6, 8, 10";

        int m_JobsPerBatch = 4;

        public int jobsPerBatch
        {
            get { return m_JobsPerBatch; }
            set
            {
                if (value == m_JobsPerBatch)
                    return;
                if (value > k_MaxJobsPerBatch)
                    Debug.Log(k_MaxJobsMessage);
                else if (!k_JobsPerBatchOptions.Contains(value))
                    Debug.Log(k_AvailableOptionsMessage);
                else
                    m_JobsPerBatch = value;
            }
        }
        
        public CameraFrame camera { get; set; }

        public int CompletedSampleCount { get; private set; }
        
        float lastBatchTime;
        
        // TODO - move this out of the tracer class
        public EditorCoroutine Routine { get; set; }
        
        public NativeArray<float3>[] m_BatchBuffers;

        NativeArray<JobHandle> m_BatchHandles;

        public HitableArray<Sphere> Spheres;
        
        public JobHandle m_Handle;
        JobHandle m_DummyHandle;
        
        public BatchedTracer(HitableArray<Sphere> spheres, CameraFrame camera, int width, int height)
            : base(width, height, TextureFormat.RGBAFloat)
        {
            this.camera = camera;
            Spheres = spheres;
            Setup();
        }

        ~BatchedTracer()
        {
            Dispose();
        }

        internal void AllocateSampleJobBuffers(int pixelCount, Allocator allocator = Allocator.Persistent)
        {
            m_BatchHandles.DisposeIfCreated();
            m_BatchHandles = new NativeArray<JobHandle>(jobsPerBatch, allocator);

            if (m_BatchBuffers != null)
            {
                for (int j = 0; j < m_BatchBuffers.Length; j++)
                    m_BatchBuffers[j].DisposeIfCreated();
            }

            m_BatchBuffers = new NativeArray<float3>[jobsPerBatch];
            for (int j = 0; j < m_BatchBuffers.Length; j++)
                m_BatchBuffers[j] = new NativeArray<float3>(pixelCount, allocator);
        }

        public override void Setup()
        {
            m_DummyHandle = new JobHandle();
            m_DummyHandle.Complete();

            TextureFormat = TextureFormat.RGBAFloat;
            var length = texture.height * texture.width;
            PixelBuffer = texture.GetRawTextureData<float4>();

            AllocateSampleJobBuffers(length);
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
                random.InitState(random.state + uint.MaxValue / 10);
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
                        // the only difference between this and without focus is the GetRay call
                        Ray r = camera.GetRay(u, v, random);
                        var linearColor = RayMath.Color(r, World, 0, ref random);
                        Pixels[index] = math.sqrt(linearColor);
                    }
                }
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
                        var linearColor = RayMath.Color(r, World, 0, ref random);
                        Pixels[index] = math.sqrt(linearColor);
                    }
                }
            }
        }

        public void DrawToTexture()
        {
            for (int i = 0; i < jobsPerBatch; i++)
            {
                var rand = new Random();
                rand.InitState((uint)i + (uint)CompletedSampleCount + 100);
                var job = new SerialJobWithFocus()
                {
                    camera = camera,
                    random = rand,
                    size = texture.GetSize(),
                    World = Spheres,
                    Pixels = m_BatchBuffers[i],
                };

                m_BatchHandles[i] = job.Schedule(m_Handle);
            }

            // TODO - variable batch size combine job
            var combineJob = new CombineJobFour(m_BatchBuffers, PixelBuffer, CompletedSampleCount);

            var batchHandle = JobHandle.CombineDependencies(m_BatchHandles);
            
            m_Handle = combineJob.Schedule(combineJob.In1.Length, 1024, batchHandle);
            m_Handle.Complete();

            CompletedSampleCount += jobsPerBatch;
            texture.LoadAndApply(PixelBuffer, false);
        }

        public override JobHandle Schedule(JobHandle dependency = default)
        {
            // TODO - this?
            throw new NotImplementedException();
        }

        public void DrawToTextureWithoutFocus()
        {
            for (int i = 0; i < jobsPerBatch; i++)
            {
                var rand = new Random();
                rand.InitState((uint)i + (uint)CompletedSampleCount + 100);
                var job = new SerialJob()
                {
                    camera = camera,
                    random = rand,
                    size = texture.GetSize(),
                    World = Spheres,
                    Pixels = m_BatchBuffers[i]
                };

                m_BatchHandles[i] = job.Schedule(m_Handle);
            }

            var batchHandle = JobHandle.CombineDependencies(m_BatchHandles);
            m_Handle = Combine.ScheduleJob(m_BatchBuffers, PixelBuffer, CompletedSampleCount, batchHandle);
            
            // TODO - make this async
            m_Handle.Complete();

            CompletedSampleCount += jobsPerBatch;
            texture.LoadAndApply(PixelBuffer, false);
        }

        // TODO - refactor this to not wait synchronously on the jobs
        public IEnumerator BatchCoroutine(int count, Action onBatchComplete, float cycleTime = 0.64f)
        {
            yield return BatchCoroutine(count, DrawToTexture, onBatchComplete, cycleTime);
        }
        
        public IEnumerator BatchCoroutineNoFocus(int count, Action onBatchComplete, float cycleTime = 0.64f)
        {
            yield return BatchCoroutine(count, DrawToTextureWithoutFocus, onBatchComplete, cycleTime);
        }
        
        public IEnumerator BatchCoroutine(int count, Action draw, Action onBatchComplete, float cycleTime = 0.64f)
        {
            // TODO - name the magic number
            if (Time.time - lastBatchTime < cycleTime)
                yield return null;

            for (int i = 0; i < count / jobsPerBatch; i++)
            {
                draw();
                onBatchComplete();
                lastBatchTime = Time.time;
                yield return null;
            }
            
            if(Routine != null)
                EditorCoroutineUtility.StopCoroutine(Routine);
        }

        public override void Dispose()
        {
            Spheres.Dispose();
            foreach (var b in m_BatchBuffers)
                b.DisposeIfCreated();
            if(m_BatchHandles.IsCreated)
                m_BatchHandles.Dispose();
        }
    }
}