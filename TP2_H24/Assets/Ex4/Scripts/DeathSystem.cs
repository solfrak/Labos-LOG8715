using Unity.Burst;
using Unity.Collections;
using Unity.Entities;


[BurstCompile]
public partial struct DeathSystem : Unity.Entities.ISystem
{
    EntityQuery query;
    public void OnCreate(ref SystemState state)
    {
        query = state.GetEntityQuery(ComponentType.ReadWrite<DeathComponentTag>());
    }

    public void OnDestroy(ref SystemState state) { }


    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var entities = query.ToEntityArray(AllocatorManager.Temp);

        for(int i = 0; i < entities.Length; ++i)
        {
            state.EntityManager.DestroyEntity(entities[i]);
        }

        entities.Dispose();
    }
}
