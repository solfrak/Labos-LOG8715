
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditorInternal.VR;
using UnityEngine;
using Unity.Burst;

[BurstCompile]
public struct JobPredatorLifeTime : IJobParallelFor
{
    [ReadOnly] public NativeArray<Vector3> predatorPositions;

    [ReadOnly] public NativeArray<Vector3> preyPositions;
    public NativeArray<float> decreasingFactors;
    public NativeArray<bool> reproduced;

    public void Execute(int index)
    {
        decreasingFactors[index] = 1.0f;
        Vector3 predatorPosition = predatorPositions[index];
        float touchingDistanceSq = Ex4Config.TouchingDistance * Ex4Config.TouchingDistance;

        for (int i = 0; i < predatorPositions.Length; i++)
        {
            if (i == index) continue; // Skip self

            float distanceSq = (predatorPositions[i] - predatorPosition).sqrMagnitude;
            if (distanceSq < touchingDistanceSq)
            {
                reproduced[index] = true;
                break;
            }
        }

        for (int i = 0; i < preyPositions.Length; i++)
        {
            float distanceSq = (preyPositions[i] - predatorPosition).sqrMagnitude;
            if (distanceSq < touchingDistanceSq)
            {
                decreasingFactors[index] /= 2;
            }
        }
    }
}