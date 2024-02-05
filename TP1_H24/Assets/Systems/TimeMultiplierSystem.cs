using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;

public class TimeMultiplierSystem : ISystem
{
    public string Name {get; set;}
    private IEntityManager EntityManager;

    private const int Multipier = 4;

    public TimeMultiplierSystem(IEntityManager entityManager)
    {
        Name = "TimeMultiplierSystem";
        EntityManager = entityManager;
    }

    public void UpdateSystem()
    {
        for(int i = 0; i < Multipier - 1; i++)
        {
            foreach (var system in ECSController.Instance.AllSystems)
            {
                var leftSideExecute = system.GetType().GetMethod("UpdateLeftSide");
                if(leftSideExecute != null) leftSideExecute.Invoke(system, null);
            }
        }
    }
}