using Unity.Burst;
using Unity.Entities;

[BurstCompile]
public struct LifetimeComponent : IComponentData
{
    public float DecreasingFactor;
    public float TimeRemaining;
    public float StartingLifetime;
}