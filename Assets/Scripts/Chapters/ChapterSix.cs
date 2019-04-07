using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace RayTracingWeekend
{
    public class ChapterSix : Chapter<Color24>
    {
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
                    for (float i = 0; i < size.x; i++)
                    {
                        var index = (int) (j * nx + i);
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

        public override void DrawToTexture()
        {
            var spheres = new HitableArray<Sphere>(2, Allocator.TempJob)
            {
                Objects =
                {
                    [0] = new Sphere(new float3(0f, 0f, -1f), 0.5f),
                    [1] = new Sphere(new float3(0f, -100.5f, -1f), 100f)
                }
            };
            
            var rand = new Random();
            rand.InitState();

            var job = new Job()
            {
                camera = CameraFrame.Default,
                numberOfSamples = numberOfSamples,
                random = rand,
                size = Constants.ImageSize,
                World = spheres,
                Pixels = GetBuffer()
            };
            
            job.Run();
            texture.LoadAndApply(job.Pixels);
            spheres.Dispose();
        }
    }
}