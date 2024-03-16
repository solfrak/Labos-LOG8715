
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditorInternal.VR;
using UnityEngine;
using Unity.Burst;

[BurstCompile]
public struct PredatorLifeTimeJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<Vector3> predatorPositions;

    [ReadOnly] public NativeArray<Vector3> preyPositions;
    public NativeArray<float> decreasingFactors;
    public bool reproduced;

    public void Execute(int index)
    {
        decreasingFactors[index] = 1.0f;
        for (int i = 0; i < predatorPositions.Length; i++)
        {
            if (Vector3.Distance(predatorPositions[i], predatorPositions[index]) < Ex4Config.TouchingDistance)
            {
                reproduced = true;
                break;
            }
        }
        
        for (int i = 0; i < preyPositions.Length; i++)
        {
            if (Vector3.Distance(preyPositions[i], predatorPositions[index]) < Ex4Config.TouchingDistance)
            {
                decreasingFactors[index] /= 2;
            }
        }
    }
}