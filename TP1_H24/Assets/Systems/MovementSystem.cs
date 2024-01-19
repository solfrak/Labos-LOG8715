using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementSystem : ISystem
{
    public string Name { get; set;}
    public MovementSystem() => Name = "MovementSystem";

    public void UpdateSystem()
    {
        foreach(var entity in EntityManager.Instance.GetEntities())
        {
            PhysicComponent phys = entity.GetComponent<PhysicComponent>();
            phys.position += phys.velocity;
            entity.UpdateComponent(phys);
            ECSController.Instance.UpdateShapePosition(entity.id, entity.GetComponent<PhysicComponent>().position);
        }
    }
}
