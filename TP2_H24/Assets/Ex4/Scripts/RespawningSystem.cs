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
public partial struct RespawningSystem : Unity.Entities.ISystem
{
    private const float StartingLifetimeLowerBound = 5;
    private const float StartingLifetimeUpperBound = 15;
    private const float StartingDecreasingFactor = 1;
    private const float StartingSize = 1;

    EntityQuery plantQuery;
    EntityQuery preyQuery;
    EntityQuery predatorQuery;

    public void OnCreate(ref SystemState state)
    {
        predatorQuery = state.GetEntityQuery(ComponentType.ReadWrite<LifetimeComponent>(), ComponentType.ReadWrite<PositionComponentData>(),
                    ComponentType.ReadWrite<ReproductionComponent>(), ComponentType.ReadWrite<RespawnComponentTag>(), ComponentType.ReadOnly<PredatorComponentTag>());
        preyQuery = state.GetEntityQuery(ComponentType.ReadWrite<LifetimeComponent>(), ComponentType.ReadWrite<PositionComponentData>(),
            ComponentType.ReadWrite<ReproductionComponent>(), ComponentType.ReadWrite<RespawnComponentTag>(), ComponentType.ReadOnly<PreyComponentTag>());
        plantQuery = state.GetEntityQuery(ComponentType.ReadWrite<LifetimeComponent>(), ComponentType.ReadWrite<PositionComponentData>(),
            ComponentType.ReadWrite<ReproductionComponent>(), ComponentType.ReadWrite<RespawnComponentTag>(), ComponentType.ReadOnly<PlantComponentTag>());

        state.RequireAnyForUpdate(predatorQuery, preyQuery, plantQuery);
    }

    public void OnDestroy(ref SystemState state) { }


    public void OnUpdate(ref SystemState state)
    {
        var plantEntities = plantQuery.ToEntityArray(AllocatorManager.Temp);
        var predatorEntities = predatorQuery.ToEntityArray(AllocatorManager.Temp);
        var preyEntities = preyQuery.ToEntityArray(AllocatorManager.Temp);


        int halfWidth = Ex4Spawner.Instance.Width / 2;
        int halfHeight = Ex4Spawner.Instance.Height / 2;

        foreach(var entity in preyEntities)
        {
            // Initialize the random position
            var positionComponent = SystemAPI.GetComponentRW<PositionComponentData>(entity);
            positionComponent.ValueRW.Position = new float2(Random.Range(-halfWidth, halfWidth), Random.Range(-halfHeight, halfHeight));

            // Initialize starting life
            var lifetimeComponent = SystemAPI.GetComponentRW<LifetimeComponent>(entity);
            lifetimeComponent.ValueRW.StartingLifetime = Random.Range(StartingLifetimeLowerBound, StartingLifetimeUpperBound);
            lifetimeComponent.ValueRW.TimeRemaining = lifetimeComponent.ValueRO.StartingLifetime;
            lifetimeComponent.ValueRW.DecreasingFactor = StartingDecreasingFactor;

            var reproductionComponent = SystemAPI.GetComponentRW<ReproductionComponent>(entity);
            reproductionComponent.ValueRW.Reproduces = false;

            state.EntityManager.RemoveComponent<RespawnComponentTag>(entity);
        }
        foreach(var entity in predatorEntities)
        {
            // Initialize the random position
            var positionComponent = SystemAPI.GetComponentRW<PositionComponentData>(entity);
            positionComponent.ValueRW.Position = new float2(Random.Range(-halfWidth, halfWidth), Random.Range(-halfHeight, halfHeight));

            // Initialize starting life
            var lifetimeComponent = SystemAPI.GetComponentRW<LifetimeComponent>(entity);
            lifetimeComponent.ValueRW.StartingLifetime = Random.Range(StartingLifetimeLowerBound, StartingLifetimeUpperBound);
            lifetimeComponent.ValueRW.TimeRemaining = lifetimeComponent.ValueRO.StartingLifetime;
            lifetimeComponent.ValueRW.DecreasingFactor = StartingDecreasingFactor;

            var reproductionComponent = SystemAPI.GetComponentRW<ReproductionComponent>(entity);
            reproductionComponent.ValueRW.Reproduces = false;

            state.EntityManager.RemoveComponent<RespawnComponentTag>(entity);
        }

        foreach(var entity in plantEntities)
        {
            // Initialize the random position
            var positionComponent = SystemAPI.GetComponentRW<PositionComponentData>(entity);
            positionComponent.ValueRW.Position = new float2(Random.Range(-halfWidth, halfWidth), Random.Range(-halfHeight, halfHeight));

            // Initialize starting life
            var lifetimeComponent = SystemAPI.GetComponentRW<LifetimeComponent>(entity);
            lifetimeComponent.ValueRW.StartingLifetime = Random.Range(StartingLifetimeLowerBound, StartingLifetimeUpperBound);
            lifetimeComponent.ValueRW.TimeRemaining = lifetimeComponent.ValueRO.StartingLifetime;
            lifetimeComponent.ValueRW.DecreasingFactor = StartingDecreasingFactor;

            // Initialize Size
            var sizeComponent = SystemAPI.GetComponentRW<SizeComponentData>(entity);
            sizeComponent.ValueRW.Size = StartingSize;

            // All plants reproduce
            var reproductionComponent = SystemAPI.GetComponentRW<ReproductionComponent>(entity);
            reproductionComponent.ValueRW.Reproduces = true;

            state.EntityManager.RemoveComponent<RespawnComponentTag>(entity);
        }

        plantEntities.Dispose();
        predatorEntities.Dispose();
        preyEntities.Dispose();
    }
}
