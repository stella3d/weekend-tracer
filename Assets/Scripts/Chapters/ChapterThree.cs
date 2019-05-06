using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace RayTracingWeekend
{
    public class ChapterThree : Chapter<Color24>
    {
        [BurstCompile]
        public struct Job : IJob
        {
            public int2 size;

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
                        float3 col = Color(r);

                        var index = (int) (j * nx + i);
                        Pixels[index] = col.ToRgb24();
                    }
                }
            }

            public static float3 Color(Ray r)
            {
                float3 unitVector = math.normalize(r.direction);
                float t = 0.5f * (unitVector.y + 1f);
                return (1f - t) * Constants.one + t * Constants.blueGradient;
            }
        }

        public override void DrawToTexture()
        {
            var job = new Job()
            {
                size = Constants.DefaultImageSize,
                Pixels = GetBuffer()
            };
            
            job.Run();
            texture.LoadAndApply(job.Pixels);
        }

        public override JobHandle Schedule()
        {
            throw new System.NotImplementedException();
        }
    }
}