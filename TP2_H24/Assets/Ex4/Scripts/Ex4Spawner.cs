using System;
using UnityEngine;
using Random = UnityEngine.Random;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Profiling;

public class Ex4Spawner : MonoBehaviour
{
    public static Transform[] PlantTransforms;
    public static Transform[] PreyTransforms;
    public static Transform[] PredatorTransforms;

    public static Lifetime[] PlantLifetimes;
    public static Lifetime[] PreyLifetimes;
    public static Lifetime[] PredatorLifetimes;

    public Ex4Config config;
    public GameObject predatorPrefab;
    public GameObject preyPrefab;
    public GameObject plantPrefab;
    NativeArray<Vector3> plantPositions;
    NativeArray<float> plantDecreasingFactors;
    NativeArray<Vector3> predatorPositions;
    NativeArray<Vector3> preyPositions;
    NativeArray<Vector3> preyVelocities;
    NativeArray<Vector3> predatorVelocities;
    NativeArray<float> preyDecreasingFactors;
    NativeArray<float> predatorDecreasingFactors;
    NativeArray<bool> preyReproduced;
    NativeArray<bool> predatorReproduced;
    private int _height;
    private int _width;

    public static Ex4Spawner Instance { get; private set; }

    public void Respawn(Transform t)
    {
        var halfWidth = _width / 2;
        var halfHeight = _height / 2;
        t.position = new Vector3Int(Random.Range(-halfWidth, halfWidth), Random.Range(-halfHeight, halfHeight));
    }

    private void Awake()
    {
        Instance = this;
    }
    void GetPositions()
    {
        plantPositions = new NativeArray<Vector3>(PlantTransforms.Length, Allocator.TempJob);
        predatorPositions = new NativeArray<Vector3>(PredatorTransforms.Length, Allocator.TempJob);
        preyPositions = new NativeArray<Vector3>(PreyTransforms.Length, Allocator.TempJob);

        //TODO make those 3 for loop parallel
        for (int i = 0; i < PlantTransforms.Length; i++)
        {
            plantPositions[i] = PlantTransforms[i].position;
        }
        for (int i = 0; i < PredatorTransforms.Length; i++)
        {
            predatorPositions[i] = PredatorTransforms[i].position;
        }
        for (int i = 0; i < PreyTransforms.Length; i++)
        {
            preyPositions[i] = PreyTransforms[i].position;
        }
    }
    void Start()
    {
        var size = (float)config.gridSize;
        var ratio = Camera.main!.aspect;
        _height = (int)Math.Round(Math.Sqrt(size / ratio));
        _width = (int)Math.Round(size / _height);

        PlantTransforms = new Transform[config.plantCount];
        PlantLifetimes = new Lifetime[config.plantCount];
        for (var i = 0; i < config.plantCount; i++)
        {
            var go = Create(plantPrefab);
            PlantTransforms[i] = go.transform;
            PlantLifetimes[i] = go.GetComponent<Lifetime>();
        }

        PreyTransforms = new Transform[config.preyCount];
        PreyLifetimes = new Lifetime[config.preyCount];
        for (var i = 0; i < config.preyCount; i++)
        {
            var go = Create(preyPrefab);
            PreyTransforms[i] = go.transform;
            PreyLifetimes[i] = go.GetComponent<Lifetime>();
        }

        PredatorTransforms = new Transform[config.predatorCount];
        PredatorLifetimes = new Lifetime[config.predatorCount];
        for (var i = 0; i < config.predatorCount; i++)
        {
            var go = Create(predatorPrefab);
            PredatorTransforms[i] = go.transform;
            PredatorLifetimes[i] = go.GetComponent<Lifetime>();
        }
    }

    private void Update()
    {
        preyVelocities = new NativeArray<Vector3>(PreyTransforms.Length, Allocator.TempJob);
        predatorVelocities = new NativeArray<Vector3>(PredatorTransforms.Length, Allocator.TempJob);

        preyDecreasingFactors = new NativeArray<float>(PreyLifetimes.Length, Allocator.TempJob);
        predatorDecreasingFactors = new NativeArray<float>(PredatorLifetimes.Length, Allocator.TempJob);
        plantDecreasingFactors = new NativeArray<float>(PlantLifetimes.Length, Allocator.TempJob);

        preyReproduced = new NativeArray<bool>(PreyLifetimes.Length, Allocator.TempJob);
        predatorReproduced = new NativeArray<bool>(PredatorLifetimes.Length, Allocator.TempJob);

        Profiler.BeginSample("GetPosition");
        GetPositions();
        Profiler.EndSample();

        Profiler.BeginSample("MoveJobs");
        MoveJobs();
        Profiler.EndSample();
        Profiler.BeginSample("UpdateLifetime");
        UpdateLifeTimes();
        Profiler.EndSample();

        plantPositions.Dispose();
        predatorPositions.Dispose();
        preyPositions.Dispose();
        Destroy();
    }

    private void Destroy()
    {
        preyVelocities.Dispose();
        predatorVelocities.Dispose();
        preyDecreasingFactors.Dispose();
        predatorDecreasingFactors.Dispose();
        plantDecreasingFactors.Dispose();
        preyReproduced.Dispose();
        predatorReproduced.Dispose();
    }

    private void UpdateLifeTimes()
    {
        JobHandle plantJobHandle = getPlantLifeTimeJobHandle();
        JobHandle preyJobHandle = getPreyLifeTimeJobHandle();
        JobHandle predatorJobHandle = getPredatorLifeTimeJobHandle();
        JobHandle.CombineDependencies(plantJobHandle, preyJobHandle, predatorJobHandle).Complete();
        for (int i = 0; i < PlantLifetimes.Length; i++)
        {
            PlantLifetimes[i].decreasingFactor = plantDecreasingFactors[i];
        }
        for (int i = 0; i < PreyLifetimes.Length; i++)
        {
            PreyLifetimes[i].decreasingFactor = preyDecreasingFactors[i];
            PreyLifetimes[i].reproduced = preyReproduced[i];
        }
        for (int i = 0; i < PredatorLifetimes.Length; i++)
        {
            PredatorLifetimes[i].decreasingFactor = predatorDecreasingFactors[i];
            PredatorLifetimes[i].reproduced = predatorReproduced[i];
        }
    }

    private JobHandle getPlantLifeTimeJobHandle()
    {
        for (int i = 0; i < PlantLifetimes.Length; i++)
        {
            plantDecreasingFactors[i] = PlantLifetimes[i].decreasingFactor;
        }

        JobPlantLifeTime plantLifeTimeJob = new JobPlantLifeTime
        {
            preyPositions = preyPositions,
            plantPositions = plantPositions,
            decreasingFactors = plantDecreasingFactors
        };

        JobHandle jobHandle = plantLifeTimeJob.Schedule(PlantLifetimes.Length, 60);
        return jobHandle;
    }

    private JobHandle getPreyLifeTimeJobHandle()
    {
        for (int i = 0; i < PreyLifetimes.Length; i++)
        {
            preyDecreasingFactors[i] = PreyLifetimes[i].decreasingFactor;
            preyReproduced[i] = PreyLifetimes[i].reproduced;
        }

        JobPreyLifeTime preyLifeTimeJob = new JobPreyLifeTime
        {
            preyPositions = preyPositions,
            predatorPositions = predatorPositions,
            plantPositions = plantPositions,
            decreasingFactors = preyDecreasingFactors,
            reproduced = preyReproduced,
        };

        JobHandle jobHandle = preyLifeTimeJob.Schedule(PreyLifetimes.Length, 60);
        return jobHandle;

    }

    private JobHandle getPredatorLifeTimeJobHandle()
    {
        for (int i = 0; i < PredatorLifetimes.Length; i++)
        {
            predatorDecreasingFactors[i] = PredatorLifetimes[i].decreasingFactor;
            predatorReproduced[i] = PredatorLifetimes[i].reproduced;
        }

        JobPredatorLifeTime predatorLifeTimeJob = new JobPredatorLifeTime
        {
            predatorPositions = predatorPositions,
            preyPositions = preyPositions,
            decreasingFactors = predatorDecreasingFactors,
            reproduced = predatorReproduced
        };
        JobHandle jobHandle = predatorLifeTimeJob.Schedule(PredatorLifetimes.Length, 60);
        return jobHandle;

    }

    private JobHandle MovePredatorToPrey()
    {
        MoveJob moveJob = new MoveJob
        {
            goToPositions = preyPositions,
            ourPositions = predatorPositions,
            velocities = predatorVelocities,
            speed = Ex4Config.PredatorSpeed,
        };
        JobHandle jobHandle = moveJob.Schedule(PredatorTransforms.Length, 60);
        return jobHandle;
    }
    private JobHandle MovePreyToPlant()
    {
        MoveJob moveJob = new MoveJob
        {
            goToPositions = plantPositions,
            ourPositions = preyPositions,
            velocities = preyVelocities,
            speed = Ex4Config.PreySpeed,
        };
        JobHandle jobHandle = moveJob.Schedule(PreyTransforms.Length, 60);
        return jobHandle;
    }
    private void MoveJobs()
    {
        JobHandle moveToPlantHandle = MovePreyToPlant();
        JobHandle moveToPreyHandle = MovePredatorToPrey();
        JobHandle.CombineDependencies(moveToPlantHandle, moveToPreyHandle).Complete();
        for (int i = 0; i < PreyTransforms.Length; i++)
        {
            PreyTransforms[i].localPosition += preyVelocities[i] * Time.deltaTime;
            PreyLifetimes[i].decreasingFactor = preyDecreasingFactors[i];
            PreyLifetimes[i].reproduced = preyReproduced[i];
        }


        for (int i = 0; i < PredatorTransforms.Length; i++)
        {
            PredatorTransforms[i].localPosition += predatorVelocities[i] * Time.deltaTime;
            PredatorLifetimes[i].decreasingFactor = predatorDecreasingFactors[i];
            PredatorLifetimes[i].reproduced = predatorReproduced[i];
        }

    }

    private GameObject Create(GameObject prefab)
    {
        var go = Instantiate(prefab);
        Respawn(go.transform);
        return go;
    }
}
