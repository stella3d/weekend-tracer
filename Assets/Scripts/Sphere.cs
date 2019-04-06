using Unity.Mathematics;

namespace RayTracingWeekend
{
    public struct Sphere : IHittable
    {
        public float3 center;
        public float radius;

        public Sphere(float3 center, float radius)
        {
            this.center = center;
            this.radius = radius;
        }

        public bool Hit(Ray r, float tMin, float tMax, ref HitRecord rec)
        {
            float3 oc = r.origin - center;
            float a = math.dot(r.direction, r.direction);
            float b = math.dot(oc, r.direction);
            float c = math.dot(oc, oc) - radius * radius;
            float discriminant = b * b - a * c;

            if (discriminant > 0f)
            {
                var sqrtDiscriminant = math.sqrt(discriminant);
                float temp = (-b - sqrtDiscriminant) / a;
                if (temp < tMax && temp > tMin)
                {
                    rec.t = temp;
                    rec.p = r.PointAtParameter(rec.t);
                    rec.normal = (rec.p - center) / radius;
                    return true;
                }
                temp = (-b + sqrtDiscriminant) / a;
                if (temp < tMax && temp > tMin)
                {
                    rec.t = temp;
                    rec.p = r.PointAtParameter(rec.t);
                    rec.normal = (rec.p - center) / radius;
                    return true;
                }
            }

            return false;
        }
    }
}