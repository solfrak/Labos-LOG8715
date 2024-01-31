using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ProtectionComponent : IComponent
{
    public enum State { READY, ACTIVE, COOLDOWN, UNPROTECTABLE }
    public float ElapsedTimeProtected;
    public float ElapsedCoolDown;
    public int ProtectedCollisionCount;
    public State ProtectionState ;
}
