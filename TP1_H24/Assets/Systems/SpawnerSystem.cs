using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerSystem : ISystem
{
    public string Name { get; set;}
    public SpawnerSystem(IEntityManager entityManager)
    {
        Name = "SpawnerSystem";
        EntityManager = entityManager;

    }

    private IEntityManager EntityManager;
    private bool isInitPhase = true;

    public void UpdateSystem()
    {
        if(isInitPhase)
        {
            foreach (var circle in ECSController.Instance.Config.circleInstancesToSpawn)
            {
                Utils.SpawnCircle(circle.initialPosition, circle.initialVelocity, circle.initialSize);
            }
            isInitPhase = false;

        }
    }

    
}
