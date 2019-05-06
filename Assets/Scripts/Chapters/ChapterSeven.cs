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

            // TODO - only use utils version of this
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

            // implementing as a straight translation of the C++ gave me a stack overflow
            int recursionCounter;

            public float3 Color(Ray r, HitableArray<Sphere> world)
            {
                var rec = new HitRecord();
                if (recursionCounter < maxHits && world.Hit(r, 0.001f, float.MaxValue, ref rec))
                {
                    recursionCounter++;
                    //var target = rec.p + rec.normal + Utils.RandomInUnitSphere(random);
                    var target = rec.p + rec.normal + RandomInUnitSphere();
                    return absorbRate * Color(new Ray(rec.p, target - rec.p), world);
                }
                
                return Utils.BackgroundColor(ref r);
            }
        }

        // TODO - make this respect canvas scaling options
        int m_CanvasScale = 1;

        // TODO - factor into base class ? ALSO RESET TO ACTUAL BOOK SPHERE
        HitableArray<Sphere> m_Spheres = ExampleSphereSets.FourVaryingSize();
        
        public override void Dispose()
        {
            base.Dispose();
            m_Spheres.Dispose();
        }

        public void DrawToTexture()
        {
            ScaleTexture(m_CanvasScale);
            var spheres = ExampleSphereSets.FourVaryingSize();
            
            var rand = new Random();
            rand.InitState();

            var job = new Job()
            {
                absorbRate = absorbRate,
                maxHits = 32,
                camera = CameraFrame.Default,
                numberOfSamples = numberOfSamples,
                random = rand,
                size = Constants.DefaultImageSize * m_CanvasScale,
                World = spheres,
                Pixels = pixelBuffer
            };
            
            job.Run();
            texture.LoadAndApply(job.Pixels);
            spheres.Dispose();
        }

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
                size = Constants.DefaultImageSize * m_CanvasScale,
                World = m_Spheres,
                Pixels = pixelBuffer
            };
            
            jobHandle = job.Schedule(dependency);
            return jobHandle;
        }
    }
}