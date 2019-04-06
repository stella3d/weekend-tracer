using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace RayTracingWeekend
{
    public class ChapterFive : Chapter<Color24>
    {
        public float spherePositionZ = -1f;
        public float3 sphereColor = new float3(1f, 0f, 0f);
        
        [BurstCompile]
        public struct Job : IJob
        {
            public int2 size;
            
            public float3 spherePosition;
            public float3 sphereColor;

            [WriteOnly] public NativeArray<Color24> Pixels;

            public void Execute()
            {
                var nx = (float) size.x;
                var ny = (float) size.y;
                var lowerLeftCorner = new float3(-2, -1, -1);
                var horizontal = new float3(4, 0, 0);
                var vertical = new float3(0, 2, 0);
                var origin = new float3();
                for (float j = 0; j < size.y; j++)
                {
                    for (float i = 0; i < size.x; i++)
                    {
                        float u = i / nx;
                        float v = j / ny;
                        Ray r = new Ray(origin, lowerLeftCorner + u * horizontal + v * vertical);
                        float3 col = Color(r);

                        var index = (int) (j * nx + i);
                        Pixels[index] = col.ToRgb24();
                    }
                }
            }

            static float HitSphere(float3 center, float radius, Ray r)
            {
                float3 oc = r.origin - center;
                float a = math.dot(r.direction, r.direction);
                float b = 2f * math.dot(oc, r.direction);
                float c = math.dot(oc, oc) - radius * radius;
                float discriminant = b * b - 4 * a * c;
                if (discriminant < 0f)
                    return -1f;
                
                return (-b - math.sqrt(discriminant)) / (2f * a);
            }

            public float3 Color(Ray r)
            {
                float t = HitSphere(spherePosition, 0.5f, r);
                if (t > 0f)
                {
                    float3 n = math.normalize(r.PointAtParameter(t) - new float3(0f, 0f, -1f));
                    return 0.5f * new float3(n.x + 1f, n.y + 1f, n.z + 1f);
                }
                
                float3 unitDirection = math.normalize(r.direction);
                t = 0.5f * (unitDirection.y + 1f);
                return (1f - t) * Constants.one + t * Constants.blueGradient;
            }
        }

        public override void DrawToTexture()
        {
            var job = new Job()
            {
                sphereColor = sphereColor,
                spherePosition = new float3(0f, 0f, spherePositionZ),
                size = Constants.ImageSize,
                Pixels = GetBuffer()
            };
            
            job.Run();
            texture.LoadAndApply(job.Pixels);
        }
    }
}