using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ProtectionComponent : IComponent
{
    public enum State { READY, ACTIVE, COOLDOWN, UNPROTECTABLE }
    public float ElapsedTimeProtected;
    public float ElapsedCoolDown;
    public int ProtectionTriggerCollisionCount;
    public State ProtectionState;

    public bool CanGainProtectionTriggerCollisions()
    {
        return ProtectionState == State.READY;
    }
}
