
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditorInternal.VR;
using UnityEngine;
using Unity.Burst;

[BurstCompile]
public struct JobPreyLifeTime : IJobParallelFor
{


    [ReadOnly] public NativeArray<Vector3> predatorPositions;
    [ReadOnly] public NativeArray<Vector3> plantPositions;
    [ReadOnly] public NativeArray<Vector3> preyPositions;
    public NativeArray<float> decreasingFactors;
    public bool reproduced;
    [ReadOnly] public float factor;

    public void Execute(int index)
    {
        decreasingFactors[index] = 1.0f;
        for (int i = 0; i < plantPositions.Length; i++)
        {
            if (Vector3.Distance(plantPositions[i], preyPositions[index]) < Ex4Config.TouchingDistance)
            {
                decreasingFactors[index] /= 2;
                break;
            }
        }

        for (int i = 0; i < predatorPositions.Length; i++)
        {
            if (Vector3.Distance(predatorPositions[i], preyPositions[index]) < Ex4Config.TouchingDistance)
            {
                decreasingFactors[index] *= 2f;
                break;
            }
        }

        for (int i = 0; i < preyPositions.Length; i++)
        {
            var dist = Vector3.Distance(preyPositions[i], preyPositions[index]);
            if (dist < Ex4Config.TouchingDistance && dist != 0)
            {
                reproduced = true;
                break;
            }
        }
    }
}