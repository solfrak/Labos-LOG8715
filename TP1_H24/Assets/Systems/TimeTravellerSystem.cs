using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;

public class TimeTravellerSystem : ISystem
{
    public string Name { get; set;}
    
    public TimeTravellerSystem(IEntityManager entityManager)
    {
        Name = "TimeTravellerSystem";
        EntityManager = entityManager;
    } 
    private IEntityManager EntityManager;


    private Queue<Tuple<IState, float>> StateQueue = new Queue<Tuple<IState, float>>();
    private float ElapsedCooldown;
    private float ElapsedTime = 0.0f;
    private float AbilityCooldown = 3.0f;
    public void UpdateSystem()
    {
        float currentTime = ElapsedTime;
        ElapsedTime += Time.deltaTime;

        var currentState = EntityManager.GetState();
        StateQueue.Enqueue(new Tuple<IState, float>(currentState, currentTime));

        if(Time.realtimeSinceStartup >= AbilityCooldown)
        {

            if(IsAbilityTrigger())
            {
                var currentEntities = CopyEntitesList();
                EntityManager.UpdateEntityState(StateQueue.Dequeue().Item1);
                var previousEntities = CopyEntitesList();

                InstantiateDestroyedEntity(currentEntities, previousEntities);
                StateQueue.Clear();
                return;
            }
            HandleCoolDown();
            DequeueUntilAbilityCooldown(currentTime);
        }
    }

    private bool IsAbilityTrigger()
    {
        if(Input.GetKey(KeyCode.Space) && ElapsedCooldown <= 0.0f)
        {
            ElapsedCooldown = AbilityCooldown;
            return true;
        }
        
        return false;
    }

    private void HandleCoolDown()
    {
        if(ElapsedCooldown != 0.0f)
        {
            ElapsedCooldown -= Time.deltaTime;
        }
    }

    private void DequeueUntilAbilityCooldown(float currentTime)
    {
        Tuple<IState, float> firstState = StateQueue.Peek();
        while(currentTime - firstState.Item2 >= AbilityCooldown)
        {
            StateQueue.Dequeue();
            firstState = StateQueue.Peek();
        }
    }

    private void InstantiateDestroyedEntity(List<uint> currentEntities, List<uint> previousEntities)
    {
        foreach(var entity in previousEntities)
        {
            if(!currentEntities.Contains(entity))
            {
                PhysicComponent physicComponent = (PhysicComponent)EntityManager.GetComponent<PhysicComponent>(entity);
                ECSController.Instance.CreateShape(entity, physicComponent.size);
            }
        }
    }

    private List<uint> CopyEntitesList()
    {
        List<uint> copy = new List<uint>();
        foreach(var entity in EntityManager.GetEntities())
        {
            copy.Add(entity);
        }

        return copy;
    }
}