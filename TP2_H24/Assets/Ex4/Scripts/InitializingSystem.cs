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

            var plantEntities = entityManager.Instantiate(plantPrefabEntity, config.plantCount, Allocator.Temp);
            var preyEntities = entityManager.Instantiate(preyPrefabEntity, config.preyCount, Allocator.Temp);
            var predatorEntities = entityManager.Instantiate(predatorPrefabEntity, config.predatorCount, Allocator.Temp);

            EntityArchetype plantArchetype = entityManager.CreateArchetype(plantComponentTypes);
            EntityArchetype preyArchetype = entityManager.CreateArchetype(preyComponentTypes);
            EntityArchetype predatorArchetype = entityManager.CreateArchetype(predatorComponentTypes);


            for (int i = 0; i < config.plantCount; i++)
            {
            foreach(var componentType in plantComponentTypes)
            {
                    entityManager.AddComponent(plantEntities[i], componentType);
            }
                entityManager.SetArchetype(plantEntities[i], plantArchetype);
                entityManager.SetComponentData(plantEntities[i],
                                      new LocalTransform
                                      {
                                          Position = new float3(Random.Range(-halfWidth, halfWidth), Random.Range(-halfHeight, halfHeight), 0)
                    ,
                                          Rotation = quaternion.identity,
                                          Scale = 1f
                                      }

                    );
            }

            for (int i = 0; i < config.preyCount; i++)
            {
                entityManager.SetArchetype(preyEntities[i], preyArchetype);

                entityManager.SetComponentData(preyEntities[i],
                    new LocalTransform
                    {
                        Position = new float3(Random.Range(-halfWidth, halfWidth), Random.Range(-halfHeight, halfHeight), 0)
                    ,
                        Rotation = quaternion.identity,
                        Scale = 1f
                    }

                    );
            }

            for (int i = 0; i < config.predatorCount; i++)
            {
                entityManager.SetArchetype(predatorEntities[i], predatorArchetype);
                entityManager.SetComponentData(predatorEntities[i], new LocalTransform
                {
                    Position = new float3(Random.Range(-halfWidth, halfWidth), Random.Range(-halfHeight, halfHeight), 0)
                    ,
                    Rotation = quaternion.identity,
                    Scale = 1f
                }

                    );
            }

            //entityManager.SetArchetype(plantEntity, plantArchetype);
            //entityManager.SetArchetype(predatorEntity, predatorArchetype);
            //entityManager.CreateEntity(plantArchetype, config.plantCount);
            //entityManager.CreateEntity(preyArchetype, config.preyCount);
            //entityManager.CreateEntity(predatorArchetype, config.predatorCount);
        }
    }
}
