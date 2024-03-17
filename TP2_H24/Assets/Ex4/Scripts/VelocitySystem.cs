using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;
using Random = UnityEngine.Random;

[BurstCompile]
public partial struct VelocitySystem : Unity.Entities.ISystem
{
    EntityQuery plantQuery;
    EntityQuery preyQuery;
    EntityQuery predatorQuery;

    public void OnCreate(ref SystemState state)
    {
        predatorQuery = state.GetEntityQuery(ComponentType.ReadWrite<VelocityComponent>(), ComponentType.ReadOnly<PositionComponentData>(),
            ComponentType.ReadOnly<PredatorComponentTag>());
        preyQuery = state.GetEntityQuery(ComponentType.ReadWrite<VelocityComponent>(), ComponentType.ReadOnly<PositionComponentData>(),
            ComponentType.ReadOnly<PreyComponentTag>());
        plantQuery = state.GetEntityQuery(ComponentType.ReadOnly<SizeComponentData>(), ComponentType.ReadOnly<PlantComponentTag>());

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

        var preyVelocities = new NativeArray<RefRW<VelocityComponent>>(preyEntities.Length, Allocator.TempJob);
        var predatorVelocities = new NativeArray<RefRW<VelocityComponent>>(predatorEntities.Length, Allocator.TempJob);
        for(int i = 0; i < predatorEntities.Length; i++)
        {
            predatorPositions[i] = SystemAPI.GetComponentRW<PositionComponentData>(predatorEntities[i]).ValueRO.Position;
            predatorVelocities[i] = SystemAPI.GetComponentRW<VelocityComponent>(predatorEntities[i]);
        }
        for(int i = 0; i < preyEntities.Length; i++)
        {
            preyPositions[i] = SystemAPI.GetComponentRW<PositionComponentData>(preyEntities[i]).ValueRO.Position;
            preyVelocities[i] = SystemAPI.GetComponentRW<VelocityComponent>(preyEntities[i]);
        }
        for(int i = 0; i < plantEntities.Length; i++)
        {
            plantPositions[i] = SystemAPI.GetComponentRW<PositionComponentData>(plantEntities[i]).ValueRO.Position;
        }

        MoveJob moveJobPredator = new MoveJob
        {
            goToPositions = preyPositions,
            ourPositions = predatorPositions,
            velocities = predatorVelocities,
            speed = Ex4Config.PredatorSpeed,
        };
        JobHandle jobHandlePredatorMovement = moveJobPredator.Schedule(predatorEntities.Length, 60);

        MoveJob moveJobPrey = new MoveJob
        {
            goToPositions = plantPositions,
            ourPositions = preyPositions,
            velocities = preyVelocities,
            speed = Ex4Config.PreySpeed,
        };
        JobHandle jobHandlePreyMovement = moveJobPrey.Schedule(preyEntities.Length, 60);

        JobHandle.CombineDependencies(jobHandlePredatorMovement, jobHandlePreyMovement).Complete();

        plantEntities.Dispose();
        plantPositions.Dispose();
        preyPositions.Dispose();
        preyEntities.Dispose();
        preyVelocities.Dispose();
        predatorEntities.Dispose();
        predatorPositions.Dispose();
        predatorVelocities.Dispose();
    }
}
