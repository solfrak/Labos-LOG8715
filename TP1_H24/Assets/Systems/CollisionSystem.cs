using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Il2Cpp;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class ColisionSystem : ISystem
{
    public string Name { get; set; }

    private float sWidth;
    private float sHeight;
    public ColisionSystem(IEntityManager entityManager)
    {
        Name = "ColisionSystem";
        EntityManager = entityManager;
        Vector2 camSize = GetPlayableScreenSize();
        sWidth = camSize.x;
        sHeight = camSize.y;
    }
    private IEntityManager EntityManager;

    private Vector2 GetPlayableScreenSize()
    {
        float cameraHeight = 2f * Camera.main.orthographicSize;
        float cameraWidth = cameraHeight * Camera.main.aspect;

        return new Vector2(cameraWidth, cameraHeight);
    }
    public void UpdateSystem()
    {
        List<Tuple<uint, uint, PhysicComponent, PhysicComponent>> collisions = new List<Tuple<uint, uint, PhysicComponent, PhysicComponent>>();
        var entities = EntityManager.GetEntities();
        ResetCollisions(entities);
        DetectCollision(ref collisions, entities);
        UpdateAfterCollision(ref collisions);
    }

    public void UpdateLeftSide()
    {
        List<Tuple<uint, uint, PhysicComponent, PhysicComponent>> collisions = new List<Tuple<uint, uint, PhysicComponent, PhysicComponent>>();
        var leftEntities = Utils.GetLeftSideEntities(EntityManager);
        var rightEntities = GetPotentialCollisionEntities(leftEntities);
        foreach(var entity in rightEntities)
        {
            leftEntities.Add(entity);
        }
        ResetCollisions(leftEntities);
        DetectCollision(ref collisions, leftEntities);
        UpdateAfterCollision(ref collisions);
    }
    
    private List<uint> GetPotentialCollisionEntities(List<uint> leftEntities)
    {
        var entities = EntityManager.GetEntities().Where(x => !leftEntities.Contains(x));
        List<uint> potentials = new List<uint>();

        foreach(var entity in entities)
        {
            PhysicComponent physicComponent = EntityManager.GetComponent<PhysicComponent>(entity);
            if(physicComponent.position.x >= 0.0f && physicComponent.position.x - physicComponent.size < 0.0f)
            {
                potentials.Add(entity);
            }
        }

        return potentials;
    }

    private void ResetCollisions(List<uint> entities)
    {
        for (int i = 0; i < entities.Count; i++)
        {
            CollisionComponent collisionComponent = EntityManager.GetComponent<CollisionComponent>(entities[i]);
            collisionComponent.CollisionCount = 0;
            EntityManager.UpdateComponent(entities[i], collisionComponent);
        }
    }

    private void DetectCollision(ref List<Tuple<uint, uint, PhysicComponent, PhysicComponent>> collisions, List<uint> entities)
    {
        for (int i = 0; i < entities.Count; i++)
        {
            PhysicComponent physicComponent = EntityManager.GetComponent<PhysicComponent>(entities[i]);
            CollisionComponent collisionComponent = EntityManager.GetComponent<CollisionComponent>(entities[i]);

            CalculateScreenCollision(ref physicComponent, ref collisionComponent);

            EntityManager.UpdateComponent(entities[i], physicComponent);
            EntityManager.UpdateComponent(entities[i], collisionComponent);

            for (int j = i; j < entities.Count; j++)
            {
                if (i != j)
                {
                    var firstEntity = entities[i];
                    var secondEntity = entities[j];

                    PhysicComponent firstPhys = EntityManager.GetComponent<PhysicComponent>(firstEntity);
                    PhysicComponent secondPhys = EntityManager.GetComponent<PhysicComponent>(secondEntity);

                    CollisionResult result = CollisionUtility.CalculateCollision(firstPhys.position, firstPhys.velocity, 
                        firstPhys.size, secondPhys.position, secondPhys.velocity, secondPhys.size);
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
            PhysicComponent physicComponent1 = collision.Item3;
            PhysicComponent physicComponent2 = collision.Item4;

            CollisionComponent collisionComponent1 = EntityManager.GetComponent<CollisionComponent>(collision.Item1);
            CollisionComponent collisionComponent2 = EntityManager.GetComponent<CollisionComponent>(collision.Item2);
            ProtectionComponent protectionComponent1 = EntityManager.GetComponent<ProtectionComponent>(collision.Item1);
            ProtectionComponent protectionComponent2 = EntityManager.GetComponent<ProtectionComponent>(collision.Item2);

            EntityManager.UpdateComponent(collision.Item1, physicComponent1);
            EntityManager.UpdateComponent(collision.Item2, physicComponent2);

            if(!physicComponent1.isStatic && !physicComponent2.isStatic)
            {
                UpdateCollisionCount(collision.Item1, collision.Item2, physicComponent1, physicComponent2, 
                    collisionComponent1, collisionComponent2, protectionComponent1, protectionComponent2);
            }
        }
    }

    private void CalculateScreenCollision(ref PhysicComponent input, ref CollisionComponent collisionComponent)
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
    }

    private void UpdateCollisionCount(uint entity1, uint entity2, PhysicComponent physicComponent1, PhysicComponent physicComponent2, 
        CollisionComponent collisionComponent1, CollisionComponent collisionComponent2, ProtectionComponent protectionComponent1, ProtectionComponent protectionComponent2)
    {
        collisionComponent1.CollisionCount++;
        collisionComponent2.CollisionCount++;

        if (physicComponent1.size > physicComponent2.size)
        {
            if(protectionComponent2.ProtectionState == ProtectionComponent.State.ACTIVE)
            {
                if(protectionComponent1.ProtectionState != ProtectionComponent.State.ACTIVE)
                {
                    collisionComponent1.changeSizeCollision--;
                }
            }
            else
            {
                collisionComponent1.changeSizeCollision++;
                collisionComponent2.changeSizeCollision--;
            }
        }
        else if (physicComponent1.size < physicComponent2.size)
        {
            if (protectionComponent1.ProtectionState == ProtectionComponent.State.ACTIVE)
            {
                if (protectionComponent2.ProtectionState != ProtectionComponent.State.ACTIVE)
                {
                    collisionComponent2.changeSizeCollision--;
                }
            }
            else
            {
                collisionComponent2.changeSizeCollision++;
                collisionComponent1.changeSizeCollision--;
            }
        }
        else // Same size
        {
            if(protectionComponent1.CanGainProtectionTriggerCollisions())
            {
                protectionComponent1.ProtectionTriggerCollisionCount++;
            }
            if (protectionComponent2.CanGainProtectionTriggerCollisions())
            {
                protectionComponent2.ProtectionTriggerCollisionCount++;
            }
        }

        EntityManager.UpdateComponent(entity1,  collisionComponent1);
        EntityManager.UpdateComponent(entity1,  protectionComponent1);
        EntityManager.UpdateComponent(entity2,  collisionComponent2);
        EntityManager.UpdateComponent(entity2,  protectionComponent2);
    }
}

