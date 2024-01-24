using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct PhysicComponent : IComponent
{
    public uint entityId;
    public int size;
    public Vector2 position;
    public Vector2 velocity;
}