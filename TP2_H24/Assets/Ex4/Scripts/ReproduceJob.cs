using Unity.Burst;
using Unity.Jobs;
using UnityEngine;
using Unity.Collections;

[BurstCompile]
public struct ReproduceJob : IJob
{
    public bool reproduce;
    [ReadOnly] public NativeArray<Vector3> firstPosition;
    [ReadOnly] public Vector3 stuffPosition;

    public void Execute()
    {
        for (int i = 0; i < firstPosition.Length; i++)
        {
            if (Vector3.Distance(firstPosition[i], stuffPosition) < Ex4Config.TouchingDistance)
            {
                reproduce = true;
                break;
            }
        }
    }
}