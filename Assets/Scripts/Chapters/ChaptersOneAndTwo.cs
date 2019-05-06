using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace RayTracingWeekend
{
    public class ChaptersOneAndTwo : Chapter<Color24>
    {
        /*
        public override void Setup()
        {
            var length = texture.width * texture.height;
            m_Pixels = new NativeArray<Color24>(length, Allocator.Persistent);
        }

        public override void Dispose()
        {
            m_Pixels.DisposeIfCreated();
        }
        */

        [BurstCompile]
        public struct Job : IJob
        {
            public int2 size;

            [WriteOnly] 
            public NativeArray<Color24> Pixels;

            public void Execute()
            {
                var nx = (float) size.x;
                var ny = (float) size.y;
                for (var j = 0; j < size.y; j++)
                {
                    var rowIndex = j * nx;
                    for (var i = 0; i < size.x; i++)
                    {
                        var col = new float3(i / nx, j / ny, 0.2f);
                        var index = (int) (rowIndex + i);
                        Pixels[index] = col.ToRgb24();
                    }
                }
            }
        }

        // TODO - factor all of these into a common method
        public override void DrawToTexture()
        {
            texture.LoadAndApply(pixelBuffer);
        }

        public override JobHandle Schedule(JobHandle dependency = default)
        {
            if(!jobHandle.IsCompleted)
                jobHandle.Complete();
            
            var job = new Job()
            {
                size = Constants.DefaultImageSize,
                Pixels = pixelBuffer
            };

            jobHandle = job.Schedule(dependency);
            return jobHandle;
        }
    }
}