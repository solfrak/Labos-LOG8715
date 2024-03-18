using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;


[BurstCompile]
public partial struct SizeSystem : Unity.Entities.ISystem
{
    EntityQuery plantQuery;

    public void OnCreate(ref SystemState state)
    {
        plantQuery = state.GetEntityQuery(ComponentType.ReadWrite<LocalTransform>(), ComponentType.ReadOnly<PlantComponentTag>(), ComponentType.ReadOnly<LifetimeComponent>());
    }

    public void OnDestroy(ref SystemState state) { }


    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var entities = plantQuery.ToEntityArray(AllocatorManager.Temp);


        for(int i = 0; i < entities.Length; ++i)
        {
            var localTransform = SystemAPI.GetComponentRW<LocalTransform>(entities[i]);
            var lifetimeComponent = SystemAPI.GetComponentRO<LifetimeComponent>(entities[i]);

            float size = lifetimeComponent.ValueRO.TimeRemaining / lifetimeComponent.ValueRO.StartingLifetime;
            localTransform.ValueRW.Scale = size;
        }

        entities.Dispose();
    }
}
