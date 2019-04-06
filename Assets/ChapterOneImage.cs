using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


public class ChapterOneImage
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Color24
    {
        public byte r;
        public byte g;
        public byte b;

        public Color24(byte r, byte g, byte b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }
    }

    public struct Job : IJob
    {
        public int2 size;

        [WriteOnly]
        public NativeArray<Color24> Pixels;

        public void Execute()
        {
            var nx = (float) size.x;
            var ny = (float) size.y;
            for (float j = 0; j < size.y; j++)
            {
                for (float i = 0; i < size.x; i++)
                {
                    const float rgbMultiplier = 255.999f;
                    byte r = (byte)(i / nx * rgbMultiplier);
                    byte g = (byte)(j / ny * rgbMultiplier);
                    byte b = (byte)((0.2f) * rgbMultiplier);

                    var index = (int) (j * nx + i);
                    
                    Pixels[index] = new Color24(r, g, b);
                }
            }
        }
    }

    static readonly int2 imageSize = new int2(200, 100);

    public JobHandle Handle;

    public Texture2D texture = new Texture2D(imageSize.x, imageSize.y, TextureFormat.RGB24, false);
    
    public void WriteTestImage()
    {
        var buffer = new NativeArray<Color24>(imageSize.x * imageSize.y, Allocator.Persistent);

        var job = new Job()
        {
            size = imageSize,
            Pixels = buffer
        };

        Handle = job.Schedule();
        Handle.Complete();
        
        texture.LoadRawTextureData(job.Pixels);
        texture.Apply();
        
        buffer.Dispose();
    }
}
