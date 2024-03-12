using Unity.Collections;
using UnityEngine;
using Unity.Jobs;
using Unity.Entities.UniversalDelegates;
public class ChangePredatorLifetime : MonoBehaviour
{
    private Lifetime _lifetime;
    NativeArray<Vector3> firstPosition;
    NativeArray<Vector3> secondPosition;

    public void Start()
    {
        _lifetime = GetComponent<Lifetime>();
        firstPosition = new NativeArray<Vector3>(Ex4Spawner.PreyTransforms.Length, Allocator.Persistent);
        secondPosition = new NativeArray<Vector3>(Ex4Spawner.PredatorTransforms.Length, Allocator.Persistent);

    }

    public void Update()
    {
        _lifetime.decreasingFactor = 1.0f;
        for (int i = 0; i < Ex4Spawner.PredatorTransforms.Length; i++)
        {
            secondPosition[i] = Ex4Spawner.PredatorTransforms[i].position;
        }

        for (int i = 0; i < Ex4Spawner.PreyTransforms.Length;i++)
        {
            firstPosition[i] = Ex4Spawner.PreyTransforms[i].position;
        }
  
        ReproduceJob reproduceJob = new ReproduceJob
        {
            reproduce = _lifetime.reproduced,
            firstPosition = secondPosition,
            stuffPosition = transform.position,
        };


        LifeTimeJob changeLifetime = new LifeTimeJob
        {
            decreasingFactor = _lifetime.decreasingFactor,
            firstPosition = firstPosition,
            stuffPosition = transform.position,
            factor = 0.5f,
        };
        JobHandle jobHandleReproduce = reproduceJob.Schedule();
        JobHandle jobHandle = changeLifetime.Schedule();
        jobHandleReproduce.Complete();
        jobHandle.Complete();

    }
}