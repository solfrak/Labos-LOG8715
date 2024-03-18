using System;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

[BurstCompile]
public partial struct LifetimeFactorSystem : Unity.Entities.ISystem
{
    EntityQuery plantQuery;
    EntityQuery preyQuery;
    EntityQuery predatorQuery;

    public void OnCreate(ref SystemState state)
    {
        predatorQuery = state.GetEntityQuery(ComponentType.ReadWrite<LifetimeComponent>(), ComponentType.ReadOnly<PositionComponentData>(),
             ComponentType.ReadOnly<PredatorComponentTag>());
        preyQuery = state.GetEntityQuery(ComponentType.ReadWrite<LifetimeComponent>(), ComponentType.ReadOnly<PositionComponentData>(),
            ComponentType.ReadOnly<PreyComponentTag>());
        plantQuery = state.GetEntityQuery(ComponentType.ReadWrite<LifetimeComponent>(), ComponentType.ReadOnly<PositionComponentData>(), 
            ComponentType.ReadOnly<PlantComponentTag>());

        state.RequireAnyForUpdate(predatorQuery, preyQuery, plantQuery);
    }

    public void OnDestroy(ref SystemState state) { }


    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var plantEntities = plantQuery.ToEntityArray(AllocatorManager.Temp);
        var predatorEntities = predatorQuery.ToEntityArray(AllocatorManager.Temp);
        var preyEntities = preyQuery.ToEntityArray(AllocatorManager.Temp);

        var plantPositions = new NativeArray<float2>(plantEntities.Length, Allocator.TempJob);
        var predatorPositions = new NativeArray<float2>(predatorEntities.Length, Allocator.TempJob);
        var preyPositions = new NativeArray<float2>(preyEntities.Length, Allocator.TempJob);

        var preyLifetimes = new NativeArray<RefRW<LifetimeComponent>>(preyEntities.Length, Allocator.TempJob);
        var predatorLifetimes = new NativeArray<RefRW<LifetimeComponent>>(predatorEntities.Length, Allocator.TempJob);
        var plantLifetimes = new NativeArray<RefRW<LifetimeComponent>>(plantEntities.Length, Allocator.TempJob);

        for(int i = 0; i < predatorEntities.Length; i++)
        {
            predatorPositions[i] = SystemAPI.GetComponentRW<PositionComponentData>(predatorEntities[i]).ValueRO.Position;
            predatorLifetimes[i] = SystemAPI.GetComponentRW<LifetimeComponent>(predatorEntities[i]);
        }
        for(int i = 0; i < preyEntities.Length; i++)
        {
            preyPositions[i] = SystemAPI.GetComponentRW<PositionComponentData>(preyEntities[i]).ValueRO.Position;
            preyLifetimes[i] = SystemAPI.GetComponentRW<LifetimeComponent>(preyEntities[i]);
        }
        for(int i = 0; i < plantEntities.Length; i++)
        {
            plantPositions[i] = SystemAPI.GetComponentRW<PositionComponentData>(plantEntities[i]).ValueRO.Position;
            plantLifetimes[i] = SystemAPI.GetComponentRW<LifetimeComponent>(plantEntities[i]);
        }


        JobPlantLifeTime plantLifeTimeJob = new JobPlantLifeTime
        {
            preyPositions = preyPositions,
            plantPositions = plantPositions,
            decreasingFactors = plantLifetimes,
            touchingDistance = Ex4Config.TouchingDistance
        };

        JobHandle plantLifetimeJobHandle = plantLifeTimeJob.Schedule(plantEntities.Length, 60);

        JobPreyLifeTime preyLifeTimeJob = new JobPreyLifeTime
        {
            preyPositions = preyPositions,
            predatorPositions = predatorPositions,
            plantPositions = plantPositions,
            decreasingFactorsPreys = preyLifetimes,
            touchingDistance = Ex4Config.TouchingDistance,
        };

        JobHandle preyLifeTimeJobHandle = preyLifeTimeJob.Schedule(preyEntities.Length, 60);

        JobPredatorLifeTime predatorLifeTimeJob = new JobPredatorLifeTime
        {
            predatorPositions = predatorPositions,
            preyPositions = preyPositions,
            decreasingFactors = predatorLifetimes,
            touchingDistance = Ex4Config.TouchingDistance
        };
        JobHandle predatorLifeTimeJobHandle = predatorLifeTimeJob.Schedule(predatorEntities.Length, 60);

        JobHandle.CombineDependencies(plantLifetimeJobHandle, preyLifeTimeJobHandle, predatorLifeTimeJobHandle).Complete();

        plantEntities.Dispose();
        plantPositions.Dispose();
        plantLifetimes.Dispose();
        preyPositions.Dispose();
        preyEntities.Dispose();
        preyLifetimes.Dispose();
        predatorLifetimes.Dispose();
        predatorEntities.Dispose();
        predatorPositions.Dispose();
    }
}
