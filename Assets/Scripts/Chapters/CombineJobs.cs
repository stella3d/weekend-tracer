using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
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

        public CombineJobEight(NativeArray<float3>[] buffers, NativeArray<float4> accumulated, int completedSamples)
        {
            if(buffers.Length != 8)
                Debug.LogWarning($"CombineJobEight constructor needs 8 buffer inputs, but got {buffers.Length}!");

            In1 = buffers[0];
            In2 = buffers[1];
            In3 = buffers[2];
            In4 = buffers[3];
            In5 = buffers[4];
            In6 = buffers[5];
            In7 = buffers[6];
            In8 = buffers[7];
            Accumulated = accumulated;
            CompletedSampleCount = completedSamples;
        }

        public void Execute(int i)
        {
            var sum = In1[i] + In2[i] + In3[i] + In4[i] + In5[i] + In6[i] + In7[i] + In8[i];
            var sumPixel = new float4(sum.x, sum.y , sum.z, 1f);
            var averagePixel = sumPixel / 8;
            
            averagePixel.w = 1f;
            
            var a = Accumulated[i];
            var aWeighted = a * CompletedSampleCount;
            
            var acc = (sumPixel + aWeighted) / (8 + CompletedSampleCount);
            acc.w = 1f;                    // hard-code full alpha
            Accumulated[i] = acc;
        }
    }

    [BurstCompile]
    public struct CombineJobTen : IJobParallelFor
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
        [ReadOnly] public NativeArray<float3> In9;
        [ReadOnly] public NativeArray<float3> In10;

        public NativeArray<float4> Accumulated;

        public CombineJobTen(NativeArray<float3>[] buffers, NativeArray<float4> accumulated, int completedSamples)
        {
            if (buffers.Length != 10)
                Debug.LogWarning($"CombineJobTen constructor needs 10 buffer inputs, but got {buffers.Length}!");

            In1 = buffers[0];
            In2 = buffers[1];
            In3 = buffers[2];
            In4 = buffers[3];
            In5 = buffers[4];
            In6 = buffers[5];
            In7 = buffers[6];
            In8 = buffers[7];
            In9 = buffers[8];
            In10 = buffers[9];
            Accumulated = accumulated;
            CompletedSampleCount = completedSamples;
        }

        public void Execute(int i)
        {
            var sum = In1[i] + In2[i] + In3[i] + In4[i] + In5[i] + In6[i] + In7[i] + In8[i] + In9[i] + In10[i];
            var sumPixel = new float4(sum.x, sum.y, sum.z, 1f);
            var averagePixel = sumPixel / 10;

            averagePixel.w = 1f;

            var a = Accumulated[i];
            var aWeighted = a * CompletedSampleCount;

            var acc = (sumPixel + aWeighted) / (10 + CompletedSampleCount);
            acc.w = 1f; // hard-code full alpha
            Accumulated[i] = acc;
        }
    }
}