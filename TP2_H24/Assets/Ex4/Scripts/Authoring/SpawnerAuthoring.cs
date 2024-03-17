using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using System;
public class SpawnerAuthoring : MonoBehaviour
{
    public Ex4Config config;
    public GameObject predatorPrefab;
    public GameObject preyPrefab;
    public GameObject plantPrefab;

    class Baker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            var size = (float)authoring.config.gridSize;
            var ratio = Camera.main!.aspect;
            var _height = (int)Math.Round(Math.Sqrt(size / ratio));
            var _width = (int)Math.Round(size / _height);
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new SpawnerConfig
            {
                plantPrefabEntity = GetEntity(authoring.plantPrefab, TransformUsageFlags.Dynamic),
                predatorPrefabEntity = GetEntity(authoring.predatorPrefab, TransformUsageFlags.Dynamic),
                preyPrefabEntity = GetEntity(authoring.preyPrefab, TransformUsageFlags.Dynamic),
                plantCount = authoring.config.plantCount,
                predatorCount = authoring.config.predatorCount,
                preyCount = authoring.config.preyCount,
                height = _height,
                width = _width
                
            });
        }
    }
}

struct SpawnerConfig : IComponentData
{
    public Entity predatorPrefabEntity;
    public Entity preyPrefabEntity;
    public Entity plantPrefabEntity;
    public int height;
    public int width;
    public int plantCount;
    public int predatorCount;
    public int preyCount;
}