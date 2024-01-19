using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour
{

    private static EntityManager _instance;
    private static uint counter = 0;
    public static EntityManager Instance 
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindObjectOfType<EntityManager>();

                if(_instance == null)
                {
                    GameObject obj = new GameObject("_EntityManager");
                    _instance = obj.AddComponent<EntityManager>();
                }
            }
            return _instance;
        }
    }
    private List<Entity> entities = new List<Entity>();

    public void AddEntity(Entity entity)
    {
        entities.Add(entity);
    }

    public List<Entity> GetEntities()
    {
        return entities;
    }

    public uint GetId()
    {
        return counter++;
    }
}