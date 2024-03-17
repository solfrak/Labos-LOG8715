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

        for (int i = 0; i < entities.Length; i++)
        {
            var positionComponent = SystemAPI.GetComponentRW<PositionComponentData>(entities[i]);
            var velocityComponent = SystemAPI.GetComponentRO<VelocityComponent>(entities[i]);
            var localTransform = SystemAPI.GetComponentRW<LocalTransform>(entities[i]);
            positionComponent.ValueRW.Position = positionComponent.ValueRW.Position + deltaTime * velocityComponent.ValueRO.Velocity;
            localTransform.ValueRW.Position = new float3(positionComponent.ValueRW.Position.x, positionComponent.ValueRW.Position.y, 0);

        }
        //foreach((RefRW<LocalTransform> localTransform, RefRO<PositionComponentData> Position, RefRO<VelocityComponent> Velocity) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<PositionComponentData>, RefRO<VelocityComponent>>())
        // {
        //     var position = Position.ValueRO.Position + deltaTime * Velocity.ValueRO.Velocity;
        //     localTransform.ValueRW.Position = localTransform.ValueRW.Position + new float3(position.x, position.y, 0);
        // }
        //entities.Dispose();
    }
}