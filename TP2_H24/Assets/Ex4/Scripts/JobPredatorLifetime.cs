
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Entities;

[BurstCompile]
public struct JobPredatorLifeTime : IJobParallelFor
{
    [ReadOnly] public NativeArray<float3> predatorPositions;
    [ReadOnly] public NativeArray<float3> preyPositions;
    public NativeArray<RefRW<LifetimeComponent>> decreasingFactors;
    public float touchingDistance;

    public void Execute(int index)
    {
        decreasingFactors[index].ValueRW.DecreasingFactor = 1.0f;
        float3 predatorPosition = predatorPositions[index];

        for (int i = 0; i < preyPositions.Length; i++)
        {
            if (math.distance(preyPositions[i], predatorPosition) < touchingDistance)
            {
                decreasingFactors[index].ValueRW.DecreasingFactor /= 2;
            }
        }
    }
}