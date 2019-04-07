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

        public static void PositionNoise(Random rand, float magnitude, HitableArray<Sphere> list)
        {
            for (var i = 0; i < list.Objects.Length; i++)
            {
                var sphere = list.Objects[i];
                var firstRandom = rand.NextFloat();
                var secondRandom = rand.NextFloat();
                if (firstRandom > 0.5f)
                {
                    if (secondRandom > 0.5f)
                    {
                        sphere.center += new float3(rand.NextFloat(), 0f, rand.NextFloat()) * magnitude;
                    }
                    else
                    {
                        sphere.center += new float3(rand.NextFloat(), 0f, -rand.NextFloat()) * magnitude;
                    }
                }
                else
                {
                    if (secondRandom > 0.5f)
                    {
                        sphere.center += new float3(-rand.NextFloat(), 0f, rand.NextFloat()) * magnitude;
                    }
                    else
                    {
                        sphere.center += new float3(-rand.NextFloat(), 0f, -rand.NextFloat()) * magnitude;
                    }
                }
                
                list.Objects[i] = sphere;
            }
        }
        
        public static void SizeNoise(Random rand, float magnitude, HitableArray<Sphere> list)
        {
            for (var i = 0; i < list.Objects.Length; i++)
            {
                var sphere = list.Objects[i];
                var firstRandom = rand.NextFloat();
                if (firstRandom > 0.5f)
                {
                    sphere.radius += rand.NextFloat() * magnitude;
                }
                else
                {
                    sphere.radius += rand.NextFloat() * -magnitude;
                }
                
                list.Objects[i] = sphere;
            }
        }
    }
}