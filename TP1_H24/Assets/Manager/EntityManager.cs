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

public interface IState{}

public interface IEntityManager
{
    uint CreateEntity();
    void DestroyEntity(uint entity);
    void AddComponent<T>(uint entity, T component) where T : IComponent;
    T GetComponent<T>(uint entity) where T : IComponent;

    void UpdateComponent<T>(uint entity, T component) where T : IComponent;
    List<uint> GetEntities();
    
    void UpdateEntityState(IState state);
    IState GetState();

}

public struct BaseEntityState : IState
{
    public List<uint> entities;
    public uint counter;
    public Dictionary<uint, Dictionary<Type, int>> componentIndexer;
    public Dictionary<Type, List<ComponentWrapper>> components;
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
        Dictionary<Type, int> entityComponents = componentIndexer[entity];
        

        foreach(var pair in entityComponents.ToList())
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

    public T GetComponent<T>(uint entity) where T:IComponent
    {
        Type componentType = typeof(T);
        int index = componentIndexer[entity][componentType];
        return (T) components[componentType][index].component;
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

    public void UpdateEntityState(IState state)
    {
        BaseEntityState bState = (BaseEntityState)state;
        entities = bState.entities;
        counter = bState.counter;
        componentIndexer = bState.componentIndexer;
        components = bState.components;
    }

    public IState GetState()
    {
        List<uint> copyEntities = new List<uint>();
        foreach(var entity in entities)
        {
            copyEntities.Add(entity);
        }

        Dictionary<uint, Dictionary<Type, int>> copyIndexer = new Dictionary<uint, Dictionary<Type, int>>();
        foreach(var entry in componentIndexer)
        {
            var entity = entry.Key;
            copyIndexer[entity] = new Dictionary<Type, int>();
            foreach(var types in componentIndexer[entity])
            {
                copyIndexer[entity][types.Key] = types.Value;
            }
        }

        Dictionary<Type, List<ComponentWrapper>> copyComponents = new Dictionary<Type, List<ComponentWrapper>>();
        foreach(var entry in components)
        {
            var type = entry.Key;
            copyComponents[type] = new List<ComponentWrapper>();
            foreach(var element in entry.Value)
            {
                copyComponents[type].Add(new ComponentWrapper{ component = element.component, entityId = element.entityId});
            }
        }

        BaseEntityState state = new BaseEntityState {
            entities = copyEntities,
            counter = counter,
            componentIndexer = copyIndexer,
            components = copyComponents
        };

        return state;

    }
}


