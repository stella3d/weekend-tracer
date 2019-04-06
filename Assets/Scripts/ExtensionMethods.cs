using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace RayTracingWeekend
{
    public static class ExtensionMethods
    {
        public static Color24 ToRgb24(this float3 color)
        {
            const float rgbMultiplier = 255.999f;
            var r = (byte) (color.x * rgbMultiplier);
            var g = (byte) (color.y * rgbMultiplier);
            var b = (byte) (color.z * rgbMultiplier);
            return new Color24(r, g, b);
        }

        public static void RunAndApply<TPixel, TJob>(this TJob job, Texture2D texture, bool dispose = true)
            where TJob : struct, IJob, IGetPixelBuffer<TPixel>
            where TPixel : struct
        {
            var handle = job.Schedule();
            handle.Complete();
            var pixels = job.GetPixels();
            texture.LoadRawTextureData(pixels);
            texture.Apply();
            if (dispose)
                pixels.Dispose();
        }
    }
}