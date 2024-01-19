using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity
{
    private Dictionary<Type, object> components = new Dictionary<Type, object>();
    public uint id { get; set; }

    public void AddComponent<T>(T component)
    {
        components[typeof(T)] = component;
    }

    public T GetComponent<T>()
    {
        return (T)components[typeof(T)];
    }

    public void UpdateComponent<T>(T component)
    {
        Type type = typeof(T);
        components[type] = component;
    }
}
