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
        int halfWidth = config.width / 2;
        int halfHeight = config.height / 2;

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
            //Entity entity = state.EntityManager.Instantiate(config.PreyPrefab);
            ////var transform = state.EntityManager.GetComponentData<LocalTransform>(entity);

            //transform.Position.x = Random.Range(-config.width / 2, config.width / 2);
            //transform.Position.y = Random.Range(-config.height / 2, config.height / 2);
            //state.EntityManager.SetComponentData(entity, transform);
            EntityArchetype plantArchetype = entityManager.CreateArchetype(plantComponentTypes);
            EntityArchetype preyArchetype = entityManager.CreateArchetype(preyComponentTypes);
            EntityArchetype predatorArchetype = entityManager.CreateArchetype(predatorComponentTypes);


            state.EntityManager.AddComponent(plantEntities, plantComponentTypeSet);
            state.EntityManager.AddComponent(preyEntities, preyComponentTypeSet);
            state.EntityManager.AddComponent(predatorEntities, predatorComponentTypeSet);
            //for (int i = 0; i < config.plantCount; i++)
            //{
            //    var transform = state.EntityManager.GetComponentData<LocalTransform>(plantEntities[i]);
            //    transform.Position.x = Random.Range(-config.width / 2, config.width / 2);
            //    transform.Position.y = Random.Range(-config.height / 2, config.height / 2);
            //    state.EntityManager.SetComponentData(plantEntities[i], transform);
            //}

            //for (int i = 0; i < config.preyCount; i++)
            //{
            //    var transform = state.EntityManager.GetComponentData<LocalTransform>(plantEntities[i]);
            //    transform.Position.x = Random.Range(-config.width / 2, config.width / 2);
            //    transform.Position.y = Random.Range(-config.height / 2, config.height / 2);
            //    state.EntityManager.SetComponentData(preyEntities[i], transform);
            //}

            //for (int i = 0; i < config.predatorCount; i++)
            //{
            //    var transform = state.EntityManager.GetComponentData<LocalTransform>(plantEntities[i]);
            //    transform.Position.x = Random.Range(-config.width / 2, config.width / 2);
            //    transform.Position.y = Random.Range(-config.height / 2, config.height / 2);
            //    state.EntityManager.SetComponentData(predatorEntities[i], transform);
            //}



            //entityManager.SetArchetype(plantEntity, plantArchetype);
            //entityManager.SetArchetype(predatorEntity, predatorArchetype);
            //entityManager.CreateEntity(plantArchetype, config.plantCount);
            //entityManager.CreateEntity(preyArchetype, config.preyCount);
            //entityManager.CreateEntity(predatorArchetype, config.predatorCount);
        }
    }
}
