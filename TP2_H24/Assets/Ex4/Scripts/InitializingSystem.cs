using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public partial struct InitializingSystem : Unity.Entities.ISystem
{
    EntityQuery configQuery;

    public void OnCreate(ref SystemState state)
    {
        configQuery = SystemAPI.QueryBuilder().WithAll<ConfigWrapperComponent>().Build();

        state.RequireForUpdate(configQuery);
    }

    public void OnUpdate(ref SystemState state) 
    {
        var entitiesQuery = SystemAPI.QueryBuilder().WithAll<LifetimeComponent>().Build();
         
        // If there are no entities, create them
        bool areEntitiesInitialized = !entitiesQuery.IsEmpty;
        ConfigWrapperComponent config = configQuery.GetSingleton<ConfigWrapperComponent>();

        if(!areEntitiesInitialized)
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


            EntityArchetype plantArchetype = entityManager.CreateArchetype(plantComponentTypes);
            EntityArchetype preyArchetype = entityManager.CreateArchetype(preyComponentTypes);
            EntityArchetype predatorArchetype = entityManager.CreateArchetype(predatorComponentTypes);

            entityManager.CreateEntity(plantArchetype, config.NbPlants);
            entityManager.CreateEntity(preyArchetype, config.NbPreys);
            entityManager.CreateEntity(predatorArchetype, config.NbPredators);
        }
    }
}