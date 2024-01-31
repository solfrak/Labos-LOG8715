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
        var camSize = GetPlayableScreenSize();
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
        DetectCollision(ref collisions);
        UpdateAfterCollision(ref collisions);
    }

    private void DetectCollision(ref List<Tuple<uint, uint, PhysicComponent, PhysicComponent>> collisions)
    {

        // var entities = EntityManager.Instance.GetEntities();
        var entities = BaseEntityManager.Instance.GetEntities();

        for (int i = 0; i < entities.Count; i++)
        {
            BaseEntityManager.Instance.UpdateComponent(entities[i], CalculateScreenCollision((PhysicComponent)BaseEntityManager.Instance.GetComponent<PhysicComponent>(entities[i])));
            for (int j = i; j < entities.Count; j++)
            {
                if (i != j)
                {
                    var firstEntity = entities[i];
                    var secondEntity = entities[j];


                    PhysicComponent firstPhys = (PhysicComponent)BaseEntityManager.Instance.GetComponent<PhysicComponent>(firstEntity);
                    PhysicComponent secondPhys = (PhysicComponent)BaseEntityManager.Instance.GetComponent<PhysicComponent>(secondEntity);

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

            CollisionComponent cEntity1 = (CollisionComponent)BaseEntityManager.Instance.GetComponent<CollisionComponent>(collision.Item1);
            CollisionComponent cEntity2 = (CollisionComponent)BaseEntityManager.Instance.GetComponent<CollisionComponent>(collision.Item2);

            BaseEntityManager.Instance.UpdateComponent(collision.Item1, pEntity1);
            BaseEntityManager.Instance.UpdateComponent(collision.Item2, pEntity2);

            if(!pEntity1.isStatic && !pEntity2.isStatic)
            {
                UpdateCollisionCount(pEntity1, pEntity2, cEntity1, cEntity2, collision.Item1, collision.Item2);
            }
        }
    }

    private PhysicComponent CalculateScreenCollision(PhysicComponent input)
    {
        if (input.position.x + (input.size / 2) >= sWidth / 2 || input.position.x - (input.size / 2) <= (-sWidth / 2))
        {
            input.velocity.x *= -1.0f;
        }
        if (input.position.y + (input.size / 2) >= sHeight / 2 || input.position.y - (input.size / 2) <= (-sHeight / 2))
        {
            input.velocity.y *= -1.0f;
        }

        return input;

    }

    private void UpdateCollisionCount(PhysicComponent physicComponent1, PhysicComponent physicComponent2, CollisionComponent collisionComponent1, CollisionComponent collisionComponent2, uint entity1, uint entity2)
    {
        if(physicComponent1.size == physicComponent2.size)
        {
            collisionComponent1.CollisionCount++;
            collisionComponent2.CollisionCount++;
            return;
        }

        if(physicComponent1.size > physicComponent2.size)
        {
            collisionComponent1.augmentSizeCollision++;
            collisionComponent2.diminishSizeCollision++;
            collisionComponent1.CollisionCount++;
            collisionComponent2.CollisionCount++;

        }
        else
        {
            collisionComponent1.diminishSizeCollision++;
            collisionComponent2.augmentSizeCollision++;
            collisionComponent1.CollisionCount++;
            collisionComponent2.CollisionCount++;
        }

        BaseEntityManager.Instance.UpdateComponent(entity1, collisionComponent1);
        BaseEntityManager.Instance.UpdateComponent(entity2, collisionComponent2);
    }
}

