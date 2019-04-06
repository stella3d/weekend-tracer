namespace RayTracingWeekend
{
    public interface IHittable
    {
        bool Hit(Ray r, float tMin, float tMax, ref HitRecord rec);
    }
}