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
        List<uint> entities = EntityManager.GetEntities();
        Move(entities);
    }

    public void UpdateLeftSide()
    {
        List<uint> leftEntities = Utils.GetLeftSideEntities(EntityManager);
        Move(leftEntities);
    }

    private void Move(List<uint> entities)
    {
        foreach (var entity in entities)
        {
            PhysicComponent physicComponent = EntityManager.GetComponent<PhysicComponent>(entity);
            physicComponent.position += physicComponent.velocity * Time.deltaTime;

            EntityManager.UpdateComponent(entity, physicComponent);

            ECSController.Instance.UpdateShapePosition(entity, physicComponent.position);
        }
    }
}
