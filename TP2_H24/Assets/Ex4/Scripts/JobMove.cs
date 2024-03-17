
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
    [ReadOnly] public NativeArray<float2> goToPositions;
    [ReadOnly] public NativeArray<float2> ourPositions;

    public void Execute(int index)
    {
        float closestDistanceSq = float.MaxValue;
        float2 closestPosition = float2.zero;

        for (int i = 0; i < goToPositions.Length; i++)
        {
            // Skip if the current position is the predator's own position
            if (i == index) continue;

            float2 difference = (goToPositions[i] - ourPositions[index]);
            float distanceSq = difference.x * difference.x + difference.y * difference.y;
            if (distanceSq < closestDistanceSq)
            {
                closestDistanceSq = distanceSq;
                closestPosition = goToPositions[i];
            }
        }

        float2 direction = (closestPosition - ourPositions[index]);
        velocities[index].ValueRW.Velocity = math.normalizesafe(direction) * speed;
    }
}
