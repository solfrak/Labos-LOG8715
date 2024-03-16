
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditorInternal.VR;
using UnityEngine;
using Unity.Burst;

[BurstCompile]
public struct JobPlantLifeTime : IJobParallelFor
{
    [ReadOnly]public NativeArray<Vector3> plantPositions;
    [ReadOnly] public NativeArray<Vector3> preyPositions;
    public NativeArray<float> decreasingFactors;

    public void Execute(int index)
    {
        decreasingFactors[index] = 1.0f;

        for (int i = 0; i < preyPositions.Length; i++)
        {
            float distanceSq = (preyPositions[i] - plantPositions[index]).sqrMagnitude;
            if (distanceSq < Ex4Config.TouchingDistance * Ex4Config.TouchingDistance)
            {
                decreasingFactors[index] *= 2;
                break;
            }
        }
    }
}