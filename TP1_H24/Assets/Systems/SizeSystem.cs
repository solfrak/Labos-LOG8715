using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizeSystem : ISystem
{
    public const int MIN_SIZE = 0;
    public string Name {get; set;}

    public SizeSystem() => Name = "SizeSystem";

    public void UpdateSystem()
    {
        foreach(var entity in BaseEntityManager.Instance.GetEntities())
        {
            PhysicComponent physicComponent = BaseEntityManager.Instance.GetComponent<PhysicComponent>(entity);
            CollisionComponent coll = BaseEntityManager.Instance.GetComponent<CollisionComponent>(entity);
            int size = CalculateSize(coll.augmentSizeCollision, coll.diminishSizeCollision, coll.initialSize);

            physicComponent.size = size;
            BaseEntityManager.Instance.UpdateComponent(entity, physicComponent);
            ECSController.Instance.UpdateShapeSize(entity, size);

            if (size <= MIN_SIZE)
            {
                DestroyComponent destroyComponent = BaseEntityManager.Instance.GetComponent<DestroyComponent>(entity);
                destroyComponent.toDestroy = true;
                BaseEntityManager.Instance.UpdateComponent(entity, destroyComponent);
            }
        }
    }

    private int CalculateSize(int greaterCount, int lesserCount, int initialSize)
    {
        int diff = greaterCount - lesserCount;

        initialSize += diff;
        return initialSize;
    }

}
