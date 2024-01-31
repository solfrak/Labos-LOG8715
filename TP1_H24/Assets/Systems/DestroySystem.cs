using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroySystem : ISystem
{
    public string Name {get; set;}

    public DestroySystem() => Name = "DestroySystem";

    public void UpdateSystem()
    {

        List<uint> elementToDestroy = new List<uint>();
        foreach(var entity in BaseEntityManager.Instance.GetEntities())
        {
            PhysicComponent physicComponent = BaseEntityManager.Instance.GetComponent<PhysicComponent>(entity);

            if(physicComponent.size <= 0)
            {
                ECSController.Instance.DestroyShape(entity);
                elementToDestroy.Add(entity);
            }
        }

        foreach(var entity in elementToDestroy)
        {
            BaseEntityManager.Instance.DestroyEntity(entity);
        }
    }
}