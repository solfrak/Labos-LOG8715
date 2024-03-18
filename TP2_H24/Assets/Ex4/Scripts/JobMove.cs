
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;
using System;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile]
public struct MoveJob : IJobParallelFor
{
    [ReadOnly] public float speed;
    public NativeArray<RefRW<VelocityComponent>> velocities;
    [ReadOnly] public NativeArray<float3> goToPositions;
    [ReadOnly] public NativeArray<float3> ourPositions;

    public void Execute(int index)
    {
        float closestDistanceSq = float.MaxValue;
        float3 closestPosition = float3.zero;

        for (int i = 0; i < goToPositions.Length; i++)
        {
            // Skip if the current position is the predator's own position
            if (i == index) continue;

            float3 difference = (goToPositions[i] - ourPositions[index]);
            float distanceSq = difference.x * difference.x + difference.y * difference.y;
            if (distanceSq < closestDistanceSq)
            {
                closestDistanceSq = distanceSq;
                closestPosition = goToPositions[i];
            }
        }

        float3 direction = (closestPosition - ourPositions[index]);
        velocities[index].ValueRW.Velocity = math.normalizesafe(direction) * speed;
    }
}
