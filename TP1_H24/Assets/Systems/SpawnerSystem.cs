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
                Entity circleEntity = new Entity();
                PhysicComponent phys = new PhysicComponent();
                phys.size = circle.initialSize;
                phys.position = circle.initialPosition;
                phys.velocity = circle.initialVelocity;

                circleEntity.AddComponent(phys);
                circleEntity.id = EntityManager.Instance.GetId();
                EntityManager.Instance.AddEntity(circleEntity);
                ECSController.Instance.CreateShape(circleEntity.id, circleEntity.GetComponent<PhysicComponent>().size);
                ECSController.Instance.UpdateShapePosition(circleEntity.id, circleEntity.GetComponent<PhysicComponent>().position);
            }
            isInitPhase = false;

        }
    }
}
