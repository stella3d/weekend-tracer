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
    public class DebugTracerWithoutFocus : Chapter<float4>, IDisposable
    {
        public int numberOfSamples;

        public NativeArray<float4> m_TextureBuffer;
        public NativeArray<float3> m_TracerBuffer;
        
        public NativeArray<int> m_PerPixelRaySegmentCount;
        public NativeArray<Ray> m_RaySegments;

        int m_JobCount = 10;

        public HitableArray<Sphere> Spheres;
        
#if DEBUG_RAYS
        public NativeArray<Ray> InitialRays;
#endif

        public JobHandle m_Handle;
        
        public int canvasScale { get; set; }
        public float fieldOfView { get; set; }
        public CameraFrame camera { get; set; }

        public int CompletedSampleCount { get; private set; }
        
        public DebugTracerWithoutFocus(HitableArray<Sphere> spheres, CameraFrame camera, int canvasScale = 4)
        {
            this.canvasScale = canvasScale;
            this.camera = camera;
            Spheres = spheres;
            Setup();
        }

        ~DebugTracerWithoutFocus()
        {
            Dispose();
        }

        internal override void Setup()
        {
            //Dispose();
            ScaleTexture(canvasScale, TextureFormat.RGBAFloat);
            var length = texture.height * texture.width;

            m_TracerBuffer = new NativeArray<float3>(length, Allocator.Persistent);
            m_TextureBuffer = new NativeArray<float4>(length, Allocator.Persistent);
            
            m_PerPixelRaySegmentCount = new NativeArray<int>(length, Allocator.Persistent);
            m_RaySegments = new NativeArray<Ray>(length * 50, Allocator.Persistent);

            CompletedSampleCount = 0;
        }

        public struct Float3ToFloat4Job : IJobParallelFor
        {
            [ReadOnly] public NativeArray<float3> In;
            [WriteOnly] public NativeArray<float4> Out;

            public float aValue;
            
            public void Execute(int index)
            {
                var i = In[index];
                Out[index] = new float4(i.x, i.y, i.z, aValue);
            }
        }

        [BurstCompile]
        public struct SerialJobWithFocus : IJob
        {
            public int2 size;
            public Random random;
            public CameraFrame camera;

            [ReadOnly] public HitableArray<Sphere> World;
            [WriteOnly] public NativeArray<float3> Pixels;
            
            [WriteOnly] public NativeArray<int> PixelRaySegmentCount;
            [WriteOnly] public NativeArray<Ray> RaySegments;

            int m_PixelRaySegmentCounter;
            int m_RaySegmentIndex;
            
            public void Execute()
            {
                var nx = (float) size.x;
                var ny = (float) size.y;
                m_RaySegmentIndex = 0;
                for (float j = 0; j < size.y; j++)
                {
                    for (float i = 0; i < size.x; i++)
                    {
                        var index = (int) (j * nx + i);
                        float u = (i + random.NextFloat()) / nx;
                        float v = (j + random.NextFloat()) / ny;
                        Ray r = camera.GetRay(u, v, random);
                        
                        m_PixelRaySegmentCounter = 0;
                        RecordRaySegment(r);
                        
                        var color = Color(r, World, 0);
                        PixelRaySegmentCount[index] = m_PixelRaySegmentCounter;
                        
                        Pixels[index] = color;
                    }
                }
            }

            void RecordRaySegment(Ray segment)
            {
                RaySegments[m_RaySegmentIndex] = segment;
                m_RaySegmentIndex++;
                m_PixelRaySegmentCounter++;
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
                                    RecordRaySegment(scattered);
                                    return attenuation * Color(scattered, world, depth + 1);
                                }
                                break;
                            case MaterialType.Metal:
                                if (MetalMaterial.Scatter(r, rec, random, ref attenuation, ref scattered))
                                {
                                    RecordRaySegment(scattered);
                                    return attenuation * Color(scattered, world, depth + 1);
                                }
                                break;
                            case MaterialType.Dielectric:
                                if (Utils.DielectricScatter(random, r, rec, ref attenuation, ref scattered))
                                {
                                    RecordRaySegment(scattered);
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

        public override void DrawToTexture()
        {
            var rand = new Random();
            rand.InitState((uint)CompletedSampleCount + 100);
            var job = new SerialJobWithFocus()
            {
                camera = camera,
                random = rand,
                size = Constants.ImageSize * canvasScale,
                World = Spheres,
                Pixels = m_TracerBuffer,
                PixelRaySegmentCount = m_PerPixelRaySegmentCount,
                RaySegments = m_RaySegments,
            };

            var convertJob = new Float3ToFloat4Job
            {
                In = m_TracerBuffer,
                Out = m_TextureBuffer,
                aValue = 1f,
            };

            var batchHandle = job.Schedule();
            batchHandle = convertJob.Schedule(m_TracerBuffer.Length, 4096, batchHandle);
            batchHandle.Complete();

            CompletedSampleCount += m_JobCount;
            texture.LoadAndApply(m_TextureBuffer, false);
        }
        
        public bool clearOnDraw { get; set; }
      
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
                DrawToTexture();
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
            if(m_TracerBuffer.IsCreated)
                m_TextureBuffer.Dispose();
            if(m_TextureBuffer.IsCreated)
                m_TextureBuffer.Dispose();
        }
    }
}