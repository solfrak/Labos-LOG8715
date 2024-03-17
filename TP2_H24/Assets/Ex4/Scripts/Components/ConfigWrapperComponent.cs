using Unity.Entities;

public struct ConfigWrapperComponent : IComponentData
{
    public int NbPlants;
    public int NbPreys;
    public int NbPredators;

    public float PreySpeed;
    public float PredatorSpeed;
    public float TouchingDistance;

    public int Width;
    public int Height;
}
