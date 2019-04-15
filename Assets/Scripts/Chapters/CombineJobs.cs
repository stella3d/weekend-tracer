using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace RayTracingWeekend
{
    public static class Combine
    {
        public static JobHandle GetCombineJob(this NativeArray<float3>[] sources, int completedSampleCount, 
            NativeArray<float4> output, JobHandle dependency)
        {
            var count = sources.Length;
            switch (count)
            {
                case 2:
                    var jobTwo = new CombineJobTwo()
                    {
                        CompletedSampleCount = completedSampleCount,
                        In1 = sources[0],
                        In2 = sources[1],
                        Accumulated = output
                    };
                    return jobTwo.Schedule(sources[0].Length, 1024, dependency);
                case 4:
                    var jobFour = new CombineJobFour()
                    {
                        CompletedSampleCount = completedSampleCount,
                        In1 = sources[0],
                        In2 = sources[1],
                        In3 = sources[2],
                        In4 = sources[3],
                        Accumulated = output
                    };
                    return jobFour.Schedule(sources[0].Length, 1024, dependency);
                case 8:
                    var jobEight = new CombineJobEight()
                    {
                        CompletedSampleCount = completedSampleCount,
                        In1 = sources[0],
                        In2 = sources[1],
                        In3 = sources[2],
                        In4 = sources[3],
                        In5 = sources[4],
                        In6 = sources[5],
                        In7 = sources[6],
                        In8 = sources[7],
                        Accumulated = output
                    };
                    return jobEight.Schedule(sources[0].Length, 1024, dependency);
            }

            return dependency;
        }
    }


    [BurstCompile]
    public struct CombineJobTwo : IJobParallelFor
    {
        public int CompletedSampleCount;
        
        [ReadOnly] public NativeArray<float3> In1;
        [ReadOnly] public NativeArray<float3> In2;
        
        public NativeArray<float4> Accumulated;
        
        public void Execute(int i)
        {
            var sum = In1[i] + In2[i];
            var sumPixel = new float4(sum.x, sum.y , sum.z, 1f);
            
            var a = Accumulated[i];
            var aWeighted = a * CompletedSampleCount;
            var acc = (sumPixel + aWeighted) / (8 + CompletedSampleCount);
            Accumulated[i] = acc;
        }
    }
    
    [BurstCompile]
    public struct CombineJobFour : IJobParallelFor
    {
        public int CompletedSampleCount;
        
        [ReadOnly] public NativeArray<float3> In1;
        [ReadOnly] public NativeArray<float3> In2;
        [ReadOnly] public NativeArray<float3> In3;
        [ReadOnly] public NativeArray<float3> In4;
        
        public NativeArray<float4> Accumulated;
        
        public void Execute(int i)
        {
            var sum = In1[i] + In2[i] + In3[i] + In4[i];
            var sumPixel = new float4(sum.x, sum.y , sum.z, 1f);
            
            var a = Accumulated[i];
            var aWeighted = a * CompletedSampleCount;
            var acc = (sumPixel + aWeighted) / (8 + CompletedSampleCount);
            Accumulated[i] = acc;
        }
    }
    
    [BurstCompile]
    public struct CombineJobEight : IJobParallelFor
    {
        public int CompletedSampleCount;
        
        [ReadOnly] public NativeArray<float3> In1;
        [ReadOnly] public NativeArray<float3> In2;
        [ReadOnly] public NativeArray<float3> In3;
        [ReadOnly] public NativeArray<float3> In4;
        [ReadOnly] public NativeArray<float3> In5;
        [ReadOnly] public NativeArray<float3> In6;
        [ReadOnly] public NativeArray<float3> In7;
        [ReadOnly] public NativeArray<float3> In8;
        
        public NativeArray<float4> Accumulated;
        
        public void Execute(int i)
        {
            var sum = In1[i] + In2[i] + In3[i] + In4[i] + In5[i] + In6[i] + In7[i] + In8[i];
            var sumPixel = new float4(sum.x, sum.y , sum.z, 1f);
            
            var a = Accumulated[i];
            var aWeighted = a * CompletedSampleCount;
            var acc = (sumPixel + aWeighted) / (8 + CompletedSampleCount);
            Accumulated[i] = acc;
        }
    }
}