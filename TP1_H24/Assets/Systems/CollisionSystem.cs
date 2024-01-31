using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Il2Cpp;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class ColisionSystem : ISystem
{
    public string Name { get; set; }

    private float sWidth;
    private float sHeight;
    public ColisionSystem()
    {
        Name = "ColisionSystem";
        Vector2 camSize = GetPlayableScreenSize();
        sWidth = camSize.x;
        sHeight = camSize.y;
    }

    private Vector2 GetPlayableScreenSize()
    {
        float cameraHeight = 2f * Camera.main.orthographicSize;
        float cameraWidth = cameraHeight * Camera.main.aspect;

        return new Vector2(cameraWidth, cameraHeight);
    }
    public void UpdateSystem()
    {
        List<Tuple<uint, uint, PhysicComponent, PhysicComponent>> collisions = new List<Tuple<uint, uint, PhysicComponent, PhysicComponent>>();
        ResetCollisions();
        DetectCollision(ref collisions);
        UpdateAfterCollision(ref collisions);
    }

    private void ResetCollisions()
    {
        var entities = BaseEntityManager.Instance.GetEntities();

        for (int i = 0; i < entities.Count; i++)
        {
            CollisionComponent collisionComponent = BaseEntityManager.Instance.GetComponent<CollisionComponent>(entities[i]);
            collisionComponent.CollisionCount = 0;
        }
    }

    private void DetectCollision(ref List<Tuple<uint, uint, PhysicComponent, PhysicComponent>> collisions)
    {
        var entities = BaseEntityManager.Instance.GetEntities();

        for (int i = 0; i < entities.Count; i++)
        {
            PhysicComponent physicComponent = BaseEntityManager.Instance.GetComponent<PhysicComponent>(entities[i]);
            CollisionComponent collisionComponent = BaseEntityManager.Instance.GetComponent<CollisionComponent>(entities[i]);
            BaseEntityManager.Instance.UpdateComponent(entities[i], CalculateScreenCollision(physicComponent, ref collisionComponent));
            for (int j = i; j < entities.Count; j++)
            {
                if (i != j)
                {
                    var firstEntity = entities[i];
                    var secondEntity = entities[j];

                    PhysicComponent firstPhys = BaseEntityManager.Instance.GetComponent<PhysicComponent>(firstEntity);
                    PhysicComponent secondPhys = BaseEntityManager.Instance.GetComponent<PhysicComponent>(secondEntity);

                    CollisionResult result = CollisionUtility.CalculateCollision(firstPhys.position, firstPhys.velocity, firstPhys.size, secondPhys.position, secondPhys.velocity, secondPhys.size);
                    if (result != null)
                    {
                        firstPhys.velocity = result.velocity1;
                        firstPhys.position = result.position1;
                        secondPhys.velocity = result.velocity2;
                        secondPhys.position = result.position2;
                        Tuple<uint, uint, PhysicComponent, PhysicComponent> tuple = new(firstEntity, secondEntity, firstPhys, secondPhys);
                        collisions.Add(tuple);
                    }
                }
            }
        }
    }

    private void UpdateAfterCollision(ref List<Tuple<uint, uint, PhysicComponent, PhysicComponent>> collisions)
    {
        foreach (var collision in collisions)
        {
            PhysicComponent pEntity1 = collision.Item3;
            PhysicComponent pEntity2 = collision.Item4;

            CollisionComponent cEntity1 = BaseEntityManager.Instance.GetComponent<CollisionComponent>(collision.Item1);
            CollisionComponent cEntity2 = BaseEntityManager.Instance.GetComponent<CollisionComponent>(collision.Item2);

            BaseEntityManager.Instance.UpdateComponent(collision.Item1, pEntity1);
            BaseEntityManager.Instance.UpdateComponent(collision.Item2, pEntity2);

            if(!pEntity1.isStatic && !pEntity2.isStatic)
            {
                UpdateCollisionCount(pEntity1, pEntity2, cEntity1, cEntity2, collision.Item1, collision.Item2);
            }
        }
    }

    private PhysicComponent CalculateScreenCollision(PhysicComponent input, ref CollisionComponent collisionComponent)
    {
        if(input.position.x + input.size / 2.0f >= sWidth / 2.0f)
        {
            input.velocity.x = MathF.Abs(input.velocity.x) * -1.0f;
            collisionComponent.CollisionCount++;
        }
        else if(input.position.x - input.size / 2.0f <= -sWidth / 2.0f)
        {
            input.velocity.x = MathF.Abs(input.velocity.x);
            collisionComponent.CollisionCount++;
        }
        if (input.position.y + (input.size / 2.0f) >= sHeight / 2.0f)
        {
            input.velocity.y = MathF.Abs(input.velocity.y) * -1.0f;
            collisionComponent.CollisionCount++;
        }
        else if (input.position.y - (input.size / 2.0f) <= -sHeight / 2.0f)
        {
            input.velocity.y = MathF.Abs(input.velocity.y);
            collisionComponent.CollisionCount++;
        }
        return input;
    }

    private void UpdateCollisionCount(PhysicComponent physicComponent1, PhysicComponent physicComponent2, CollisionComponent collisionComponent1, CollisionComponent collisionComponent2, uint entity1, uint entity2)
    {
        collisionComponent1.CollisionCount++;
        collisionComponent2.CollisionCount++;

        if (physicComponent1.size > physicComponent2.size)
        {
            collisionComponent1.augmentSizeCollision++;
            collisionComponent2.diminishSizeCollision++;
        }
        else
        {
            collisionComponent1.diminishSizeCollision++;
            collisionComponent2.augmentSizeCollision++;
        }
        BaseEntityManager.Instance.UpdateComponent(entity1, collisionComponent1);
        BaseEntityManager.Instance.UpdateComponent(entity2, collisionComponent2);
    }

    bool IsProtected(uint entity)
    {
        ProtectionComponent protectStat = BaseEntityManager.Instance.GetComponent<ProtectionComponent>(entity);

        return (protectStat.ProtectionState == ProtectionComponent.State.ACTIVE);
    }
}

