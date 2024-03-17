
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

struct JobReproduce : IJobParallelFor
{
    [ReadOnly] public NativeArray<float2> positions;
    public NativeArray<RefRW<ReproductionComponent>> reproduced;
    public float touchingDistance;

    public void Execute(int index)
    {
        for (int i = 0; i < positions.Length; i++)
        {
            if(i == index)
                continue; // Skip self

            if(Vector2.Distance(positions[i], positions[index]) < touchingDistance)
            {
                reproduced[index].ValueRW.Reproduces = true;
                break;
            }
        }
    }
}