using Unity.Entities;

public struct LifetimeComponent : IComponentData
{
    public float DecreasingFactor;
    public float TimeRemaining;
    public float StartingLifetime;
}