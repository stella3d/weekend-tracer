using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEditor.MemoryProfiler;

namespace RayTracingWeekend
{
    public struct CameraFrame
    {
        public float3 origin;
        public float3 lowerLeftCorner;
        public float3 horizontal;
        public float3 vertical;
        public float3 u, v, w;
        public float lensRadius;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Ray GetRay(float s, float t)
        {
            return new Ray(origin, lowerLeftCorner + s * horizontal + t * vertical - origin);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Ray GetRay(float s, float t, Random rand)
        {
            float3 rd = lensRadius * rand.RandomInUnitDisk();
            float3 offset = u * rd.x + v * rd.y;
            return new Ray(origin + offset, lowerLeftCorner + s * horizontal + t * vertical - origin - offset);
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
            
            u = v = w = new float3();
            lensRadius = 1f;
        }
        
        /// <summary>
        /// The camera constructor used in Chapter 10
        /// </summary>
        public CameraFrame(float3 lookFrom, float3 lookAt, float3 vup, float vfov, float aspect)
        {
            float theta = (float)(vfov * math.PI / 180f);
            float halfHeight = math.tan(theta / 2);
            float halfWidth = aspect * halfHeight;
            origin = lookFrom;
            w = math.normalize(lookFrom - lookAt);
            u = math.normalize(math.cross(vup, w));
            v = math.cross(w, u);
            lowerLeftCorner = new float3(-halfWidth, -halfHeight, -1f);        
            lowerLeftCorner = origin - halfWidth * u - halfHeight * v - w;
            horizontal = 2 * halfWidth * u;
            vertical = 2 * halfHeight * v;
            lensRadius = 1f;
        }
        
        /// <summary>
        /// The camera constructor used in Chapter 11
        /// </summary>
        public CameraFrame(float3 lookFrom, float3 lookAt, float3 vup, 
            float vfov, float aspect, float aperture = 2f, float focusDistance = 1f)
        {
            lensRadius = aperture / 2;
            float theta = (float)(vfov * math.PI / 180f);
            float halfHeight = math.tan(theta / 2);
            float halfWidth = aspect * halfHeight;
            origin = lookFrom;
            w = math.normalize(lookFrom - lookAt);
            u = math.normalize(math.cross(vup, w));
            v = math.cross(w, u);
            var focusedHalfWidth = halfWidth * focusDistance * u;
            var focusedHalfHeight = halfHeight * focusDistance * v;
            lowerLeftCorner = origin - focusedHalfWidth - focusedHalfHeight - focusDistance * w;
            horizontal = 2 * halfWidth * focusDistance * u;
            vertical = 2 * halfHeight * focusDistance * v;
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