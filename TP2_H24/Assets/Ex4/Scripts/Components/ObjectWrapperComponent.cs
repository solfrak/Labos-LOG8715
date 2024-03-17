using System;
using Unity.Entities;
using UnityEngine;

public struct ObjectWrapperComponent : IComponentData, IDisposable
{
    GameObject gameObject;

    public void Dispose()
    {
        UnityEngine.Object.Destroy(gameObject);
    }
}
