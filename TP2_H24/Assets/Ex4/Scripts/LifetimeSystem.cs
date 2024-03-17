using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
public partial struct LifetimeSystem : Unity.Entities.ISystem
{
    public void OnCreate(ref SystemState state) 
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        ComponentType[] plantComponentTypes = new ComponentType[]
        {
            ComponentType.ReadWrite<LifetimeComponent>(),
            ComponentType.ReadWrite<PositionComponentData>(),
            ComponentType.ReadWrite<SizeComponentData>()
        };

        ComponentType[] preyComponentTypes = new ComponentType[]
        {
            ComponentType.ReadWrite<LifetimeComponent>(),
            ComponentType.ReadWrite<PositionComponentData>(),
            ComponentType.ReadWrite<VelocityComponent>(),
            ComponentType.ReadWrite<ReproductionComponent>(),
            ComponentType.ReadOnly<PreyComponentTag>()
        };

        ComponentType[] predatorComponentTypes = new ComponentType[]
        {
            ComponentType.ReadWrite<LifetimeComponent>(),
            ComponentType.ReadWrite<PositionComponentData>(),
            ComponentType.ReadWrite<VelocityComponent>(),
            ComponentType.ReadWrite<ReproductionComponent>(),
            ComponentType.ReadOnly<PredatorComponentTag>()
        };

        EntityArchetype plantArchetype = entityManager.CreateArchetype(plantComponentTypes);
        EntityArchetype preyArchetype = entityManager.CreateArchetype(preyComponentTypes);
        EntityArchetype predatorArchetype = entityManager.CreateArchetype(predatorComponentTypes);

        entityManager.CreateEntity(plantArchetype, 5);
        entityManager.CreateEntity(preyArchetype, 5);
        entityManager.CreateEntity(predatorArchetype, 5);

        Ex4Config config = Ex4Spawner.Instance.config;

        foreach(RefRW<LifetimeComponent> lifetimeComponent in SystemAPI.Query<RefRW<LifetimeComponent>>())
        {
            lifetimeComponent.ValueRW.TimeRemaining = 12.0f;
        }
    }

    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Queries for all Spawner components. Uses RefRW because this system wants
        // to read from and write to the component. If the system only needed read-only
        // access, it would use RefRO instead.
        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach(RefRW<LifetimeComponent> lifetimeComponent in SystemAPI.Query<RefRW<LifetimeComponent>>())
        {
            lifetimeComponent.ValueRW.TimeRemaining = lifetimeComponent.ValueRW.TimeRemaining - deltaTime * lifetimeComponent.ValueRO.DecreasingFactor;
        }
    }
}
