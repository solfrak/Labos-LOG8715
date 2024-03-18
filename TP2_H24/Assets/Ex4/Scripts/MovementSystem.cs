using System;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

[BurstCompile]
public partial struct MovementSystem : Unity.Entities.ISystem
{
    EntityQuery query;
    public void OnCreate(ref SystemState state)
    {
        query = state.GetEntityQuery(ComponentType.ReadOnly<VelocityComponent>(), ComponentType.ReadWrite<LocalTransform>());
        state.RequireForUpdate(query);
    }

    public void OnDestroy(ref SystemState state) { }


    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if(query == null)
            return;

        float deltaTime = SystemAPI.Time.DeltaTime;
        var entities = query.ToEntityArray(AllocatorManager.Temp);

        for (int i = 0; i < entities.Length; i++)
        {
            var velocityComponent = SystemAPI.GetComponentRO<VelocityComponent>(entities[i]);
            var localTransform = SystemAPI.GetComponentRW<LocalTransform>(entities[i]);
            localTransform.ValueRW.Position = localTransform.ValueRW.Position + deltaTime * velocityComponent.ValueRO.Velocity;
        }

        entities.Dispose();
    }
}
