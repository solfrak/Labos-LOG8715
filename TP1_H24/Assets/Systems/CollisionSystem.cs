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
        ResetCollisions();
        DetectCollision(ref collisions);
        UpdateAfterCollision(ref collisions);
    }

    private void ResetCollisions()
    {
        var entities = BaseEntityManager.Instance.GetEntities();

        for (int i = 0; i < entities.Count; i++)
        {
            CollisionComponent collisionComponent = (CollisionComponent)BaseEntityManager.Instance.GetComponent<CollisionComponent>(entities[i]);
            collisionComponent.CollisionCount = 0;
        }
    }

    private void DetectCollision(ref List<Tuple<uint, uint, PhysicComponent, PhysicComponent>> collisions)
    {
        var entities = BaseEntityManager.Instance.GetEntities();

        for (int i = 0; i < entities.Count; i++)
        {
            PhysicComponent physicComponent = (PhysicComponent)BaseEntityManager.Instance.GetComponent<PhysicComponent>(entities[i]);
            CollisionComponent collisionComponent = (CollisionComponent)BaseEntityManager.Instance.GetComponent<CollisionComponent>(entities[i]);
            BaseEntityManager.Instance.UpdateComponent(entities[i], CalculateScreenCollision(physicComponent, collisionComponent));
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

    private PhysicComponent CalculateScreenCollision(PhysicComponent input, CollisionComponent collisionComponent)
    {
        if(input.position.x + input.size / 2 >= sWidth / 2)
        {
            input.velocity.x = MathF.Abs(input.velocity.x) * -1.0f;
            collisionComponent.CollisionCount++;
        }
        else if(input.position.x - input.size / 2 <= -sWidth / 2)
        {
            input.velocity.x = MathF.Abs(input.velocity.x);
            collisionComponent.CollisionCount++;
        }
        if (input.position.y + (input.size / 2) >= sHeight / 2)
        {
            input.velocity.y = MathF.Abs(input.velocity.y) * -1.0f;
            collisionComponent.CollisionCount++;
        }
        else if (input.position.y - (input.size / 2) <= -sHeight / 2)
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

        //if (physicComponent1.size == physicComponent2.size)
        //{
        //    return;
        //}
        //// bigger than two, 1 get bigger, 2 smaller
        //if(physicComponent1.size > physicComponent2.size)
        //{
        //    collisionComponent1.augmentSizeCollision++;
        //    collisionComponent2.diminishSizeCollision++;
    

        //}
        //else
        //{
        //    collisionComponent1.diminishSizeCollision++;
        //    collisionComponent2.augmentSizeCollision++;
           
        //}
        //
        if (physicComponent1.size == physicComponent2.size)
        {
            // Les cercles ont la même taille, pas de changement
            return;
        }
        else if (physicComponent1.size > physicComponent2.size)

            {
                // Le cercle actuel est plus grand que l'autre cercle
                if (!IsProtected(entity1))
            {
                // Augmenter la taille du cercle actuel
                collisionComponent1.augmentSizeCollision++;
            }
            // Réduire la taille de l'autre cercle
            collisionComponent2.diminishSizeCollision++;
        }
        else
        {
            // Le cercle actuel est plus petit que l'autre cercle
            if (!IsProtected(entity1))
            {
                // Réduire la taille du cercle actuel
                collisionComponent1.diminishSizeCollision++;
            }
            // Réduire la taille de l'autre cercle
            collisionComponent2.diminishSizeCollision++;
        }
    }

    bool IsProtected(uint entity)
    {
        ProtectionStat protectStat = (ProtectionStat)BaseEntityManager.Instance.GetComponent<ProtectionStat>(entity);

        return (protectStat.ProtectionState == ProtectionStat.State.ACTIVE);
    }
}

