using Unity.Mathematics;

namespace RayTracingWeekend
{
    public struct HitRecord
    {
        public float t;
        public float3 p;
        public float3 normal;
        public Material material;

        public HitRecord(float t, float3 p, float3 normal)
        {
            this.t = t;
            this.p = p;
            this.normal = normal;
            material = new Material(MaterialType.Lambertian, new float3(0.5f, 0.5f, 0.5f));
        }
        
        public HitRecord(float t, float3 p, float3 normal, Material material)
        {
            this.t = t;
            this.p = p;
            this.normal = normal;
            this.material = material;
        }
    }
}