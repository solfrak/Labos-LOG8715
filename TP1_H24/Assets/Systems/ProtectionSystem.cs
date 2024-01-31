using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ProtectionSystem : ISystem
{
    public string Name {get; set;}
    private int ProtectionSize;
    private float ProtectionDuration;
    private float ProtectionCooldown;
    private float ProtectionCollisionCount;
    private enum ProtectionState {READY, ACTIVE, COOLDOWN}
    public ProtectionSystem() 
    {
        Name = "ProtectionSystem";
        ProtectionSize = ECSController.Instance.Config.protectionSize;
        ProtectionDuration = ECSController.Instance.Config.protectionDuration;
        ProtectionCooldown = ECSController.Instance.Config.protectionCooldown;
        ProtectionCollisionCount = ECSController.Instance.Config.protectionCollisionCount;
    }

    public void UpdateSystem()
    {
        List<uint> entities = BaseEntityManager.Instance.GetEntities();

        foreach(var entity in entities)
        {
            ProtectionComponent protectionStat = (ProtectionComponent)BaseEntityManager.Instance.GetComponent<ProtectionComponent>(entity);
            PhysicComponent physicComponent = (PhysicComponent)BaseEntityManager.Instance.GetComponent<PhysicComponent>(entity);


            switch (protectionStat.ProtectionState)
            {
                case ProtectionComponent.State.ACTIVE:
                    HandleActive(ref protectionStat);
                    break;
                case ProtectionComponent.State.COOLDOWN:
                    HandleCooldown(ref protectionStat);
                    break;
                case ProtectionComponent.State.READY:
                    HandleReady(ref physicComponent, ref protectionStat);
                    break;
                default:
                    break;
            }

            BaseEntityManager.Instance.UpdateComponent(entity, protectionStat);
        }
    }
    
    private void HandleActive(ref ProtectionComponent protectionStat)
    {
        if(protectionStat.ElapsedTimeProtected >= ProtectionDuration || protectionStat.ProtectedCollisionCount >= ProtectionCollisionCount)
        {
            protectionStat.ElapsedTimeProtected = 0.0f;
            protectionStat.ProtectionState = ProtectionComponent.State.COOLDOWN;
            return;
        }

        protectionStat.ElapsedTimeProtected += Time.deltaTime;
    }

    private void HandleCooldown(ref ProtectionComponent protectionStat)
    {
        if(protectionStat.ElapsedCoolDown >= ProtectionCooldown)
        {
            protectionStat.ElapsedCoolDown = 0.0f;
            protectionStat.ProtectionState = ProtectionComponent.State.READY;
            return;
        }
        protectionStat.ElapsedCoolDown += Time.deltaTime;
    }

    private void HandleReady(ref PhysicComponent physicComponent, ref ProtectionComponent protectionStat)
    {
        if(physicComponent.size <= ProtectionSize)
        {
            protectionStat.ProtectionState = ProtectionComponent.State.ACTIVE;
        }
    }
}