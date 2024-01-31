using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Dictates when an entity should be considered destroyed
public struct DestroyComponent : IComponent
{
    public bool toDestroy;
}
