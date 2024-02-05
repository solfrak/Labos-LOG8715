using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizeSystem : ISystem
{
    public const int MIN_SIZE = 0;
    public string Name {get; set;}

    public SizeSystem(IEntityManager entityManager)
    {
        Name = "SizeSystem";
        EntityManager = entityManager;
    }
    private IEntityManager EntityManager;

    public void UpdateSystem()
    {
        var entities = EntityManager.GetEntities();
        Update(entities);
    }

    public void UpdateLeftSide()
    {
        var entities = Utils.GetLeftSideEntities(EntityManager);
        Update(entities);
    }

    private void Update(List<uint> entities)
    {
        foreach (var entity in entities)
        {
            PhysicComponent physicComponent = EntityManager.GetComponent<PhysicComponent>(entity);
            CollisionComponent coll = EntityManager.GetComponent<CollisionComponent>(entity);
            int size = CalculateSize(coll.changeSizeCollision, coll.initialSize);

            physicComponent.size = size;
            EntityManager.UpdateComponent(entity, physicComponent);
            ECSController.Instance.UpdateShapeSize(entity, size);

            if (size <= MIN_SIZE)
            {
                DestroyComponent destroyComponent = EntityManager.GetComponent<DestroyComponent>(entity);
                destroyComponent.toDestroy = true;
                EntityManager.UpdateComponent(entity, destroyComponent);
            }
        }
    }

    private int CalculateSize(int change, int initialSize)
    {
        initialSize += change;
        return initialSize;
    }

}
