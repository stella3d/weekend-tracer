using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace RayTracingWeekend
{
    public class ChapterFiveTwo : Chapter<Color24>
    {
        [BurstCompile]
        public struct SecondImageJob : IJob
        {
            public int2 size;
            
            [ReadOnly] public HitableArray<Sphere> World;
            
            [WriteOnly] public NativeArray<Color24> Pixels;

            public void Execute()
            {
                var nx = (float) size.x;
                var ny = (float) size.y;
                var lowerLeftCorner = new float3(-2, -1, -1);
                var horizontal = new float3(4, 0, 0);
                var vertical = new float3(0, 2, 0);
                var origin = new float3();

                for (float j = 0; j < size.y; j++)
                {
                    for (float i = 0; i < size.x; i++)
                    {
                        float u = i / nx;
                        float v = j / ny;
                        Ray r = new Ray(origin, lowerLeftCorner + u * horizontal + v * vertical);
                        float3 col = Color(r, World);

                        var index = (int) (j * nx + i);
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
            // TODO - make these permanently allocated ?
            var spheres = new HitableArray<Sphere>(2, Allocator.TempJob)
            {
                Objects =
                {
                    [0] = new Sphere(new float3(0f, 0f, -1f), 0.5f),
                    [1] = new Sphere(new float3(0f, -100.5f, -1f), 100f)
                }
            };

            var job = new SecondImageJob()
            {
                size = Constants.DefaultImageSize,
                World = spheres,
                Pixels = GetBuffer()
            };
            
            job.Run();
            texture.LoadAndApply(job.Pixels);
            spheres.Dispose();
        }
    }
}