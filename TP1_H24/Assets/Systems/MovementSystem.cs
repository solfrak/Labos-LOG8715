using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementSystem : ISystem
{
    public string Name { get; set;}
    public MovementSystem() => Name = "MovementSystem";

    public void UpdateSystem()
    {
        List<uint> list = BaseEntityManager.Instance.GetEntities();

        foreach(var entity in list)
        {
            PhysicComponent physicComponent = BaseEntityManager.Instance.GetComponent<PhysicComponent>(entity);
            physicComponent.position += physicComponent.velocity * Time.deltaTime;

            BaseEntityManager.Instance.UpdateComponent(entity, physicComponent);

            ECSController.Instance.UpdateShapePosition(entity, physicComponent.position);
        }
        
    }
}
