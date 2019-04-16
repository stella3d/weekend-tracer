using System;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace RayTracingWeekend
{
    public struct HitableArray<T> : IHittable, IDisposable
        where T: struct, IHittable
    {
        public NativeArray<T> Objects;

        public HitableArray(int count, Allocator allocator = Allocator.Persistent)
        {
            Objects = new NativeArray<T>(count, allocator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Hit(Ray r, float tMin, float tMax, ref HitRecord rec)
        {
            HitRecord tempRecord = new HitRecord();
            bool hitAnything = false;
            float closestSoFar = tMax;
            for (var i = 0; i < Objects.Length; i++)
            {
                var obj = Objects[i];
                if (obj.Hit(r, tMin, closestSoFar, ref tempRecord))
                {
                    hitAnything = true;
                    closestSoFar = tempRecord.t;
                    rec = tempRecord;
                }
            }

            return hitAnything;
        }

        public void Dispose()
        {
            if(Objects.IsCreated)
                Objects.Dispose();
        }
    }
}