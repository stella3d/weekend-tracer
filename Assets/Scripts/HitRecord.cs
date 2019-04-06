using Unity.Mathematics;

namespace RayTracingWeekend
{
    public struct HitRecord
    {
        public float t;
        public float3 p;
        public float3 normal;

        public HitRecord(float t, float3 p, float3 normal)
        {
            this.t = t;
            this.p = p;
            this.normal = normal;
        }
    }
}