
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Entities;

[BurstCompile]
public struct JobPlantLifeTime : IJobParallelFor
{
    [ReadOnly]public NativeArray<float2> plantPositions;
    [ReadOnly] public NativeArray<float2> preyPositions;
    public NativeArray<RefRW<LifetimeComponent>> decreasingFactors;
    public float touchingDistance;

    public void Execute(int index)
    {
        decreasingFactors[index].ValueRW.DecreasingFactor = 1.0f;

        float2 plantPosition = plantPositions[index];

        for(int i = 0; i < preyPositions.Length; i++)
        {
            if(Vector2.Distance(preyPositions[i], plantPosition) < touchingDistance)
            {
                decreasingFactors[index].ValueRW.DecreasingFactor *= 2;
                break;
            }
        }
    }
}