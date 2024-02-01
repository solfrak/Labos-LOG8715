using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementSystem : ISystem
{
    public string Name { get; set;}
    public MovementSystem(IEntityManager entityManager)
    {
        Name = "MovementSystem";
        EntityManager = entityManager;

    }
    private IEntityManager EntityManager;

    public void UpdateSystem()
    {
        List<uint> list = EntityManager.GetEntities();

        foreach(var entity in list)
        {
            PhysicComponent physicComponent = EntityManager.GetComponent<PhysicComponent>(entity);
            physicComponent.position += physicComponent.velocity * Time.deltaTime;

            EntityManager.UpdateComponent(entity, physicComponent);

            ECSController.Instance.UpdateShapePosition(entity, physicComponent.position);
        }
        
    }
}
