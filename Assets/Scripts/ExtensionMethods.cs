using Unity.Mathematics;

namespace DefaultNamespace
{
    public static class ExtensionMethods
    {
        public static Color24 ToRgb24(this float3 color)
        {
            const float rgbMultiplier = 255.999f;
            var r = (byte)(color.x * rgbMultiplier);
            var g = (byte)(color.y * rgbMultiplier);
            var b = (byte)(color.z * rgbMultiplier);
            return new Color24(r, g, b);
        }
    }
}