
using System;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class ExplosionSystem : ISystem
{
    public string Name {get; set;}

    private int explosionSize;
    private const int PLAYER_EXPLOSION_THRESHOLD = 4;

    public ExplosionSystem()
    {
        Name = "ExplosionSystem";
        explosionSize = ECSController.Instance.Config.explosionSize;
    }
    public void UpdateSystem()
    {
        CircleSizeExplosion();
        PlayerExplosionCheck();
    }

    private void CircleSizeExplosion()
    {
        var entities = BaseEntityManager.Instance.GetEntities();

        for (int i = 0; i < entities.Count; i++)
        {
            PhysicComponent physicComponent = BaseEntityManager.Instance.GetComponent<PhysicComponent>(entities[i]);

            if (physicComponent.size >= explosionSize)
            {
                SpawnExplosionCircles(physicComponent);
                DestroyComponent destroyComponent = BaseEntityManager.Instance.GetComponent<DestroyComponent>(entities[i]);
                destroyComponent.toDestroy = true;
                BaseEntityManager.Instance.UpdateComponent(entities[i], destroyComponent);
            }
        }
    }

    private void PlayerExplosionCheck()
    {
        var entities = BaseEntityManager.Instance.GetEntities();
        if (!Input.GetMouseButtonDown(0))
            return;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos = new Vector2(worldPos.x, worldPos.y);

        for (int i = 0; i < entities.Count; i++)
        {
            PhysicComponent physicComponent = BaseEntityManager.Instance.GetComponent<PhysicComponent>(entities[i]);

            if (physicComponent.isStatic)
                continue;

            if (PositionInCircle(physicComponent, mousePos))
            {
                if (physicComponent.size >= PLAYER_EXPLOSION_THRESHOLD)
                {
                    SpawnExplosionCircles(physicComponent);
                }
                DestroyComponent destroyComponent = BaseEntityManager.Instance.GetComponent<DestroyComponent>(entities[i]);
                destroyComponent.toDestroy = true;
                BaseEntityManager.Instance.UpdateComponent(entities[i], destroyComponent);
            }
        }
    }

    bool PositionInCircle(PhysicComponent physicComponent, Vector2 position)
    {
        Vector2 distanceToCircleCenter = position - physicComponent.position;
        return distanceToCircleCenter.magnitude < physicComponent.size / 2.0f;
    }

    Vector2 GetSpawnDirection(int index)
    {
        double angle =  index* (float)(Math.PI / 2);
        int x = (int)Math.Round(Math.Cos(angle + Math.PI/4));
        int y = (int)Math.Round(Math.Sin(angle+ Math.PI/4));
        return new Vector2(x, y);
    }

    private void SpawnExplosionCircles(PhysicComponent physicComponent)
    {
        int size = (int)Math.Ceiling(physicComponent.size / 4.0);

        //Calculate position and velocity for each circle to spawn
        for (int i = 0; i < 4; i++)
        {
            //Change those value
            Vector2 velocity = GetSpawnDirection(i) * physicComponent.velocity.sqrMagnitude;
            Vector2 position = physicComponent.position + GetSpawnDirection(i) * size;

            Utils.SpawnCircle(position, velocity, size, true);
        }
    }
}
