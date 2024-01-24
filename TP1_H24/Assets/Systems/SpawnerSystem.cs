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
                var entity = EntityManager.Instance.CreateEntity();
                EntityManager.Instance.AddComponent(entity, new PhysicComponent{ entityId = entity.id, position = circle.initialPosition, size = circle.initialSize, velocity = circle.initialVelocity});
                // Old_Entity circleEntity = new Old_Entity();
                // PhysicComponent phys = new PhysicComponent();
                // phys.size = circle.initialSize;
                // phys.position = circle.initialPosition;
                // phys.velocity = circle.initialVelocity;

                // circleEntity.AddComponent(phys);
                // circleEntity.id = Old_EntityManager.Instance.GetId();
                // Old_EntityManager.Instance.AddEntity(circleEntity);
                ECSController.Instance.CreateShape(entity.id, circle.initialSize);
                ECSController.Instance.UpdateShapePosition(entity.id, circle.initialPosition);
            }
            isInitPhase = false;

        }
    }
}
