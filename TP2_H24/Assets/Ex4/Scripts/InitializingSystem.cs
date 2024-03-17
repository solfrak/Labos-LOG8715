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
        var entitiesQuery = SystemAPI.QueryBuilder().WithAll<LifetimeComponent>().Build();

        // If there are no entities, create them
        bool areEntitiesInitialized = !entitiesQuery.IsEmpty;
        //Ex4Config config = Ex4Spawner.Instance.config;
        SpawnerConfig config = SystemAPI.GetSingleton<SpawnerConfig>();
        int halfWidth = Ex4Spawner.Instance.Width / 2;
        int halfHeight = Ex4Spawner.Instance.Height / 2;
        if (!areEntitiesInitialized)
        {
            EntityManager entityManager = state.EntityManager;

            // Create all entities
            ComponentType[] plantComponentTypes = new ComponentType[]
            {
                ComponentType.ReadWrite<LifetimeComponent>(),
                ComponentType.ReadWrite<PositionComponentData>(),
                ComponentType.ReadWrite<SizeComponentData>(),
                ComponentType.ReadWrite<ReproductionComponent>(),
                ComponentType.ReadOnly<PlantComponentTag>(),
                ComponentType.ReadOnly<RespawnComponentTag>(),
            };

            ComponentType[] preyComponentTypes = new ComponentType[]
            {
                ComponentType.ReadWrite<LifetimeComponent>(),
                ComponentType.ReadWrite<PositionComponentData>(),
                ComponentType.ReadWrite<VelocityComponent>(),
                ComponentType.ReadWrite<ReproductionComponent>(),
                ComponentType.ReadOnly<PreyComponentTag>(),
                ComponentType.ReadOnly<RespawnComponentTag>(),
            };

            ComponentType[] predatorComponentTypes = new ComponentType[]
            {
                ComponentType.ReadWrite<LifetimeComponent>(),
                ComponentType.ReadWrite<PositionComponentData>(),
                ComponentType.ReadWrite<VelocityComponent>(),
                ComponentType.ReadWrite<ReproductionComponent>(),
                ComponentType.ReadOnly<PredatorComponentTag>(),
                ComponentType.ReadOnly<RespawnComponentTag>(),
            };
            var plantPrefabEntity = config.plantPrefabEntity;
            var preyPrefabEntity = config.preyPrefabEntity;
            var predatorPrefabEntity = config.predatorPrefabEntity;

            var plantEntity = entityManager.Instantiate(plantPrefabEntity, config.plantCount, Allocator.Temp);
            var preyEntity = entityManager.Instantiate(preyPrefabEntity, config.preyCount, Allocator.Temp);
            var predatorEntity = entityManager.Instantiate(predatorPrefabEntity, config.predatorCount, Allocator.Temp);

            EntityArchetype plantArchetype = entityManager.CreateArchetype(plantComponentTypes);
            EntityArchetype preyArchetype = entityManager.CreateArchetype(preyComponentTypes);
            EntityArchetype predatorArchetype = entityManager.CreateArchetype(predatorComponentTypes);
            for (int i = 0; i < config.preyCount; i++)
            {
                entityManager.SetArchetype(preyEntity[i], preyArchetype);

            }   
            for (int i = 0; i < config.plantCount; i++)
            {   var position = new float3(Random.Range(-halfWidth, halfWidth), Random.Range(-halfHeight, halfHeight), Random.Range(-halfHeight, halfHeight));
                entityManager.SetArchetype(plantEntity[i], plantArchetype);
                entityManager.SetComponentData(plantEntity[i], new LocalTransform {
                Position = position, Scale = 1f, Rotation = quaternion.identity
                 });
            }
            for (int i = 0; i < config.predatorCount; i++)
            {
                entityManager.SetArchetype(predatorEntity[i], predatorArchetype);
            }

            //entityManager.SetArchetype(plantEntity, plantArchetype);
            //entityManager.SetArchetype(predatorEntity, predatorArchetype);
            //entityManager.CreateEntity(plantArchetype, config.plantCount);
            //entityManager.CreateEntity(preyArchetype, config.preyCount);
            //entityManager.CreateEntity(predatorArchetype, config.predatorCount);
        }
    }
}
