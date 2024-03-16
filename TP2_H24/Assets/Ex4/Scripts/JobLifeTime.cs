
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditorInternal.VR;
using UnityEngine;
using Unity.Burst;

[BurstCompile]
public struct JobLifeTime : IJobParallelFor
{
    [ReadOnly]public NativeArray<Vector3> positions;
    public float decreasingFactor;
    public bool reproduced;
    [ReadOnly] public float factor;

    public void Execute(int index)
    {
        for (int i = 0; i < positions.Length; i++)
        {
            if (Vector3.Distance(positions[i], positions[index]) < Ex4Config.TouchingDistance)
            {
               decreasingFactor *= factor;
            }
        }
    }
}