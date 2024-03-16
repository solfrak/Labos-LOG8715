using System;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Unity.Collections;
using Unity.Jobs;

public class Ex4Spawner : MonoBehaviour
{
    public static Transform[] PlantTransforms;
    public static Transform[] PreyTransforms;
    public static Transform[] PredatorTransforms;

    public static Lifetime[] PlantLifetimes;
    public static Lifetime[] PreyLifetimes;
    public static Lifetime[] PredatorLifetimes;

    public static Velocity[] PlantVelocities;
    public static Velocity[] PreyVelocities;
    public static Velocity[] PredatorVelocities;
    public Ex4Config config;
    public GameObject predatorPrefab;
    public GameObject preyPrefab;
    public GameObject plantPrefab;
    NativeArray<Vector3> plantPositions;
    NativeArray<Vector3> predatorPositions;
    NativeArray<Vector3> preyPositions;
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
        plantPositions = new NativeArray<Vector3>(PlantTransforms.Length, Allocator.Persistent);
        predatorPositions = new NativeArray<Vector3>(PredatorTransforms.Length, Allocator.Persistent);
        preyPositions = new NativeArray<Vector3>(PreyTransforms.Length, Allocator.Persistent);
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
        PlantVelocities = new Velocity[config.plantCount];
        for (var i = 0; i < config.plantCount; i++)
        {
            var go = Create(plantPrefab);
            PlantTransforms[i] = go.transform;
            PlantLifetimes[i] = go.GetComponent<Lifetime>();
            PlantVelocities[i] = go.GetComponent<Velocity>();
        }

        PreyTransforms = new Transform[config.preyCount];
        PreyLifetimes = new Lifetime[config.preyCount];
        PreyVelocities = new Velocity[config.preyCount];
        for (var i = 0; i < config.preyCount; i++)
        {
            var go = Create(preyPrefab);
            PreyTransforms[i] = go.transform;
            PreyLifetimes[i] = go.GetComponent<Lifetime>();
            PreyVelocities[i] = go.GetComponent<Velocity>();
        }

        PredatorTransforms = new Transform[config.predatorCount];
        PredatorLifetimes = new Lifetime[config.predatorCount];
        PredatorVelocities = new Velocity[config.predatorCount];
        for (var i = 0; i < config.predatorCount; i++)
        {
            var go = Create(predatorPrefab);
            PredatorTransforms[i] = go.transform;
            PredatorLifetimes[i] = go.GetComponent<Lifetime>();
            PredatorVelocities[i] = go.GetComponent<Velocity>();
        }
    }

    private void Update()
    {
        GetPositions();
        MovePreyToPlant();
        getPlantLifeTimeJobHandle();
        getPreyLifeTimeJobHandle();
        getPredatorLifeTimeJobHandle();
        MovePredatorToPrey();


    }

    private void getPlantLifeTimeJobHandle()
    {
        // prey Lifetime
        NativeArray<float> decreasingFactors = new NativeArray<float>(PlantLifetimes.Length, Allocator.TempJob);

        for (int i = 0; i < PlantLifetimes.Length; i++)
        {
            decreasingFactors[i] = PlantLifetimes[i].decreasingFactor;
        }


        JobPlantLifeTime plantLifeTimeJob = new JobPlantLifeTime
        {
            preyPositions = preyPositions,
            plantPositions = plantPositions,
            decreasingFactors = decreasingFactors
        };
        JobHandle jobHandle = plantLifeTimeJob.Schedule(PlantLifetimes.Length, 100);
        jobHandle.Complete();
        for (int i = 0; i < PlantLifetimes.Length; i++)
        {
            PlantLifetimes[i].decreasingFactor = decreasingFactors[i];
        }
        decreasingFactors.Dispose();
    }

    private void getPreyLifeTimeJobHandle()
    {
        NativeArray<float> decreasingFactors = new NativeArray<float>(PreyLifetimes.Length, Allocator.TempJob);
        NativeArray<bool> reproduced = new NativeArray<bool>(PreyLifetimes.Length, Allocator.TempJob);
        for (int i = 0; i < PreyLifetimes.Length; i++)
        {
            decreasingFactors[i] = PreyLifetimes[i].decreasingFactor;
            reproduced[i] = PreyLifetimes[i].reproduced;
        }

        JobPreyLifeTime preyLifeTimeJob = new JobPreyLifeTime
        {
            preyPositions = preyPositions,
            predatorPositions = predatorPositions,
            plantPositions = plantPositions,
            decreasingFactors = decreasingFactors,
            reproduced = reproduced,
        };
        JobHandle jobHandle = preyLifeTimeJob.Schedule(PreyLifetimes.Length, 100);
        jobHandle.Complete();
        for (int i = 0; i < PreyLifetimes.Length; i++)
        {
            PreyLifetimes[i].decreasingFactor = decreasingFactors[i];
            PreyLifetimes[i].reproduced = reproduced[i];

        }
        decreasingFactors.Dispose();
        reproduced.Dispose();
    }

    private void getPredatorLifeTimeJobHandle()
    {
        NativeArray<float> decreasingFactors = new NativeArray<float>(PredatorLifetimes.Length, Allocator.TempJob);
        NativeArray<bool> reproduced = new NativeArray<bool>(PredatorLifetimes.Length, Allocator.TempJob);
        for (int i = 0; i < PredatorLifetimes.Length; i++)
        {
            decreasingFactors[i] = PredatorLifetimes[i].decreasingFactor;
            reproduced[i] = PredatorLifetimes[i].reproduced;
        }

        PredatorLifeTimeJob predatorLifeTimeJob = new PredatorLifeTimeJob
        {
            predatorPositions = predatorPositions,
            preyPositions = preyPositions,
            decreasingFactors = decreasingFactors,
            reproduced = reproduced
        };
        JobHandle jobHandle = predatorLifeTimeJob.Schedule(PredatorLifetimes.Length, 100);
        jobHandle.Complete();
        for (int i = 0; i < PredatorLifetimes.Length; i++)
        {
            PredatorLifetimes[i].decreasingFactor = decreasingFactors[i];
            PredatorLifetimes[i].reproduced = reproduced[i];
        }
        decreasingFactors.Dispose();
        reproduced.Dispose();
    }

    private void MovePredatorToPrey()
    {
        NativeArray<Vector3> velocities = new NativeArray<Vector3>(PredatorTransforms.Length, Allocator.TempJob);

        MoveJob moveJob = new MoveJob
        {
            goToPositions = preyPositions,
            ourPositions = predatorPositions,
            velocities = velocities,
            speed = Ex4Config.PredatorSpeed,
        };
        //moveJobHandles[i] = moveJob.Schedule(PredatorVelocities.Length,100);
        //}
        JobHandle jobHandle = moveJob.Schedule(PredatorVelocities.Length, 100);
        jobHandle.Complete();

        for (int i = 0; i < PredatorVelocities.Length; i++)
        {
            PredatorVelocities[i].velocity = velocities[i];
            PredatorTransforms[i].localPosition += PredatorVelocities[i].velocity * Time.deltaTime;
        }

        velocities.Dispose();

    }
    private void MovePreyToPlant()
    {
        NativeArray<Vector3> velocities = new NativeArray<Vector3>(PreyTransforms.Length, Allocator.TempJob);
        MoveJob moveJob = new MoveJob
        {
            goToPositions = plantPositions,
            ourPositions = preyPositions,
            velocities = velocities,
            speed = Ex4Config.PreySpeed,
        };
        JobHandle jobHandle = moveJob.Schedule(PreyVelocities.Length, 100);
        jobHandle.Complete();
        for (int i = 0; i < PreyVelocities.Length; i++)
        {
            PreyVelocities[i].velocity = velocities[i];
            PreyTransforms[i].localPosition += PreyVelocities[i].velocity * Time.deltaTime;
        }
        velocities.Dispose();

    }


    private GameObject Create(GameObject prefab)
    {
        var go = Instantiate(prefab);
        Respawn(go.transform);
        return go;
    }
}
