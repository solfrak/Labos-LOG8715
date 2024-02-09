using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ProtectionSystem : ISystem
{
    public string Name {get; set;}
    private int MaxProtectionSize;
    private float ProtectionDuration;
    private float ProtectionCooldown;
    private float ProtectionTriggerCollisionCount;
    private enum ProtectionState {READY, ACTIVE, COOLDOWN}
    public ProtectionSystem(IEntityManager entityManager) 
    {
        Name = "ProtectionSystem";
        EntityManager = entityManager;
        MaxProtectionSize = ECSController.Instance.Config.protectionSize;
        ProtectionDuration = ECSController.Instance.Config.protectionDuration;
        ProtectionCooldown = ECSController.Instance.Config.protectionCooldown;
        // Number of collisions for a circlet to be protected
        ProtectionTriggerCollisionCount = ECSController.Instance.Config.protectionCollisionCount;
    }
    private IEntityManager EntityManager;

    public void UpdateSystem()
    {
        List<uint> entities = EntityManager.GetEntities();
        Update(entities);
    }

    public void UpdateLeftSide()
    {
        var entities = Utils.GetLeftSideEntities(EntityManager);
        Update(entities);
    }

    private void Update(List<uint> entities)
    {
        foreach (var entity in entities)
        {
            ProtectionComponent protectionComponent = EntityManager.GetComponent<ProtectionComponent>(entity);
            PhysicComponent physicComponent = EntityManager.GetComponent<PhysicComponent>(entity);


            if (physicComponent.size > MaxProtectionSize)
            {
                protectionComponent.ProtectionState = ProtectionComponent.State.UNPROTECTABLE;
                protectionComponent.ElapsedCoolDown = 0.0f;
                protectionComponent.ElapsedTimeProtected = 0.0f;
            }

            switch (protectionComponent.ProtectionState)
            {
                case ProtectionComponent.State.ACTIVE:
                    HandleActive(ref protectionComponent);
                    break;
                case ProtectionComponent.State.COOLDOWN:
                    HandleCooldown(ref protectionComponent);
                    break;
                case ProtectionComponent.State.READY:
                    HandleReady(ref physicComponent, ref protectionComponent);
                    break;
                case ProtectionComponent.State.UNPROTECTABLE:
                    HandleUnprotectable(ref physicComponent, ref protectionComponent);
                    break;
                default:
                    break;
            }

            EntityManager.UpdateComponent(entity, protectionComponent);
        }
    }

    private void HandleActive(ref ProtectionComponent protectionComponent)
    {
        protectionComponent.ElapsedTimeProtected += Time.deltaTime;

        if(protectionComponent.ElapsedTimeProtected >= ProtectionDuration)
        {
            protectionComponent.ElapsedTimeProtected = 0.0f;
            protectionComponent.ProtectionState = ProtectionComponent.State.COOLDOWN;
        }
    }

    private void HandleCooldown(ref ProtectionComponent protectionComponent)
    {
        protectionComponent.ElapsedCoolDown += Time.deltaTime;

        if(protectionComponent.ElapsedCoolDown >= ProtectionCooldown)
        {
            protectionComponent.ElapsedCoolDown = 0.0f;
            protectionComponent.ProtectionState = ProtectionComponent.State.READY;
        }
    }

    private void HandleReady(ref PhysicComponent physicComponent, ref ProtectionComponent protectionComponent)
    {
        if(protectionComponent.ProtectionTriggerCollisionCount >= ProtectionTriggerCollisionCount)
        {
            protectionComponent.ProtectionState = ProtectionComponent.State.ACTIVE;
            protectionComponent.ProtectionTriggerCollisionCount = 0;
        }
    }
    private void HandleUnprotectable(ref PhysicComponent physicComponent, ref ProtectionComponent protectionComponent)
    {
        if (physicComponent.size <= MaxProtectionSize)
        {
            protectionComponent.ProtectionState = ProtectionComponent.State.READY;
        }
    }
}