
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
struct JobReproduce : IJobParallelFor
{
    [ReadOnly] public NativeArray<float3> positions;
    public NativeArray<RefRW<ReproductionComponent>> reproduced;
    public float touchingDistance;

    public void Execute(int index)
    {
        if(reproduced[index].ValueRO.Reproduces)
            return;

        float touchingDistanceSq = touchingDistance * touchingDistance;
        for (int i = 0; i < positions.Length; i++)
        {
            if(i == index)
                continue; // Skip self

            if(math.distancesq(positions[i], positions[index]) < touchingDistanceSq)
            {
                reproduced[index].ValueRW.Reproduces = true;
                break;
            }
        }
    }
}