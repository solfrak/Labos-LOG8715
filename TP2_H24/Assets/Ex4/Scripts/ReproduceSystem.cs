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
public partial struct ReproduceSystem : Unity.Entities.ISystem
{
    EntityQuery preyQuery;
    EntityQuery predatorQuery;

    public void OnCreate(ref SystemState state)
    {
        preyQuery = state.GetEntityQuery(ComponentType.ReadWrite<ReproductionComponent>(), ComponentType.ReadOnly<LocalTransform>(), ComponentType.ReadOnly<PreyComponentTag>());
        predatorQuery = state.GetEntityQuery(ComponentType.ReadWrite<ReproductionComponent>(), ComponentType.ReadOnly<LocalTransform>(), ComponentType.ReadOnly<PredatorComponentTag>());
        state.RequireAnyForUpdate(preyQuery, predatorQuery);
    }

    public void OnDestroy(ref SystemState state) { }


    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var preyEntities = preyQuery.ToEntityArray(AllocatorManager.Temp);
        var predatorEntities = predatorQuery.ToEntityArray(AllocatorManager.Temp);

        var predatorPositions = new NativeArray<float3>(predatorEntities.Length, Allocator.TempJob);
        var preyPositions = new NativeArray<float3>(preyEntities.Length, Allocator.TempJob);

        var preyReproduces = new NativeArray<RefRW<ReproductionComponent>>(preyEntities.Length, Allocator.TempJob);
        var predatorReproduces = new NativeArray<RefRW<ReproductionComponent>>(predatorEntities.Length, Allocator.TempJob);

        for(int i = 0; i < predatorEntities.Length; i++)
        {
            predatorPositions[i] = SystemAPI.GetComponentRO<LocalTransform>(predatorEntities[i]).ValueRO.Position;
            predatorReproduces[i] = SystemAPI.GetComponentRW<ReproductionComponent>(predatorEntities[i]);
        }
        for(int i = 0; i < preyEntities.Length; i++)
        {
            preyPositions[i] = SystemAPI.GetComponentRO<LocalTransform>(preyEntities[i]).ValueRO.Position;
            preyReproduces[i] = SystemAPI.GetComponentRW<ReproductionComponent>(preyEntities[i]);
        }

        JobReproduce reproducePredatorJob = new JobReproduce
        {
            positions = predatorPositions,
            reproduced = predatorReproduces,
            touchingDistance = Ex4Config.TouchingDistance
        };
        JobHandle jobHandlePredatorReproduce = reproducePredatorJob.Schedule(predatorEntities.Length, 60);

        JobReproduce reproducePreyJob = new JobReproduce
        {
            positions = preyPositions,
            reproduced = preyReproduces,
            touchingDistance = Ex4Config.TouchingDistance
        };
        JobHandle jobHandlePreyReproduce = reproducePreyJob.Schedule(preyEntities.Length, 60);

        JobHandle.CombineDependencies(jobHandlePredatorReproduce, jobHandlePreyReproduce).Complete();

        predatorPositions.Dispose();
        predatorEntities.Dispose();
        preyEntities.Dispose();
        preyPositions.Dispose();
        preyReproduces.Dispose();
        predatorReproduces.Dispose();
    }
}
