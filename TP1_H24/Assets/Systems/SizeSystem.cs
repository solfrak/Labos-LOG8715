using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizeSystem : ISystem
{
    public string Name {get; set;}

    public SizeSystem() => Name = "SizeSystem";

    public void UpdateSystem()
    {
        foreach(var entity in BaseEntityManager.Instance.GetEntities())
        {
            PhysicComponent physicComponent = (PhysicComponent)BaseEntityManager.Instance.GetComponent<PhysicComponent>(entity);
            CollisionComponent coll = (CollisionComponent)BaseEntityManager.Instance.GetComponent<CollisionComponent>(entity);
            int size = CalculateSize(coll.augmentSizeCollision, coll.diminishSizeCollision, coll.initialSize);

            physicComponent.size = size;
            BaseEntityManager.Instance.UpdateComponent(entity, physicComponent);
            ECSController.Instance.UpdateShapeSize(entity, size);
        }
    }

    private int CalculateSize(int greaterCount, int lesserCount, int initialSize)
    {
        int diff = greaterCount - lesserCount;

        initialSize += diff;
        return initialSize;
    }

}
