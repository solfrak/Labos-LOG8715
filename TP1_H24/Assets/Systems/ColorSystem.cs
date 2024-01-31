using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSystem : ISystem
{
    public string Name {get; set;}

    public ColorSystem() => Name = "ColorSystem";

    public void UpdateSystem()
    {
        var entities = BaseEntityManager.Instance.GetEntities();
        foreach(var entity in entities)
        {
            CollisionComponent collisionComponent = (CollisionComponent)BaseEntityManager.Instance.GetComponent<CollisionComponent>(entity);
            ColorComponent colorComponent = (ColorComponent)BaseEntityManager.Instance.GetComponent<ColorComponent>(entity);
            PhysicComponent physicComponent = (PhysicComponent)BaseEntityManager.Instance.GetComponent<PhysicComponent>(entity);
            ProtectionStat protectionComponent = (ProtectionStat) BaseEntityManager.Instance.GetComponent<ProtectionStat>(entity);
            if (physicComponent.isStatic)
            {
                colorComponent.color = Color.red;
            }
            else if(collisionComponent.CollisionCount > 0)
            {
                colorComponent.color = Color.green;
            }
            else if(protectionComponent.ProtectionState == ProtectionStat.State.ACTIVE)
            {
                colorComponent.color = Color.white;
            }
            else if (protectionComponent.ProtectionState == ProtectionStat.State.READY)
            {
                colorComponent.color = Color.cyan;
            }
            else if (protectionComponent.ProtectionState == ProtectionStat.State.COOLDOWN)
            {
                colorComponent.color = Color.yellow;
            }
            else if (physicComponent.size + 1 == ECSController.Instance.Config.explosionSize)
            {
                // Orange
                colorComponent.color = new Color(255, 165, 0);
            }
            else if (collisionComponent.CollisionCount == 0)
            {
                colorComponent.color = Color.blue;
            }
            ECSController.Instance.UpdateShapeColor(entity, colorComponent.color);
            BaseEntityManager.Instance.UpdateComponent(entity, colorComponent);
        }
    }
}
