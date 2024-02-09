using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSystem : ISystem
{
    // pink
    public static Color EXPLOSION_SPAWNED_COLOR = new Color(1.0f, 0.3f, 1.0f);
    // orange
    public static Color NEAR_EXPLOSION_COLOR = new Color(1.0f, 0.56f, 0.11f);
    public string Name {get; set;}

    public ColorSystem(IEntityManager entityManager)
    {
        Name = "ColorSystem";
        EntityManager = entityManager;
    }
    private IEntityManager EntityManager;

    public void UpdateSystem()
    {
        var entities = EntityManager.GetEntities();
        Update(entities);
    }

    public void UpdateLeftSide()
    {
        var entities = Utils.GetLeftSideEntities(EntityManager);
        Update(entities);
    }

    private void Update(List<uint> entities)
    {
        foreach (var entity in entities)
        {
            CollisionComponent collisionComponent = EntityManager.GetComponent<CollisionComponent>(entity);
            ColorComponent colorComponent = EntityManager.GetComponent<ColorComponent>(entity);
            PhysicComponent physicComponent = EntityManager.GetComponent<PhysicComponent>(entity);
            ProtectionComponent protectionComponent = EntityManager.GetComponent<ProtectionComponent>(entity);

            if (physicComponent.isStatic)
            {
                colorComponent.color = Color.red;
            }
            else if (collisionComponent.CollisionCount > 0)
            {
                colorComponent.color = Color.green;
            }
            else if (protectionComponent.ProtectionState == ProtectionComponent.State.ACTIVE)
            {
                colorComponent.color = Color.white;
            }
            else if (protectionComponent.ProtectionState == ProtectionComponent.State.READY)
            {
                colorComponent.color = Color.cyan;
            }
            else if (protectionComponent.ProtectionState == ProtectionComponent.State.COOLDOWN)
            {
                colorComponent.color = Color.yellow;
            }
            else if (physicComponent.size + 1 == ECSController.Instance.Config.explosionSize)
            {
                // Orange
                colorComponent.color = NEAR_EXPLOSION_COLOR;
            }
            else if (collisionComponent.CollisionCount == 0)
            {
                colorComponent.color = Color.blue;
            }
            ECSController.Instance.UpdateShapeColor(entity, colorComponent.color);
            EntityManager.UpdateComponent(entity, colorComponent);
        }
    }
}
