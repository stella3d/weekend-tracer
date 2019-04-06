using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace RayTracingWeekend
{
    public class ChapterTwo
    {
        [BurstCompile]
        public struct Job : IJob, IGetPixelBuffer<Color24>
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

            public NativeArray<Color24> GetPixels()
            {
                return Pixels;
            }
        }

        static readonly int2 imageSize = new int2(200, 100);

        public Texture2D texture = new Texture2D(imageSize.x, imageSize.y, TextureFormat.RGB24, false);

        public void WriteTestImage()
        {
            var buffer = new NativeArray<Color24>(imageSize.x * imageSize.y, Allocator.TempJob);

            var job = new Job()
            {
                size = imageSize,
                Pixels = buffer
            };

            job.RunAndApply<Color24, Job>(texture);
        }
    }
}