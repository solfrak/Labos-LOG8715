using System;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

[BurstCompile]
public partial struct LifetimeSystem : Unity.Entities.ISystem
{
    EntityQuery query;
    public void OnCreate(ref SystemState state) 
    {
        query = state.GetEntityQuery(ComponentType.ReadWrite<LifetimeComponent>(), ComponentType.ReadOnly<ReproductionComponent>());
    }

    public void OnDestroy(ref SystemState state) { }


    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        var entities = query.ToEntityArray(AllocatorManager.Temp);

        for(int i = 0; i < entities.Length; ++i)
        {
            var lifetimeComponent = SystemAPI.GetComponentRW<LifetimeComponent>(entities[i]);
            lifetimeComponent.ValueRW.TimeRemaining = lifetimeComponent.ValueRW.TimeRemaining - deltaTime * lifetimeComponent.ValueRO.DecreasingFactor;

            if(lifetimeComponent.ValueRO.TimeRemaining < 0)
            {
                var reproduceComponent = SystemAPI.GetComponentRO<ReproductionComponent>(entities[i]);
                if(reproduceComponent.ValueRO.Reproduces)
                {
                    state.EntityManager.AddComponent<RespawnComponentTag>(entities[i]);
                }
                else
                {
                    state.EntityManager.AddComponent<DeathComponentTag>(entities[i]);
                }
            }
        }


        entities.Dispose();
    }
}
