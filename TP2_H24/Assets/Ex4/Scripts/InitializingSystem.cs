using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;
public partial struct InitializingSystem : Unity.Entities.ISystem
{

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpawnerConfig>();
    }


    public void OnUpdate(ref SystemState state)
    {
        SpawnerConfig config = SystemAPI.GetSingleton<SpawnerConfig>();
        var entitiesQuery = SystemAPI.QueryBuilder().WithAll<LifetimeComponent>().Build();

        // If there are no entities, create them
        bool areEntitiesInitialized = !entitiesQuery.IsEmpty;

        if (!areEntitiesInitialized)
        {
            EntityManager entityManager = state.EntityManager;

            // Create all entities
            ComponentType[] plantComponentTypes = new ComponentType[]
            {
                ComponentType.ReadWrite<LifetimeComponent>(),
                ComponentType.ReadWrite<ReproductionComponent>(),
                ComponentType.ReadOnly<PlantComponentTag>(),
                ComponentType.ReadOnly<RespawnComponentTag>(),

            };
            ComponentTypeSet plantComponentTypeSet = new ComponentTypeSet(plantComponentTypes);

            ComponentType[] preyComponentTypes = new ComponentType[]
            {
                ComponentType.ReadWrite<LifetimeComponent>(),
                ComponentType.ReadWrite<VelocityComponent>(),
                ComponentType.ReadWrite<ReproductionComponent>(),
                ComponentType.ReadOnly<PreyComponentTag>(),
                ComponentType.ReadOnly<RespawnComponentTag>(),
            };
            ComponentTypeSet preyComponentTypeSet = new ComponentTypeSet(preyComponentTypes);

            ComponentType[] predatorComponentTypes = new ComponentType[]
            {
                ComponentType.ReadWrite<LifetimeComponent>(),
                ComponentType.ReadWrite<VelocityComponent>(),
                ComponentType.ReadWrite<ReproductionComponent>(),
                ComponentType.ReadOnly<PredatorComponentTag>(),
                ComponentType.ReadOnly<RespawnComponentTag>(),
            };
            ComponentTypeSet predatorComponentTypeSet = new ComponentTypeSet(predatorComponentTypes);


            var plantPrefabEntity = config.plantPrefabEntity;
            var preyPrefabEntity = config.preyPrefabEntity;
            var predatorPrefabEntity = config.predatorPrefabEntity;

            var plantEntities = entityManager.Instantiate(plantPrefabEntity, config.plantCount, Allocator.Temp);
            var preyEntities = entityManager.Instantiate(preyPrefabEntity, config.preyCount, Allocator.Temp);
            var predatorEntities = entityManager.Instantiate(predatorPrefabEntity, config.predatorCount, Allocator.Temp);

            state.EntityManager.AddComponent(plantEntities, plantComponentTypeSet);
            state.EntityManager.AddComponent(preyEntities, preyComponentTypeSet);
            state.EntityManager.AddComponent(predatorEntities, predatorComponentTypeSet);

        }
    }
}
