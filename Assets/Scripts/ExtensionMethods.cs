using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace RayTracingWeekend
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Convert a 96-bit floating point RGB to 24-bit byte RGB
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color24 ToRgb24(this float3 color)
        {
            const float rgbMultiplier = 255.999f;
            var r = (byte) (color.x * rgbMultiplier);
            var g = (byte) (color.y * rgbMultiplier);
            var b = (byte) (color.z * rgbMultiplier);
            return new Color24(r, g, b);
        }
        
        public static void LoadAndApply<T>(this Texture2D texture, NativeArray<T> buffer, bool dispose = true)
            where T : struct
        {
            texture.LoadRawTextureData(buffer);
            texture.Apply();
            if(dispose)
                buffer.Dispose();
        }

        public static float3 GetAlbedo(this UnityEngine.Material material)
        {
            // it's important to get the linear representation of the color
            var c = material.color.linear;
            return new float3(c.r, c.g, c.b);
        }
    }
}