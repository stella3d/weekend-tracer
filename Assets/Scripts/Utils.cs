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
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 RandomInUnitDisk(this Random rand)
        {
            float3 p;
            float3 one = new float3(1f, 1f, 0f);
            do
            {
                p = 2f * new float3(rand.NextFloat(), rand.NextFloat(), 0f) - one;
            } 
            while (math.dot(p, p) >= 1f);
            return p;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 BackgroundColor(ref Ray r)
        {
            float3 unitDirection = math.normalize(r.direction);
            var t = 0.5f * (unitDirection.y + 1f);
            return (1f - t) * Constants.one + t * Constants.blueGradient;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Schlick(float cosine, float refractionIndex)
        {
            float r0 = (1f - refractionIndex) / (1f + refractionIndex);
            r0 = r0 * r0;
            return r0 + (1 - r0) * math.pow(1 - cosine, 5);
        }
        
        public static bool DielectricScatter(Random rand, Ray rIn, HitRecord rec, 
            ref float3 attenuation, ref Ray scattered)
        {
            var refractionIndex = rec.material.refractionIndex;
            float3 outwardNormal;
            float3 reflected = MetalMaterial.Reflect(rIn.direction, rec.normal);
            float niOverNt;
            attenuation = new float3(1f, 1f, 1f);
            float3 refracted;
            float reflectProbability;
            float cosine;
            
            if (math.dot(rIn.direction, rec.normal) > 0f)
            {
                outwardNormal = -rec.normal;
                niOverNt = refractionIndex;
                cosine = refractionIndex * math.dot(rIn.direction, rec.normal) / math.length(rIn.direction);
            }
            else
            {
                outwardNormal = rec.normal;
                niOverNt = 1f / refractionIndex;
                cosine = -math.dot(rIn.direction, rec.normal) / math.length(rIn.direction);
            }

            if (MetalMaterial.Refract(rIn.direction, outwardNormal, niOverNt, out refracted))
            {
                reflectProbability = Schlick(cosine, refractionIndex);
            }
            else
            {
                reflectProbability = 1f;
            }

            if (rand.NextFloat() < reflectProbability)
            {
                scattered = new Ray(rec.p, reflected);
            }
            else
            {
                scattered = new Ray(rec.p, refracted);
            }

            return true;
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