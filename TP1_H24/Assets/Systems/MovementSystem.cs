using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementSystem : ISystem
{
    public string Name { get; set;}
    public MovementSystem() => Name = "MovementSystem";

    public void UpdateSystem()
    {
        List<Entity> list = EntityManager.Instance.GetEntities();

        foreach(var entity in list)
        {
            PhysicComponent physicComponent = (PhysicComponent)EntityManager.Instance.GetComponents<PhysicComponent>(entity);
            physicComponent.position += physicComponent.velocity;

            EntityManager.Instance.UpdateComponent(entity, physicComponent);
            ECSController.Instance.UpdateShapePosition(physicComponent.entityId, physicComponent.position);
        }
        
    }
}
