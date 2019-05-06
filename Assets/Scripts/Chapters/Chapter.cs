using Unity.Collections;
using UnityEngine;

namespace RayTracingWeekend
{
    public abstract class Chapter<TPixel> where TPixel: struct
    {
        public Texture2D texture = new Texture2D(Constants.DefaultImageSize.x, Constants.DefaultImageSize.y, 
                                                 TextureFormat.RGB24, false);

        protected int canvasScaling = 8;
        
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

        internal virtual void Setup() { }

        public abstract void DrawToTexture();

        internal void ScaleTexture(int multiplier, TextureFormat format = TextureFormat.RGB24)
        {
            texture = new Texture2D(Constants.DefaultImageSize.x * multiplier, Constants.DefaultImageSize.y * multiplier, 
                format, false);
        }
    }
}