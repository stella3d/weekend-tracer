using Unity.Mathematics;

namespace RayTracingWeekend
{
    public static class Constants
    {
        /// <summary>
        /// The image size used in the original book
        /// </summary>
        public static int2 DefaultImageSize = new int2(200, 100);
        
        public const float rgbMultiplier = 255.999f;
        public static readonly float3 one = new float3(1f, 1f, 1f);
        public static readonly float3 blueGradient = new float3(0.5f, 0.7f, 1f);
    }
}