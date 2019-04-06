using Unity.Collections;
using UnityEngine;

namespace RayTracingWeekend
{
    public abstract class Chapter<TPixel> where TPixel: struct
    {
        public Texture2D texture = new Texture2D(Constants.ImageSize.x, Constants.ImageSize.y, 
                                                 TextureFormat.RGB24, false);

        protected NativeArray<TPixel> GetBuffer(Allocator allocator = Allocator.TempJob, int multiplier = 1)
        {
            var x = multiplier * Constants.ImageSize.x;
            var y = multiplier * Constants.ImageSize.y;
            return new NativeArray<TPixel>(x * y, allocator);
        }

        public abstract void DrawToTexture();

        protected void ScaleTexture(int multiplier)
        {
            texture = new Texture2D(Constants.ImageSize.x * multiplier, Constants.ImageSize.y * multiplier, 
                TextureFormat.RGB24, false);
        }
    }
}