using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;


[BurstCompile]
public partial struct SizeSystem : Unity.Entities.ISystem
{
    EntityQuery query;

    public void OnCreate(ref SystemState state)
    {
        query = state.GetEntityQuery(ComponentType.ReadWrite<SizeComponentData>(), ComponentType.ReadOnly<LifetimeSystem>());
    }

    public void OnDestroy(ref SystemState state) { }


    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var entities = query.ToEntityArray(AllocatorManager.Temp);

        for(int i = 0; i < entities.Length; ++i)
        {
            var sizeComponent = SystemAPI.GetComponentRW<SizeComponentData>(entities[i]);
            var lifetimeComponent = SystemAPI.GetComponentRO<LifetimeComponent>(entities[i]);

            float size = lifetimeComponent.ValueRO.TimeRemaining / lifetimeComponent.ValueRO.StartingLifetime;
            sizeComponent.ValueRW.Size = size;
        }

        entities.Dispose();
    }
}
