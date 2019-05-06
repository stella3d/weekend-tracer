using Unity.Mathematics;

namespace RayTracingWeekend
{
    public struct Material
    {
        public float fuzziness;
        public float refractionIndex;
        public MaterialType type;
        public float3 albedo;

        public Material(MaterialType type, float3 albedo, float fuzziness = 0f, float refractionIndex = 1f)
        {
            this.type = type;
            this.albedo = albedo;
            this.fuzziness = fuzziness;
            this.refractionIndex = refractionIndex;
        }
    }
}