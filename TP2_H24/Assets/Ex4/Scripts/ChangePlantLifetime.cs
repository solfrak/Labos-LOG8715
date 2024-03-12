using Unity.Collections;
using UnityEngine;
using Unity.Jobs;
public class ChangePlantLifetime : MonoBehaviour
{
    private Lifetime _lifetime;
    NativeArray<Vector3> firstPosition;
    public void Start()
    {
        _lifetime = GetComponent<Lifetime>();
        firstPosition = new NativeArray<Vector3>(Ex4Spawner.PreyTransforms.Length, Allocator.Persistent);
    }

    public void Update()
    {
        _lifetime.decreasingFactor = 1.0f;
        for (int i = 0 ; i < Ex4Spawner.PreyTransforms.Length;i++)
        {
            firstPosition[i] = Ex4Spawner.PreyTransforms[i].position;
        }

        LifeTimeJob changeLifetime = new LifeTimeJob
        {
            decreasingFactor = _lifetime.decreasingFactor,
            firstPosition = firstPosition,
            stuffPosition = transform.position,
            factor = 2f,
        };
    
         JobHandle jobHandle = changeLifetime.Schedule();
        
        jobHandle.Complete();
    }
}