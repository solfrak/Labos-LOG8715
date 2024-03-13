using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArchetypeCustom
{
    private List<EntityComponent> _entities;
    private List<uint> _componentType;
    private uint _signature = 0;
   
    public ArchetypeCustom(uint signature)
    {
        _signature = signature;
    }
    public List<EntityComponent> GetEntities()
    {
        return _entities;
    }

    public uint GetSignature()
    {
        return _signature;
    }
    
    public void AddEntity(EntityComponent entity)
    {
        _entities.Add(entity);
    }


    public void RemoveEntity(EntityComponent entity)
    {
        _entities.Remove(entity);
    }

    public bool HasEntity(EntityComponent entity)
    {
        return _entities.Contains(entity);
    }

    public bool HasAll<T1>() where T1 : IComponent
    {
        uint type = TypeRegistry<T1>.typeID;
        if (_componentType.Contains(type))
        {
            return true;
        }
        else return false;
    }

    public bool HasAll<T1, T2>() where T1 : IComponent where T2 : IComponent
    {
        uint t1 = TypeRegistry<T1>.typeID;
        uint t2 = TypeRegistry<T2>.typeID;
        if (_componentType.Contains(t1) && _componentType.Contains(t2))
        {
            return true;
        }
        else return false;
    }

    public bool HasAll<T1, T2, T3>() where T1 : IComponent where T2 : IComponent where T3 : IComponent
    {
        uint t1 = TypeRegistry<T1>.typeID;
        uint t2 = TypeRegistry<T2>.typeID;
        uint t3 = TypeRegistry<T3>.typeID;
        if (_componentType.Contains(t1) && _componentType.Contains(t2) && _componentType.Contains(t3))
        {
            return true;
        }
        else return false;
    }

    public bool HasAll<T1, T2, T3, T4>() where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent
    {
        uint t1 = TypeRegistry<T1>.typeID;
        uint t2 = TypeRegistry<T2>.typeID;
        uint t3 = TypeRegistry<T3>.typeID;
        uint t4 = TypeRegistry<T4>.typeID;
        if (_componentType.Contains(t1) && _componentType.Contains(t2) && _componentType.Contains(t3) && _componentType.Contains(t4))
        {
            return true;
        }
        else return false;
    }


}
