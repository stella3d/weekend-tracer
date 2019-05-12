using System.Runtime.CompilerServices;
using Unity.Collections;
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

        /// <summary>
        /// Draw the background gradient for any Ray that 
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 BackgroundColor(ref Ray r)
        {
            float3 unitDirection = math.normalize(r.direction);
            var t = 0.5f * (unitDirection.y + 1f);
            return (1f - t) * Constants.one + t * Constants.blueGradient;
        }
        
        // flip unity's Z around the z of where the spheres are centered.  
        // I don't remember quite why this is necessary for the unity -> ray camera translation,
        // but that part is incomplete and experimental anyway
        public static float3 FlipZ(float3 input, float originZ = -1f)
        {
            if (input.z < -1f)
            {
                input.z *= -1f;
                input.z += originZ;
            }
            else
            {
                input.z *= -1f;
                input.z += originZ;
            }

            return input;
        }
        
        public static void ReallocateIfNeeded<T>(ref NativeArray<T> array, int newLength, Allocator allocator = Allocator.Persistent)
            where T: struct
        {
            var oldLength = array.Length;
            if (newLength == oldLength)
                return;
        
            if(array.IsCreated)
                array.Dispose();
        
            array = new NativeArray<T>(newLength, allocator);
        }
    }
}