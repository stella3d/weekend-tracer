﻿using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace RayTracingWeekend
{
    public class ChapterOne
    {
        [BurstCompile]
        public struct Job : IJob, IGetPixelBuffer<Color24>
        {
            public int2 size;

            [WriteOnly] 
            public NativeArray<Color24> Pixels;

            // the Execute() method is where we will put the adapted code from the book for the most part.
            public void Execute()
            {
                var nx = (float) size.x;
                var ny = (float) size.y;
                for (float j = 0; j < size.y; j++)
                {
                    for (float i = 0; i < size.x; i++)
                    {
                        const float rgbMultiplier = 255.999f;
                        var r = (byte) (i / nx * rgbMultiplier);
                        var g = (byte) (j / ny * rgbMultiplier);
                        var b = (byte) (0.2f * rgbMultiplier);

                        var index = (int) (j * nx + i);

                        Pixels[index] = new Color24(r, g, b);
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