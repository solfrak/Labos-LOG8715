
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditorInternal.VR;
using UnityEngine;
using Unity.Burst;
using System;

[BurstCompile]
public struct MoveJob : IJobParallelFor
{
    [ReadOnly] public float speed;
    public NativeArray<Vector3> velocities;
    [ReadOnly] public NativeArray<Vector3> goToPositions;
    [ReadOnly] public NativeArray<Vector3> ourPositions;

    public void Execute(int index)
    {
        float closestDistanceSq = float.MaxValue;
        Vector3 closestPosition = Vector3.zero;

        for (int i = 0; i < goToPositions.Length; i++)
        {
            // Skip if the current position is the predator's own position
            if (i == index) continue;

            float distanceSq = (goToPositions[i] - ourPositions[index]).sqrMagnitude;
            if (distanceSq < closestDistanceSq)
            {
                closestDistanceSq = distanceSq;
                closestPosition = goToPositions[i];
            }
        }

        velocities[index] = (closestPosition - ourPositions[index]).normalized * speed;
    }
}
