using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroySystem : ISystem
{
    public string Name {get; set;}

    public DestroySystem(IEntityManager entityManager) {
        Name = "DestroySystem";
        EntityManager = entityManager;
    }
    private IEntityManager EntityManager;

    public void UpdateSystem()
    {

        List<uint> elementToDestroy = new List<uint>();
        foreach(var entity in EntityManager.GetEntities())
        {
            PhysicComponent physicComponent = EntityManager.GetComponent<PhysicComponent>(entity);
            DestroyComponent destroyComponent = EntityManager.GetComponent<DestroyComponent>(entity);

            if (destroyComponent.toDestroy)
            {
                ECSController.Instance.DestroyShape(entity);
                elementToDestroy.Add(entity);
                continue;
            }

            if (physicComponent.size <= 0)
            {
                ECSController.Instance.DestroyShape(entity);
                elementToDestroy.Add(entity);
            }
        }

        foreach(var entity in elementToDestroy)
        {
            EntityManager.DestroyEntity(entity);
        }
    }
}