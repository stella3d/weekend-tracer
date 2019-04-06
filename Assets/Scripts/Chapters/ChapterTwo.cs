using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace RayTracingWeekend
{
    public class ChapterTwo : Chapter<Color24>
    {
        [BurstCompile]
        public struct Job : IJob
        {
            public int2 size;

            [WriteOnly] 
            public NativeArray<Color24> Pixels;

            public void Execute()
            {
                var nx = (float) size.x;
                var ny = (float) size.y;
                for (var j = 0; j < size.y; j++)
                {
                    for (var i = 0; i < size.x; i++)
                    {
                        var col = new float3(i / nx, j / ny, 0.2f);
                        
                        var index = (int) (j * nx + i);
                        Pixels[index] = col.ToRgb24();
                    }
                }
            }
        }

        public void WriteTestImage()
        {
            var job = new Job()
            {
                size = Constants.ImageSize,
                Pixels = GetBuffer()
            };

            job.Run();
            texture.LoadAndApply(job.Pixels);
        }
    }
}