using Unity.Mathematics;

namespace RayTracingWeekend
{
    public struct Ray
    {
        public float3 origin;
        public float3 direction;

        public float3 PointAtParameter(float t)
        {
            return origin + t * direction;
        }


    }
}