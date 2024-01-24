using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementSystem : ISystem
{
    public string Name { get; set;}
    public MovementSystem() => Name = "MovementSystem";

    public void UpdateSystem()
    {
        List<IComponent> list = EntityManager.Instance.GetComponents<PhysicComponent>();
        for(int i = 0; i < list.Count; i++)
        {
            PhysicComponent physicComponent = (PhysicComponent)list[i];
            physicComponent.position += physicComponent.velocity;

            EntityManager.Instance.UpdateComponent(i, physicComponent);

            ECSController.Instance.UpdateShapePosition(physicComponent.entityId, physicComponent.position);
        }
        
    }
}
