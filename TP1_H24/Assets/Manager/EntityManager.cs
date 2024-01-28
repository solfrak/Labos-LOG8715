using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;

public struct Entity
{
    public uint id;
}

public struct ComponentWrapper
{
    public IComponent component;
    public uint entityId;
}

public interface IEntityManager
{
    uint CreateEntity();
    void DestroyEntity(uint entity);
    void AddComponent<T>(uint entity, T component) where T : IComponent;
    IComponent GetComponent<T>(uint entity);

    void UpdateComponent<T>(uint entity, T component) where T : IComponent;

    List<uint> GetEntities();
}


public class BaseEntityManager : IEntityManager
{

    private static BaseEntityManager _instance;
    public static BaseEntityManager Instance 
    {
        get
        {
            if(_instance == null)
            {
                _instance = new BaseEntityManager();
            }

            return _instance;
        }
    }
    private List<uint> entities = new List<uint>();
    private uint counter = 0;
    private Dictionary<uint, Dictionary<Type, int>> componentIndexer = new Dictionary<uint, Dictionary<Type, int>>();
    private Dictionary<Type, List<ComponentWrapper>> components = new Dictionary<Type, List<ComponentWrapper>>();
    public void AddComponent<T>(uint entity, T component) where T : IComponent
    {
        Type componentType = typeof(T);
        if(!components.ContainsKey(componentType))
        {
            components[componentType] = new List<ComponentWrapper>();
        }

        components[componentType].Add(new ComponentWrapper{component = component, entityId = entity});
        componentIndexer[entity][componentType] = components[componentType].Count - 1;
    }

    public uint CreateEntity()
    {
        entities.Add(counter);
        componentIndexer[counter] = new Dictionary<Type, int>();
        return counter++;
    }

    public void DestroyEntity(uint entity)
    {

        Dictionary<Type, int> entityComponent = componentIndexer[entity];
        

        foreach(var pair in entityComponent.ToList())
        {
            Type componentType = pair.Key;
            int componentIndex = pair.Value;

            //Place the last component from the list to the component to be removed
            int lastComponentIndex = components[componentType].Count - 1;
            components[componentType][componentIndex] = components[componentType][lastComponentIndex];

            //Update the indexer of the moved component
            uint movedComponentEntityId = components[componentType][componentIndex].entityId;
            componentIndexer[movedComponentEntityId][componentType] = componentIndex;

            //Remove the component from the list
            components[componentType].RemoveAt(lastComponentIndex);
            componentIndexer[entity].Remove(componentType);
        }

        componentIndexer.Remove(entity);
        entities.Remove(entity);

    }

    public IComponent GetComponent<T>(uint entity)
    {
        Type componentType = typeof(T);
        int index = componentIndexer[entity][componentType];
        return components[componentType][index].component;
    }

    public void UpdateComponent<T>(uint entity, T component) where T : IComponent
    {
        Type componentTYpe = typeof(T);
        int index = componentIndexer[entity][componentTYpe];

        ComponentWrapper updatedComponent = components[componentTYpe][index];
        updatedComponent.component = component;

        components[componentTYpe][index] = updatedComponent;
    }

    public List<uint> GetEntities()
    {
        return entities;
    }
}


