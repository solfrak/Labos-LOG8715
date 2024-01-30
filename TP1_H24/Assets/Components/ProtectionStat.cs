using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ProtectionStat : IComponent
{
    public enum State { READY, ACTIVE, COOLDOWN }
    public float ElapsedTimeProtected;
    public float ElapsedCoolDown;
    public int ProtectedCollisionCount;
    public State ProtectionState ;
}
