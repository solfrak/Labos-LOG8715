using System;
using System.Collections.Generic;

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





