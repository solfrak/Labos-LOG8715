
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Entities;

[BurstCompile]
public struct JobPreyLifeTime : IJobParallelFor
{
    [ReadOnly] public NativeArray<float2> predatorPositions;
    [ReadOnly] public NativeArray<float2> plantPositions;
    [ReadOnly] public NativeArray<float2> preyPositions;
    public NativeArray<RefRW<LifetimeComponent>> decreasingFactorsPreys;
    public float touchingDistance;

    public void Execute(int index)
    {
        decreasingFactorsPreys[index].ValueRW.DecreasingFactor = 1.0f;
        float2 preyPosition = preyPositions[index];


        for (int i = 0; i < plantPositions.Length; i++)
        {
            if (Vector2.Distance(plantPositions[i], preyPosition) < touchingDistance)
            {
                decreasingFactorsPreys[index].ValueRW.DecreasingFactor /= 2;
                break;
            }
        }

        for (int i = 0; i < predatorPositions.Length; i++)
        {
            if (Vector2.Distance(predatorPositions[i], preyPosition) < touchingDistance)
            {
                decreasingFactorsPreys[index].ValueRW.DecreasingFactor *= 2f;
                break;
            }
        }
    }
}