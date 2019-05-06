using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace RayTracingWeekend
{
    public class ChapterSeven : Chapter<Color24>
    {
        public int numberOfSamples;
        public float absorbRate;
        
        [BurstCompile]
        public struct Job : IJob
        {
            public float absorbRate;
            public int maxHits;
            public int2 size;
            public int numberOfSamples;
            public Random random;
            public CameraFrame camera;
            
            // implementing as a straight translation of the C++ gave me a stack overflow
            int recursionCounter;
            
            [ReadOnly] public HitableArray<Sphere> World;
            [WriteOnly] public NativeArray<Color24> Pixels;
            
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
                        float3 col = new float3();
                        for (int s = 0; s < numberOfSamples; s++)
                        {
                            float u = (i + random.NextFloat()) / nx;
                            float v = (j + random.NextFloat()) / ny;
                            Ray r = camera.GetRay(u, v);
                            recursionCounter = 0;
                            col += Color(r, World);
                        }

                        col /= (float)numberOfSamples;
                        Pixels[index] = col.ToRgb24();
                    }
                }
            }

            public float3 Color(Ray r, HitableArray<Sphere> world)
            {
                var rec = new HitRecord();
                if (recursionCounter < maxHits && world.Hit(r, 0.001f, float.MaxValue, ref rec))
                {
                    recursionCounter++;
                    var target = rec.p + rec.normal + Utils.RandomInUnitSphere(random);
                    return absorbRate * Color(new Ray(rec.p, target - rec.p), world);
                }
                
                return Utils.BackgroundColor(ref r);
            }
        }

        // TODO - factor into base class ? ALSO RESET TO ACTUAL BOOK SPHERE
        HitableArray<Sphere> m_Spheres = ExampleSphereSets.FourVaryingSize();

        public override JobHandle Schedule(JobHandle dependency = default)
        {
            var rand = new Random();
            rand.InitState();

            var job = new Job()
            {
                absorbRate = absorbRate,
                maxHits = 32,
                camera = CameraFrame.Default,
                numberOfSamples = numberOfSamples,
                random = rand,
                size = texture.GetSize(),
                World = m_Spheres,
                Pixels = pixelBuffer
            };
            
            jobHandle = job.Schedule(dependency);
            return jobHandle;
        }
        
        public override void Dispose()
        {
            base.Dispose();
            m_Spheres.Dispose();
        }
    }
}