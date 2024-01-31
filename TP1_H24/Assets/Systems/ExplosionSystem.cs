
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

        for(int i = 0; i < entities.Count; i++)
        {
            PhysicComponent physicComponent = BaseEntityManager.Instance.GetComponent<PhysicComponent>(entities[i]);

            if(physicComponent.size >= ExplosionSize || ClickedOnCircle(physicComponent))
            {
                SpawnExplosionCircles(physicComponent);
                DestroyEntity(entities[i]);
            }
        }
    }

    bool ClickedOnCircle(PhysicComponent physicComponent)
    {
        bool clicked= Input.GetMouseButtonDown(0);
        if (!clicked)
        {
            return false;
        }

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos = new Vector2(worldPos.x, worldPos.y);

        Vector2 distanceToCircleCenter = mousePos - physicComponent.position;

        return distanceToCircleCenter.magnitude < physicComponent.size / 2.0f;
    }

    Vector2 getVector(int index)
    {
        double angle =  index* (float)(Math.PI / 2);
        int x = (int)Math.Round(Math.Cos(angle + Math.PI/4));
        int y = (int)Math.Round(Math.Sin(angle+ Math.PI/4));
        return new Vector2(x, y);
    }
    private void DestroyEntity(uint entity)
    {
        BaseEntityManager.Instance.DestroyEntity(entity);
        ECSController.Instance.DestroyShape(entity);
    }

    private void SpawnExplosionCircles(PhysicComponent physicComponent)
    {
        int size = (int)Math.Ceiling(physicComponent.size / 4.0);

        double norm = Math.Sqrt(physicComponent.velocity.x* physicComponent.velocity.x + physicComponent.velocity.y * physicComponent.velocity.y);

        //Calculate position and velocity for each circle to spawn
        for (int i = 0; i < 4; i++)
        {
            //Change those value
            Vector2 velocity = getVector(i) * (float)norm;
            Vector2 position = physicComponent.position + getVector(i) * size;

            Utils.SpawnCircle(position, velocity, size, true);
        }
    }
}
