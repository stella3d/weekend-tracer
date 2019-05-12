using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace RayTracingWeekend
{
    public abstract class Chapter<TPixel> : IDisposable
        where TPixel: struct
    {
        public TextureFormat TextureFormat = TextureFormat.RGB24;

        public Texture2D texture;
        public JobHandle JobHandle { get; protected set; }
        
        public NativeArray<TPixel> PixelBuffer { get; protected set; }

        public Chapter(int width, int height, TextureFormat format = TextureFormat.RGB24)
        {
            texture = new Texture2D(width, height, format, false);
            Setup();
        }

        public void Resize(int2 size)
        {
            texture = new Texture2D(size.x, size.y, TextureFormat, false);
            PixelBuffer = texture.GetRawTextureData<TPixel>();
        }
        
        public virtual void Setup()
        {
            PixelBuffer = texture.GetRawTextureData<TPixel>();
        }

        public virtual void Dispose() {}

        public abstract JobHandle Schedule(JobHandle dependency = default);

        protected void UploadTextureBuffer()
        {
            texture.LoadAndApply(PixelBuffer);
        }

        public void Complete(bool updateTexture = true)
        {
            JobHandle.Complete();
            if (updateTexture)
                UploadTextureBuffer();
        }
    }
}