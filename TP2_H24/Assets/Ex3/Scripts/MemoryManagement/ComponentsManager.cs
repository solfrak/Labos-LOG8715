//#define BAD_PERF // TODO CHANGEZ MOI. Mettre en commentaire pour utiliser votre propre structure

using System;
using System.Collections.Generic;
using UnityEngine;

#if BAD_PERF
using InnerType = System.Collections.Generic.Dictionary<uint, IComponent>;
using AllComponents = System.Collections.Generic.Dictionary<uint, System.Collections.Generic.Dictionary<uint, IComponent>>;
#else
using AllComponents = System.Collections.Generic.Dictionary<uint, System.Collections.Generic.List<IComponent>>; // TODO CHANGEZ MOI, UTILISEZ VOTRE PROPRE TYPE ICI
#endif

// Appeler GetHashCode sur un Type est couteux. Cette classe sert a precalculer le hashcode
public static class TypeRegistry<T> where T : IComponent
{
    public static uint typeID = (uint)Mathf.Abs(default(T).GetRandomNumber()) % ComponentsManager.maxEntities;
}

public class Singleton<V> where V : new()
{
    private static bool isInitiated = false;
    private static V _instance;
    public static V Instance
    {
        get
        {
            if (!isInitiated)
            {
                isInitiated = true;
                _instance = new V();
            }
            return _instance;
        }
    }
    protected Singleton() { }
}

internal class ComponentsManager : Singleton<ComponentsManager>
{
    private AllComponents _allComponents = new AllComponents();
    private List<ArchetypeCustom> archetypes = new List<ArchetypeCustom>();

    public const int maxEntities = 2000;

    public void DebugPrint()
    {
        string toPrint = "";
        var allComponents = Instance.DebugGetAllComponents();
        foreach (var type in allComponents)
        {
            toPrint += $"{type}: \n";
            foreach (var component in type.Value)
            {
                toPrint += $"\t{component}: {component}\n";
            }
            toPrint += "\n";
        }
        Debug.Log(toPrint);
    }

    // CRUD
    public void SetComponent<T>(EntityComponent entityID, IComponent component) where T : IComponent
    {
        uint type = TypeRegistry<T>.typeID;
        if (!_allComponents.ContainsKey(type))
        {
            _allComponents[type] = new List<IComponent>(maxEntities);
        }
        _allComponents[type][(int)entityID.id] = component;

        UpdateArchetype(entityID, type);
    }

    public void UpdateArchetype(EntityComponent entity, uint sign, bool isRemove = false)
    {
        uint newSignature = 0;
        foreach (var archetype in archetypes)
        {
            if (archetype.HasEntity(entity))
            {
                uint old_sign = archetype.GetSignature();
                archetype.RemoveEntity(entity);
                newSignature = isRemove ? old_sign + sign : old_sign - sign;
            }

            if(archetype.GetSignature() == newSignature)
            {
                archetype.AddEntity(entity);
                return;
            }
        }

        foreach (var archetype in archetypes)
        {
            if (archetype.GetSignature() == newSignature)
            {
                archetype.AddEntity(entity);
                return;
            }
        }
    }
    public void RemoveComponent<T>(EntityComponent entityID) where T : IComponent
    {
        uint type = TypeRegistry<T>.typeID;
        _allComponents[type][(int)entityID.id] = null;

        UpdateArchetype(entityID, type, true);
    }
    public T GetComponent<T>(EntityComponent entityID) where T : IComponent
    {
        return (T)_allComponents[TypeRegistry<T>.typeID][(int)entityID.id];
    }
    public bool TryGetComponent<T>(EntityComponent entityID, out T component) where T : IComponent
    {
        if (_allComponents.ContainsKey(TypeRegistry<T>.typeID))
        {
            if (_allComponents[TypeRegistry<T>.typeID][(int)entityID.id] != null)
            {
                component = (T)_allComponents[TypeRegistry<T>.typeID][(int)entityID.id];
                return true;
            }
        }
        component = default;
        return false;
    }

    public bool EntityContains<T>(EntityComponent entity) where T : IComponent
    {
        return _allComponents[TypeRegistry<T>.typeID][(int)entity.id] != null;
    }

    public void ClearComponents<T>() where T : IComponent
    {
        if (!_allComponents.ContainsKey(TypeRegistry<T>.typeID))
        {
            _allComponents.Add(TypeRegistry<T>.typeID, new List<IComponent>(maxEntities));
        }
        else
        {
            _allComponents[TypeRegistry<T>.typeID].Clear();
        }
    }

    public void ForEach<T1>(Action<EntityComponent, T1> lambda) where T1 : IComponent
    {
        foreach(var archetype in archetypes)
        {
            if(archetype.HasAll<T1>())
            {
                foreach(var entity in archetype.GetEntities())
                {
                    var c1 = GetComponent<T1>(entity);
                    lambda(entity, c1);
                }
            }
        }
    }

    public void ForEach<T1, T2>(Action<EntityComponent, T1, T2> lambda) where T1 : IComponent where T2 : IComponent
    {
        foreach(var archetype in archetypes)
        {
            if(archetype.HasAll<T1, T2>())
            {
                foreach(var entity in archetype.GetEntities())
                {
                    var c1 = GetComponent<T1>(entity);
                    var c2 = GetComponent<T2>(entity);
                    lambda(entity, c1, c2);
                }
            }
        }
    }

    public void ForEach<T1, T2, T3>(Action<EntityComponent, T1, T2, T3> lambda) where T1 : IComponent where T2 : IComponent where T3 : IComponent
    {
        foreach(var archetype in archetypes)
        {
            if(archetype.HasAll<T1, T2, T3>())
            {
                foreach(var entity in archetype.GetEntities())
                {
                    var c1 = GetComponent<T1>(entity);
                    var c2 = GetComponent<T2>(entity);
                    var c3 = GetComponent<T3>(entity);
                    lambda(entity, c1, c2, c3);
                }
            }
        }
    }

    public void ForEach<T1, T2, T3, T4>(Action<EntityComponent, T1, T2, T3, T4> lambda) where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent
    {
        foreach(var archetype in archetypes)
        {
            if(archetype.HasAll<T1, T2, T3, T4>())
            {
                foreach(var entity in archetype.GetEntities())
                {
                    var c1 = GetComponent<T1>(entity);
                    var c2 = GetComponent<T2>(entity);
                    var c3 = GetComponent<T3>(entity);
                    var c4 = GetComponent<T4>(entity);
                    lambda(entity, c1, c2, c3, c4);
                }
            }
        }
    }

    public AllComponents DebugGetAllComponents()
    {
        return _allComponents;
    }
}
