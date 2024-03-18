using System;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

[BurstCompile]
public partial struct MovementSystem : Unity.Entities.ISystem
{
    EntityQuery query;
    public void OnCreate(ref SystemState state)
    {
        query = state.GetEntityQuery(ComponentType.ReadOnly<VelocityComponent>(), ComponentType.ReadWrite<PositionComponentData>());
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

        for(int i = 0; i < entities.Length; i++)
        {
            var positionComponent = SystemAPI.GetComponentRW<PositionComponentData>(entities[i]);
            var velocityComponent = SystemAPI.GetComponentRO<VelocityComponent>(entities[i]);
            positionComponent.ValueRW.Position = positionComponent.ValueRW.Position + deltaTime * velocityComponent.ValueRO.Velocity;
        }

        entities.Dispose();
    }
}
