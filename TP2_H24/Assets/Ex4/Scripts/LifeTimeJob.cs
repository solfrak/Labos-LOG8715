using Unity.Burst;
using Unity.Jobs;
using UnityEngine;
using Unity.Collections;

[BurstCompile]
public struct LifeTimeJob : IJob
{
    public float decreasingFactor;
    [ReadOnly] public NativeArray<Vector3> firstPosition;
    [ReadOnly] public Vector3 stuffPosition;
    [ReadOnly] public float factor;

    public void Execute()
    {
        decreasingFactor = 1.0f;
        for (int i = 0; i < firstPosition.Length; i++)
        {
            if (Vector3.Distance(firstPosition[i], stuffPosition) < Ex4Config.TouchingDistance)
            {
                decreasingFactor *= factor;
                break;
            }
        }
    }
}