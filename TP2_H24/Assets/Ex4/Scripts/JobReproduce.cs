
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

struct JobReproduce : IJobParallelFor
{
    public NativeArray<float3> positions;
    public float decreasingFactor;
    public bool reproduced;

    public void Execute(int index)
    {
        for (int i = 0; i < positions.Length; i++)
        {
            if (Vector3.Distance(positions[i], positions[index]) < Ex4Config.TouchingDistance)
            {
                reproduced = true;
                break;
            }
        }
    }
}