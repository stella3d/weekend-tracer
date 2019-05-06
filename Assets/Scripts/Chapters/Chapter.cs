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
        public TextureFormat textureFormat = TextureFormat.RGB24;
        
        public Texture2D texture = new Texture2D(Constants.DefaultImageSize.x, Constants.DefaultImageSize.y, 
                                                 TextureFormat.RGB24, false);

        public JobHandle jobHandle { get; protected set; }
        
        public NativeArray<TPixel> pixelBuffer { get; private set; }

        protected NativeArray<TPixel> GetBuffer(Allocator allocator = Allocator.TempJob, int multiplier = 1)
        {
            var x = multiplier * Constants.DefaultImageSize.x;
            var y = multiplier * Constants.DefaultImageSize.y;
            return new NativeArray<TPixel>(x * y, allocator);
        }

        public Chapter()
        {
            Setup();
        }

        public void Resize(int2 size)
        {
            texture = new Texture2D(size.x, size.y, textureFormat, false);
            pixelBuffer = texture.GetRawTextureData<TPixel>();
        }
        
        public virtual void Setup()
        {
            pixelBuffer = texture.GetRawTextureData<TPixel>();
        }

        public virtual void Dispose() {}

        public abstract JobHandle Schedule(JobHandle dependency = default);

        protected void UploadTextureBuffer()
        {
            texture.LoadAndApply(pixelBuffer);
        }

        public void Complete(bool updateTexture = true)
        {
            jobHandle.Complete();
            if (updateTexture)
                UploadTextureBuffer();
        }

        internal void ScaleTexture(int multiplier, TextureFormat format = TextureFormat.RGB24)
        {
            texture = new Texture2D(Constants.DefaultImageSize.x * multiplier, Constants.DefaultImageSize.y * multiplier, 
                format, false);
        }
    }
}