using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


public class PositionDataAuthoring : MonoBehaviour
{
    public float2 position;


    // In baking, this Baker will run once for every SpawnerAuthoring instance in a subscene.
    // (Note that nesting an authoring component's Baker class inside the authoring MonoBehaviour class
    // is simply an optional matter of style.)
    class Baker : Baker<PositionDataAuthoring>
    {
        public override void Bake(PositionDataAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PositionComponentData
            {
                Position = authoring.position,
            });
        }
    }
}