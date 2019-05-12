using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace RayTracingWeekend
{
    public class ChapterSix : Chapter<Color24>
    {
        public ChapterSix(int width, int height) : base(width, height) { }
        
        public int numberOfSamples;
        
        [BurstCompile]
        public struct Job : IJob
        {
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
                            col += Color(r, World);
                        }

                        col /= (float)numberOfSamples;
                        Pixels[index] = col.ToRgb24();
                    }
                }
            }

            public static float3 Color(Ray r, HitableArray<Sphere> world)
            {
                var rec = new HitRecord();
                if (world.Hit(r, 0f, float.MaxValue, ref rec))
                {
                    var rn = rec.normal;
                    return 0.5f * new float3(rn.x + 1f, rn.y + 1f, rn.z + 1f);
                }
                
                return Utils.BackgroundColor(ref r);
            }
        }

        // TODO - move somewhere common ?
        HitableArray<Sphere> m_Spheres = new HitableArray<Sphere>(2)
        {
            Objects =
            {
                [0] = new Sphere(new float3(0f, 0f, -1f), 0.5f),
                [1] = new Sphere(new float3(0f, -100.5f, -1f), 100f)
            }
        };

        public override JobHandle Schedule(JobHandle dependency = default)
        {
            var rand = new Random();
            rand.InitState();
            
            var job = new Job()
            {
                camera = CameraFrame.Default,
                numberOfSamples = numberOfSamples,
                random = rand,
                size = texture.GetSize(),
                World = m_Spheres,
                Pixels = PixelBuffer
            };
            
            JobHandle = job.Schedule(dependency);
            return JobHandle;
        }
        
        public override void Dispose()
        {
            base.Dispose();
            m_Spheres.Dispose();
        }
    }
}