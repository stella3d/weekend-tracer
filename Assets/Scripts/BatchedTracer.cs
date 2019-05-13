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
    public class BatchedTracer : Chapter<float4>
    {
        const int k_MaxJobsPerBatch = 10;
        static readonly string k_MaxJobsMessage =
            $"A max of {k_MaxJobsPerBatch} jobs per batch is supported by the batched tracer";
        
        static readonly int[] k_JobsPerBatchOptions = {1, 2, 4, 6, 8, 10};
        static readonly string k_AvailableOptionsMessage = "job count per batch must be one of: 1, 2, 4, 6, 8, 10";

        int m_JobsPerBatch = Utils.GetJobCount();

        public int JobsPerBatch
        {
            get => m_JobsPerBatch;
            set
            {
                if (value == m_JobsPerBatch || value % 2 != 0)
                    return;
                if (value > k_MaxJobsPerBatch)
                    Debug.Log(k_MaxJobsMessage);
                else if (!k_JobsPerBatchOptions.Contains(value))
                    Debug.Log(k_AvailableOptionsMessage);
                else
                {
                    m_JobsPerBatch = value;
                    AllocateSampleJobBuffers(texture.width * texture.height);
                }
            }
        }
        
        public CameraFrame camera { get; set; }

        public int CompletedSampleCount { get; private set; }
        
        public EditorCoroutine Routine { get; set; }
        
        public NativeArray<float3>[] m_BatchBuffers;

        NativeArray<JobHandle> m_BatchHandles;

        public HitableArray<Sphere> Spheres;
        
        public JobHandle m_Handle;
        JobHandle m_DummyHandle;

        float lastBatchTime;
        
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
            m_BatchHandles = new NativeArray<JobHandle>(m_JobsPerBatch, allocator);

            if (m_BatchBuffers != null)
                for (int j = 0; j < m_BatchBuffers.Length; j++)
                    m_BatchBuffers[j].DisposeIfCreated();
            

            // in sample jobs, we only calculate RGB, no alpha, so use float3 instead of float4
            m_BatchBuffers = new NativeArray<float3>[m_JobsPerBatch];
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
        
        // trace rays with support for defocus blur.  chapters 11 & 12
        [BurstCompile]
        public struct SampleJobWithFocus : IJob
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

        // trace rays without support for defocus blur.  chapters 8,9, & 10
        [BurstCompile]
        public struct SampleJobWithoutDefocus : IJob
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
            m_Handle = ScheduleBatch(ScheduleFocusJob);
            CompleteAndDraw();
        }

        public override JobHandle Schedule(JobHandle dependency = default)
        {
            return ScheduleBatch(ScheduleFocusJob);
        }

        // schedule a job without support for defocus blur.  chapters 8, 9, 10
        JobHandle ScheduleNoFocusJob(int batchIndex, JobHandle dependency)
        {
            var rand = new Random();
            rand.InitState((uint)batchIndex + (uint)CompletedSampleCount + 100);
            var job = new SampleJobWithoutDefocus()
            {
                camera = camera,
                random = rand,
                size = texture.GetSize(),
                World = Spheres,
                Pixels = m_BatchBuffers[batchIndex]
            };
            
            return job.Schedule(dependency);
        }
        
        // schedule a job with support for defocus blur.  chapters 11 & 12
        JobHandle ScheduleFocusJob(int batchIndex, JobHandle dependency)
        {
            var rand = new Random();
            rand.InitState((uint)batchIndex + (uint)CompletedSampleCount + 100);
            var job = new SampleJobWithFocus()
            {
                camera = camera,
                random = rand,
                size = texture.GetSize(),
                World = Spheres,
                Pixels = m_BatchBuffers[batchIndex]
            };
            
            return job.Schedule(dependency);
        }

        public bool ClearOnDraw;

        public JobHandle ScheduleBatch(Func<int, JobHandle, JobHandle> scheduleSingle)
        {
            if (ClearOnDraw)        // used for interactive mode - clear buffer every re-draw if interactive
            {
                var clearJob = new ClearAccumulatedJob<float4> { Buffer = PixelBuffer };
                m_Handle = clearJob.Schedule(PixelBuffer.Length, 4096, m_Handle);
                CompletedSampleCount = 0;
            }

            for (int i = 0; i < m_JobsPerBatch; i++)
            {
                // schedule a single sample job for the whole image
                m_BatchHandles[i] = scheduleSingle(i, m_Handle);
            }
            
            var batchHandle = JobHandle.CombineDependencies(m_BatchHandles);
            return Combine.ScheduleJob(m_BatchBuffers, PixelBuffer, CompletedSampleCount, batchHandle);
        }

        public void DrawToTextureWithoutFocus()
        {
            m_Handle = ScheduleBatch(ScheduleNoFocusJob);
            CompleteAndDraw();
        }
        
        public void CompleteAndDraw()
        {
            m_Handle.Complete();
            CompletedSampleCount += m_JobsPerBatch;
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

            for (int i = 0; i < count / m_JobsPerBatch; i++)
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