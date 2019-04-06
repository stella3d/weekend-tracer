using Unity.Collections;
using UnityEngine;

namespace RayTracingWeekend
{
    public abstract class Chapter<TPixel> where TPixel: struct
    {
        public Texture2D texture = new Texture2D(Constants.ImageSize.x, Constants.ImageSize.y, 
                                                 TextureFormat.RGB24, false);

        protected NativeArray<TPixel> GetBuffer(Allocator allocator = Allocator.TempJob)
        {
            return new NativeArray<TPixel>(Constants.ImageSize.x * Constants.ImageSize.y, allocator);
        }

        public abstract void DrawToTexture();
    }
}