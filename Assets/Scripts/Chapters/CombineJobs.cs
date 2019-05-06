using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace RayTracingWeekend
{
    public static class Combine
    {
        public static JobHandle ScheduleJob(NativeArray<float3>[] sources, NativeArray<float4> output,
            int completedSampleCount, JobHandle dependency)
        {
            var count = sources.Length;
            switch (count)
            {
                default:
                    return dependency;   
                case 2:
                    var jobTwo = new CombineJobTwo(sources, output, completedSampleCount);
                    return jobTwo.Schedule(sources[0].Length, 1024, dependency);
                case 4:
                    var jobFour = new CombineJobFour(sources, output, completedSampleCount);
                    return jobFour.Schedule(sources[0].Length, 1024, dependency);
                case 6:
                    var jobSix = new CombineJobSix(sources, output, completedSampleCount);
                    return jobSix.Schedule(sources[0].Length, 1024, dependency);
                case 8:
                    var jobEight = new CombineJobEight(sources, output, completedSampleCount);
                    return jobEight.Schedule(sources[0].Length, 1024, dependency);
                case 10:
                    var jobTen = new CombineJobTen(sources, output, completedSampleCount);
                    return jobTen.Schedule(sources[0].Length, 1024, dependency);
            }
        }
    }

    [BurstCompile]
    public struct CombineJobTwo : IJobParallelFor
    {
        public int CompletedSampleCount;
        
        [ReadOnly] public NativeArray<float3> In1;
        [ReadOnly] public NativeArray<float3> In2;
        
        public NativeArray<float4> Accumulated;
        
        public CombineJobTwo(NativeArray<float3>[] buffers, NativeArray<float4> accumulated, int completedSamples)
        {
            if (buffers.Length != 2)
                Debug.LogWarning($"CombineJobTwo needs 2 buffer inputs, but got {buffers.Length}!");

            In1 = buffers[0];
            In2 = buffers[1];
            Accumulated = accumulated;
            CompletedSampleCount = completedSamples;
        }
        
        public void Execute(int i)
        {
            var sum = In1[i] + In2[i];
            var sumPixel = new float4(sum.x, sum.y, sum.z, 1f);
            
            var a = Accumulated[i];
            var aWeighted = a * CompletedSampleCount;
            var acc = (sumPixel + aWeighted) / (2 + CompletedSampleCount);
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
        
        public CombineJobFour(NativeArray<float3>[] buffers, NativeArray<float4> accumulated, int completedSamples)
        {
            if (buffers.Length != 4)
                Debug.LogWarning($"CombineJobFour needs 4 buffer inputs, but got {buffers.Length}!");

            In1 = buffers[0];
            In2 = buffers[1];
            In3 = buffers[2];
            In4 = buffers[3];
            Accumulated = accumulated;
            CompletedSampleCount = completedSamples;
        }
        
        public void Execute(int i)
        {
            var sum = In1[i] + In2[i] + In3[i] + In4[i];
            var sumPixel = new float4(sum.x, sum.y , sum.z, 1f);
            
            var a = Accumulated[i];
            var aWeighted = a * CompletedSampleCount;
            var acc = (sumPixel + aWeighted) / (4 + CompletedSampleCount);
            Accumulated[i] = acc;
        }
    }
    
    [BurstCompile]
    public struct CombineJobSix : IJobParallelFor
    {
        public int CompletedSampleCount;
        
        [ReadOnly] public NativeArray<float3> In1;
        [ReadOnly] public NativeArray<float3> In2;
        [ReadOnly] public NativeArray<float3> In3;
        [ReadOnly] public NativeArray<float3> In4;
        [ReadOnly] public NativeArray<float3> In5;
        [ReadOnly] public NativeArray<float3> In6;
        
        public NativeArray<float4> Accumulated;

        public CombineJobSix(NativeArray<float3>[] buffers, NativeArray<float4> accumulated, int completedSamples)
        {
            if(buffers.Length != 8)
                Debug.LogWarning($"CombineJobEight constructor needs 8 buffer inputs, but got {buffers.Length}!");

            In1 = buffers[0];
            In2 = buffers[1];
            In3 = buffers[2];
            In4 = buffers[3];
            In5 = buffers[4];
            In6 = buffers[5];
            Accumulated = accumulated;
            CompletedSampleCount = completedSamples;
        }

        public void Execute(int i)
        {
            var sum = In1[i] + In2[i] + In3[i] + In4[i] + In5[i] + In6[i];
            var sumPixel = new float4(sum.x, sum.y , sum.z, 1f);
            
            var a = Accumulated[i];
            var aWeighted = a * CompletedSampleCount;
            
            var acc = (sumPixel + aWeighted) / (6 + CompletedSampleCount);
            acc.w = 1f;                    // hard-code full alpha
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

            var a = Accumulated[i];
            var aWeighted = a * CompletedSampleCount;

            var acc = (sumPixel + aWeighted) / (10 + CompletedSampleCount);
            acc.w = 1f; // hard-code full alpha
            Accumulated[i] = acc;
        }
    }
}