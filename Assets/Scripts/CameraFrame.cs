using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace RayTracingWeekend
{
    public struct CameraFrame
    {
        public float3 origin;
        public float3 lowerLeftCorner;
        public float3 horizontal;
        public float3 vertical;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Ray GetRay(float s, float t)
        {
            return new Ray(origin, lowerLeftCorner + s * horizontal + t * vertical - origin);
        }

        public CameraFrame(float vfov, float aspect)
        {
            float theta = (float)(vfov * math.PI / 180d);
            float halfHeight = math.tan(theta / 2);
            float halfWidth = aspect * halfHeight;
            lowerLeftCorner = new float3(-halfWidth, -halfHeight, -1f);
            horizontal = new float3(2 * halfWidth, 0, 0);
            vertical = new float3(0, 2 * halfHeight, 0);
            origin = new float3();
        }
        
        public CameraFrame(float3 lookFrom, float3 lookAt, float3 vup, float vfov, float aspect)
        {
            float theta = (float)(vfov * math.PI / 180f);
            float halfHeight = math.tan(theta / 2);
            float halfWidth = aspect * halfHeight;
            origin = lookFrom;
            float3 w = math.normalize(lookFrom - lookAt);
            float3 u = math.normalize(math.cross(vup, w));
            float3 v = math.cross(w, u);
            lowerLeftCorner = new float3(-halfWidth, -halfHeight, -1f);        // ?? idk whats up with this
            lowerLeftCorner = origin - halfWidth * u - halfHeight * v - w;
            horizontal = 2 * halfWidth * u;
            vertical = 2 * halfHeight * v;
        }

        public static CameraFrame Default =>
            new CameraFrame
            {
                lowerLeftCorner = new float3(-2, -1, -1),
                horizontal = new float3(4, 0, 0),
                vertical = new float3(0, 2, 0),
                origin = new float3()
            };
    }
}