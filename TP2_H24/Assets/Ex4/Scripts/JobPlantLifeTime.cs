
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Entities;

[BurstCompile]
public struct JobPlantLifeTime : IJobParallelFor
{
    [ReadOnly]public NativeArray<float3> plantPositions;
    [ReadOnly] public NativeArray<float3> preyPositions;
    public NativeArray<RefRW<LifetimeComponent>> decreasingFactors;
    public float touchingDistance;

    public void Execute(int index)
    {
        decreasingFactors[index].ValueRW.DecreasingFactor = 1.0f;

        float3 plantPosition = plantPositions[index];
        float touchingDistanceSq = touchingDistance * touchingDistance;

        for(int i = 0; i < preyPositions.Length; i++)
        {
            if(math.distancesq(preyPositions[i], plantPosition) < touchingDistanceSq)
            {
                decreasingFactors[index].ValueRW.DecreasingFactor *= 2;
                break;
            }
        }
    }
}