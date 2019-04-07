using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace RayTracingWeekend
{
    public class ChapterSevenAlternate : Chapter<float4>
    {
        public int numberOfSamples;
        public float absorbRate;

        JobHandle m_Handle;
        int m_CanvasScale = 4;

        [BurstCompile]
        public struct AllSamplesJob : IJobParallelFor
        {
            public float absorbRate;
            public int maxHits;
            public int2 size;
            public int numberOfSamples;
            public Random random;
            public CameraFrame camera;
            
            [ReadOnly] public HitableArray<Sphere> World;
            [WriteOnly] public NativeArray<float4> Pixels;
            
            int recursionCounter;

            public void Execute(int index)
            {
                var nx = (float) size.x;
                var ny = (float) size.y;
                var i = index % size.x;
                var j = (index - i) / nx;

                float3 sum = new float3();
                for (int s = 0; s < numberOfSamples; s++)
                {
                    float u = (i + random.NextFloat()) / nx;
                    float v = (j + random.NextFloat()) / ny;
                    Ray r = camera.GetRay(u, v);
                
                    recursionCounter = 0;
                    sum += Color(r, World);
                }

                var av = sum / numberOfSamples;
                Pixels[index] = new float4(av.x, av.y, av.z, 1f);
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

            public float3 Color(Ray r, HitableArray<Sphere> world)
            {
                var rec = new HitRecord();
                if (recursionCounter < maxHits && world.Hit(r, 0.001f, float.MaxValue, ref rec))
                {
                    recursionCounter++;
                    var target = rec.p + rec.normal + RandomInUnitSphere();
                    return absorbRate * Color(new Ray(rec.p, target - rec.p), world);
                }
                
                float3 unitDirection = math.normalize(r.direction);
                var t = 0.5f * (unitDirection.y + 1f);
                return (1f - t) * Constants.one + t * Constants.blueGradient;
            }
        }


        public int canvasScale
        {
            get => m_CanvasScale = 8;
            set => m_CanvasScale = value;
        }

        public override void DrawToTexture()
        {
            ScaleTexture(canvasScale, TextureFormat.RGBAFloat);
            var spheres = ExampleSphereSets.FourVaryingSize();
            
            var rand = new Random();
            rand.InitState();

            var job = new AllSamplesJob()
            {
                absorbRate = absorbRate,
                maxHits = 32,
                camera = CameraFrame.Default,
                numberOfSamples = numberOfSamples,
                random = rand,
                size = Constants.ImageSize * canvasScale,
                World = spheres,
                Pixels = GetBuffer(Allocator.TempJob, canvasScale)
            };
            
            m_Handle = job.Schedule(job.Pixels.Length, 64, m_Handle);
            m_Handle.Complete();            
            
            texture.LoadAndApply(job.Pixels);
            spheres.Dispose();
        }
    }
}