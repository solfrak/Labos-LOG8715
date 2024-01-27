using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerSystem : ISystem
{
    public string Name { get; set;}
    public SpawnerSystem() => Name = "SpawnerSystem";

    private bool isInitPhase = true;

    public void UpdateSystem()
    {
        if(isInitPhase)
        {
            foreach (var circle in ECSController.Instance.Config.circleInstancesToSpawn)
            {
                var entity = BaseEntityManager.Instance.CreateEntity();
                PhysicComponent physicComponent = new PhysicComponent { position = circle.initialPosition,  velocity = circle.initialVelocity, size = circle.initialSize, isStatic = false };
                CollisionComponent collisionComponent = new CollisionComponent{initialSize = circle.initialSize};
                if(isNormVec2Null(circle.initialVelocity))
                {
                    physicComponent.isStatic = true;
                }
                BaseEntityManager.Instance.AddComponent(entity, physicComponent);
                BaseEntityManager.Instance.AddComponent(entity, collisionComponent);

                ECSController.Instance.CreateShape(entity, circle.initialSize);
                ECSController.Instance.UpdateShapePosition(entity, circle.initialPosition);
            }
            isInitPhase = false;

        }
    }

    private bool isNormVec2Null(Vector2 input)
    {
        return (input.x * input.x) + (input.y * input.y) == 0.0;
    }
}
