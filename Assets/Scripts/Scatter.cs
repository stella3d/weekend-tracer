using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace RayTracingWeekend
{
    public static class Scatter
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Diffuse(Ray r, HitRecord rec, ref float3 attenuation, ref Ray scattered, ref Random rand) 
        {
            var target = rec.p + rec.normal + Utils.RandomInUnitSphere(rand);
            scattered = new Ray(rec.p, target - rec.p);
            attenuation = rec.material.albedo;
            return true;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Metal(Ray r, HitRecord rec, ref float3 attenuation, ref Ray scattered, ref Random random)
        {
            var m = rec.material;
            float3 reflected = RayMath.Reflect(math.normalize(r.direction), rec.normal);
            scattered = new Ray(rec.p, reflected + m.fuzziness * Utils.RandomInUnitSphere(random));
            attenuation = m.albedo;
            return math.dot(scattered.direction, rec.normal) > 0;
        }
        
        public static bool Dielectric(Ray r, HitRecord rec, 
            ref float3 attenuation, ref Ray scattered, ref Random rand)
        {
            var refractionIndex = rec.material.refractionIndex;
            float3 outwardNormal;
            float3 reflected = RayMath.Reflect(r.direction, rec.normal);
            float niOverNt;
            attenuation = new float3(1f, 1f, 1f);
            float3 refracted;
            float reflectProbability;
            float cosine;
            
            if (math.dot(r.direction, rec.normal) > 0f)
            {
                outwardNormal = -rec.normal;
                niOverNt = refractionIndex;
                cosine = refractionIndex * math.dot(r.direction, rec.normal) / math.length(r.direction);
            }
            else
            {
                outwardNormal = rec.normal;
                niOverNt = 1f / refractionIndex;
                cosine = -math.dot(r.direction, rec.normal) / math.length(r.direction);
            }

            reflectProbability = RayMath.Refract(r.direction, outwardNormal, niOverNt, out refracted) 
                ? RayMath.Schlick(cosine, refractionIndex) : 1f;

            scattered = rand.NextFloat() < reflectProbability 
                ? new Ray(rec.p, reflected) : new Ray(rec.p, refracted);

            return true;
        }

        public static bool Generic(Ray r, HitRecord rec,
            ref float3 attenuation, ref Ray scattered, ref Random rng)
        {
            switch (rec.material.type)
            {
                // TODO - put this switch inside a static Material.Scatter() method ?
                // also TODO - make the scatter API the same across types
                case MaterialType.Lambertian:
                    return Diffuse(r, rec, ref attenuation, ref scattered, ref rng);
                case MaterialType.Metal:
                    return Metal(r, rec, ref attenuation, ref scattered, ref rng);
                case MaterialType.Dielectric:
                    // The dark sphere outline bug was fixed by adding this line, which
                    // changes the state of the RNG so reflection probability works somehow.
                    // this doesn't work if you call .NextFloat() inside .Dielectric() ??
                    // DON'T REMOVE 
                    rng.NextFloat();
                    return Dielectric(r, rec, ref attenuation, ref scattered, ref rng);
            }

            return false;
        }
    }
}