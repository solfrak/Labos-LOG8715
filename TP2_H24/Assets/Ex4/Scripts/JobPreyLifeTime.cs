
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Entities;

[BurstCompile]
public struct JobPreyLifeTime : IJobParallelFor
{
    [ReadOnly] public NativeArray<float3> predatorPositions;
    [ReadOnly] public NativeArray<float3> plantPositions;
    [ReadOnly] public NativeArray<float3> preyPositions;
    public NativeArray<RefRW<LifetimeComponent>> decreasingFactorsPreys;
    public float touchingDistance;

    public void Execute(int index)
    {
        decreasingFactorsPreys[index].ValueRW.DecreasingFactor = 1.0f;
        float3 preyPosition = preyPositions[index];
        float touchingDistanceSq = touchingDistance * touchingDistance;

        for (int i = 0; i < plantPositions.Length; i++)
        {
            if (math.distancesq(plantPositions[i], preyPosition) < touchingDistanceSq)
            {
                decreasingFactorsPreys[index].ValueRW.DecreasingFactor /= 2;
                break;
            }
        }

        for (int i = 0; i < predatorPositions.Length; i++)
        {
            if (math.distancesq(predatorPositions[i], preyPosition) < touchingDistanceSq)
            {
                decreasingFactorsPreys[index].ValueRW.DecreasingFactor *= 2f;
                break;
            }
        }
    }
}