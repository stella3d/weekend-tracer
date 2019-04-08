using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace RayTracingWeekend
{
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