using Unity.Mathematics;
using UnityEditor;

namespace RayTracingWeekend
{
    public interface IMaterial
    {
        bool Scatter(Ray r, HitRecord rec, ref float3 attenuation, ref Ray scattered);
    }

    // TODO - find a way to make these not monolithic
    public struct Material
    {
        public float fuzziness;
        public float refractionIndex;
        public MaterialType type;
        public float3 albedo;

        public Material(MaterialType type, float3 albedo, float fuzziness = 0f, float refractionIndex = 0f)
        {
            this.type = type;
            this.albedo = albedo;
            this.fuzziness = fuzziness;
            this.refractionIndex = refractionIndex;
        }
    }
}