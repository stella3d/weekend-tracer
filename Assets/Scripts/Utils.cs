using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEditor;

namespace RayTracingWeekend
{
    public static class Utils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 RandomInUnitSphere(Random rand)
        {
            var r = rand;
            float3 p;
            float3 one = new float3(1f, 1f, 1f);
            do
            {
                p = 2f * new float3(r.NextFloat(), r.NextFloat(), r.NextFloat()) - one;
            } 
            while (p.x * p.x + p.y * p.y + p.z * p.z >= 1.0f);
            return p;
        }
    }
}