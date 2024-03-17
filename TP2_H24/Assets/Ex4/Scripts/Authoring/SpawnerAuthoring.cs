using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;


public class SpawnerAuthoring : MonoBehaviour
{
    public GameObject predatorPrefab;
    public GameObject preyPrefab;
    public GameObject plantPrefab;
    public int plantCount;
    public int predatorCount;
    public int preyCount;

    class Baker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new SpawnerConfig
            {
                plantPrefabEntity = GetEntity(authoring.plantPrefab, TransformUsageFlags.Dynamic),
                predatorPrefabEntity = GetEntity(authoring.predatorPrefab, TransformUsageFlags.Dynamic),
                preyPrefabEntity = GetEntity(authoring.preyPrefab, TransformUsageFlags.Dynamic),
                plantCount = authoring.plantCount,
                predatorCount = authoring.predatorCount,
                preyCount = authoring.preyCount

            });
        }
    }
}

struct SpawnerConfig : IComponentData
{
    public Entity predatorPrefabEntity;
    public Entity preyPrefabEntity;
    public Entity plantPrefabEntity;
    public int plantCount;
    public int predatorCount;
    public int preyCount;
}