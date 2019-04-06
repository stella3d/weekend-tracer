using Unity.Mathematics;
using UnityEditor;

namespace RayTracingWeekend
{
    public interface IMaterial
    {
        bool Scatter(Ray r, HitRecord rec, ref float3 attenuation, ref Ray scattered);
    }

    public struct Material
    {
        public float fuzziness;
        public MaterialType type;
        public float3 albedo;

        public Material(MaterialType type, float3 albedo, float fuzziness = 0f)
        {
            this.type = type;
            this.albedo = albedo;
            this.fuzziness = fuzziness;
        }
    }
}