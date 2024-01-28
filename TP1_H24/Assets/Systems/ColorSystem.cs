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
            if(!physicComponent.isStatic)
            {
                if(collisionComponent.augmentSizeCollision == 0 && collisionComponent.diminishSizeCollision == 0)
                {
                    colorComponent.color = Color.blue;

                }
                else
                {
                    colorComponent.color = Color.green;
                }
            }
            ECSController.Instance.UpdateShapeColor(entity, colorComponent.color);
            BaseEntityManager.Instance.UpdateComponent(entity, colorComponent);
        }
    }
}
