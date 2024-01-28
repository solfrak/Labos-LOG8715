
using System;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class ExplosionSystem : ISystem
{
    public string Name {get; set;}

    private int ExplosionSize;

    public ExplosionSystem()
    {
        Name = "ExplosionSystem";
        ExplosionSize = ECSController.Instance.Config.explosionSize;
    }
    public void UpdateSystem()
    {
        var entities = BaseEntityManager.Instance.GetEntities();

        foreach(var entity in entities)
        {
            PhysicComponent physicComponent = (PhysicComponent)BaseEntityManager.Instance.GetComponent<PhysicComponent>(entity);
            if(physicComponent.size >= ExplosionSize)
            {
                SpawnNewCircle(physicComponent);
                DestroyEntity(entity);
            }
        }
    }

    private void DestroyEntity(uint entity)
    {
        BaseEntityManager.Instance.DestroyEntity(entity);
    }

    private void SpawnNewCircle(PhysicComponent physicComponent)
    {
        int size = (int)Math.Ceiling(physicComponent.size / 4.0);

        //Calculate position and velocity for each circle to spawn
        for(int i = 0; i < 4; i++)
        {
            //Change those value
            Vector2 position = new Vector2(0.0f, 0.0f);
            Vector2 velocity = new Vector2(0.0f, 0.0f);

            Utils.SpawnCirlce(position, velocity, size);

        }

    }
}
