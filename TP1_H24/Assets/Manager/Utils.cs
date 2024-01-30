using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static void SpawnCirlce(Vector2 position, Vector2 velocity, int size)
    {
        //Add the all the required component to a new entity
        var entity = BaseEntityManager.Instance.CreateEntity();
        PhysicComponent physicComponent = new PhysicComponent { position = position,  velocity = velocity, size = size, isStatic = false };
        CollisionComponent collisionComponent = new CollisionComponent{initialSize = size};
        ColorComponent colorComponent = new ColorComponent{ color = physicComponent.isStatic ? Color.red : Color.blue};
        ProtectionStat protectionStat = new ProtectionStat { ProtectionState = ProtectionStat.State.READY };

        if (isNormVec2Null(velocity))
        {
            physicComponent.isStatic = true;
        }


        BaseEntityManager.Instance.AddComponent(entity, physicComponent);
        BaseEntityManager.Instance.AddComponent(entity, collisionComponent);
        BaseEntityManager.Instance.AddComponent(entity, colorComponent);
        BaseEntityManager.Instance.AddComponent(entity, protectionStat);

        ECSController.Instance.CreateShape(entity, physicComponent.size);
        ECSController.Instance.UpdateShapePosition(entity, physicComponent.position);
        ECSController.Instance.UpdateShapeColor(entity, colorComponent.color);
    }

    private static bool isNormVec2Null(Vector2 input)
    {
        return (input.x * input.x) + (input.y * input.y) == 0.0;
    }
} 
