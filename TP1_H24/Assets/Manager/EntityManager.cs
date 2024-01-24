using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;

public struct Entity
{
    public uint id;
}
public class EntityManager
{
    private EntityManager(){}
    private static EntityManager _instance;
    
    public static EntityManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new EntityManager();
            }
            return _instance;
        }
    }
    private List<Entity> entities = new List<Entity>();
    private Dictionary<Entity, Dictionary<Type, int>> componentIndexer = new Dictionary<Entity, Dictionary<Type, int>>();
    private Dictionary<Type, List<IComponent>> components = new Dictionary<Type, List<IComponent>>();
    

    public Entity CreateEntity()
    {
        Entity entity = new Entity { id = (uint)entities.Count};
        entities.Add(entity);
        componentIndexer[entity] = new Dictionary<Type, int>();
        return entity;
    }

    public void DestroyEntity(Entity entity)
    {

        //TODO remove all the components of the entity from the list
        Dictionary<Type, int> entityComponents = componentIndexer[entity];
        foreach(var pair in entityComponents)
        {
            //replace the data of the entity component by the last component from the list
            int lastIndex = components[pair.Key].Count - 1;
            components[pair.Key][pair.Value] = components[pair.Key][lastIndex];

            components[pair.Key].RemoveAt(lastIndex);
        }
        


        componentIndexer.Remove(entity);
        entities.Remove(entity);
    }

    public void AddComponent<T>(Entity entity, T component) where T : IComponent
    {
        Type componentType = typeof(T);
        if(!components.ContainsKey(componentType))
        {
            components[componentType] = new List<IComponent>();
        }

        components[componentType].Add(component);
        int index = components[componentType].Count - 1;
        componentIndexer[entity][componentType] = index;
    }
    
    public IComponent GetComponents<T>(Entity entity) where T : IComponent
    {
        Type componentType = typeof(T);

        if (components.ContainsKey(componentType))
        {
            // Cast the List<IComponent> to List<T>
            int index = componentIndexer[entity][componentType];
            IComponent result = components[componentType][index];
            return result;
        }
        else
        {
            return null;
        }
    }

    public void UpdateComponent<T>(Entity entity, T newComponent)
    {
        Type type = typeof(T);
        int index = componentIndexer[entity][type];
        components[type][index] = (IComponent)newComponent;
    } 

    public List<Entity> GetEntities()
    {
        return entities;
    }
}