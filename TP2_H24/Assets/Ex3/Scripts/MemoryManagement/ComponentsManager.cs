//#define BAD_PERF // TODO CHANGEZ MOI. Mettre en commentaire pour utiliser votre propre structure

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

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
    // private List<ArchetypeCustom> _archetypes = new List<ArchetypeCustom>();
    // private HashSet<ArchetypeCustom> _archetypes = new HashSet<ArchetypeCustom>();
    private Dictionary<uint, ArchetypeCustom> _archetypes = new Dictionary<uint, ArchetypeCustom>();
    private Dictionary<EntityComponent, uint> _archetypeIndexer = new Dictionary<EntityComponent, uint>();


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
        Profiler.BeginSample("SetComponent");
        uint type = TypeRegistry<T>.typeID;
        if (!_allComponents.ContainsKey(type) || _allComponents[type].Count == 0)
        {
            _allComponents[type] = new List<IComponent>(Enumerable.Repeat<IComponent>(null, maxEntities));
        }
        if(_allComponents[type][(int)entityID.id] == null)
        {
            UpdateArchetype(entityID, type);
        }
        _allComponents[type][(int)entityID.id] = component;
        Profiler.EndSample();
    }

    public void UpdateArchetype(EntityComponent entity, uint sign, bool isRemove = false)
    {
        Profiler.BeginSample("UpdateArchetype");
        //TODO If the archetype exist, remove entity from old archetype and add entity to new archetype
        //TODO If the archetyep does not exist? What can we do

        bool removeArchetype = false;

        uint old_signature = 0;
        uint new_signature = 0;
        /*
         * TODO instead of doing a for loop to find where the archetype of the entity is, use a map to know directly the archetype
         * Use a set instead of a list to hold the Archetypes. Use the Signature as the hash. Doing it this way, we could skip all
         * the for loop and just add and remove the entity to the appropriate archetype.
         */

        if (_archetypeIndexer.ContainsKey(entity))
        {
            //We know what archetype the entity currently belong
            //Remove the entity from the currenty archetype
            old_signature = _archetypeIndexer[entity];
            _archetypes[old_signature].RemoveEntity(entity);
            if(_archetypes[old_signature].Count() == 0)
            {
                removeArchetype = true;
            }


            //Add the entity to the new archetype
            new_signature = isRemove ? old_signature - sign : old_signature + sign;
            if(_archetypes.ContainsKey(new_signature))
            {
                //We already have the new Archetype
                _archetypeIndexer[entity] = new_signature;
                _archetypes[new_signature].AddEntity(entity);
            }
            else
            {
                //Create the new archetype
                goto createArchetype;
            }
        }
        else 
        {
            //We don't know what archetype the entity currently belong
            //Must be a new entity then
            new_signature = sign;
            if(_archetypes.ContainsKey(new_signature))
            {
                _archetypeIndexer[entity] = new_signature;
                _archetypes[new_signature].AddEntity(entity);
            }
            else
            {
                //Create the new archetype
                goto createArchetype;
            }
        }

        goto cleanup;

    createArchetype:
        ArchetypeCustom archetypeCustom = new ArchetypeCustom(new_signature);
        var list = new HashSet<uint>();
        if (_archetypes.ContainsKey(old_signature))
        {
            ArchetypeCustom old_archetype = _archetypes[old_signature];
            list = old_archetype.GetComponentType();
        }

        if (isRemove)
        {
            list.Remove(sign);
        }
        else
        {
            list.Add(sign);
        }

        archetypeCustom.SetComponentType(list);
        _archetypeIndexer[entity] = new_signature;
        archetypeCustom.AddEntity(entity);
        _archetypes.Add(new_signature, archetypeCustom);

    cleanup:
        if (removeArchetype)
        {
            _archetypes.Remove(old_signature);
        }


        //No archetype exist yet
        // if (newSignature == 0)
        // {
        //     newSignature = sign;
        // }
        // ArchetypeCustom archetypeCustom = new ArchetypeCustom(newSignature);
        // archetypeCustom.AddEntity(entity);
        // if(isRemove)
        // {
        //     components.Remove(sign);
        // }

        // else {
        //     components.Add(sign);
        // }

        // archetypeCustom.SetComponentType(components);
        // _archetypes.Add(archetypeCustom);

        // cleanup:
        //     if(removeArchetype)
        //     {
        //         //TODO remove unused archetype
        //     }


        Profiler.EndSample();
    }
    public void RemoveComponent<T>(EntityComponent entityID) where T : IComponent
    {
        uint type = TypeRegistry<T>.typeID;
        _allComponents[type][(int)entityID.id] = null;

        UpdateArchetype(entityID, type, true);
    }
    public T GetComponent<T>(EntityComponent entityID) where T : IComponent
    {
        uint type = TypeRegistry<T>.typeID;
        return (T)_allComponents[type][(int)entityID.id];
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
        // if (!_allComponents.ContainsKey(TypeRegistry<T>.typeID))
        // {
        //     _allComponents.Add(TypeRegistry<T>.typeID, new List<IComponent>(Enumerable.Repeat<IComponent>(null, maxEntities)));
        // }
        // else
        // {
        //     uint type = TypeRegistry<T>.typeID;
        //     for(int i = 0; i < maxEntities; i++)
        //     {
        //         _allComponents[type][i] = null;
        //     }
        // }
        Profiler.BeginSample("ClearComponent");
        var archetypeCustoms = new Dictionary<uint, ArchetypeCustom>(_archetypes);

        foreach(var archetype in archetypeCustoms)
        {
            if(archetype.Value.HasAll<T>())
            {
                foreach(var entity in archetype.Value.GetEntities())
                {
                    RemoveComponent<T>(entity);
                }
            }
        }
        Profiler.EndSample();
    }

    public void ForEach<T1>(Action<EntityComponent, T1> lambda) where T1 : IComponent
    {
        var archetypes = new Dictionary<uint, ArchetypeCustom>(_archetypes); 
        foreach(var archetype in archetypes.Values)
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
        var archetypes = new Dictionary<uint, ArchetypeCustom>(_archetypes); 
        foreach(var archetype in archetypes.Values)
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
        var archetypes = new Dictionary<uint, ArchetypeCustom>(_archetypes); 
        foreach(var archetype in archetypes.Values)
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
        var archetypes = new Dictionary<uint, ArchetypeCustom>(_archetypes); 
        foreach(var archetype in archetypes.Values)
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
