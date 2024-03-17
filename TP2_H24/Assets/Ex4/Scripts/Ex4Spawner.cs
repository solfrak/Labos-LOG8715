using System;
using UnityEngine;
using Random = UnityEngine.Random;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Profiling;
using Unity.Entities;
using Unity.Mathematics;
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
    NativeArray<Vector3> predatorPositions;
    NativeArray<Vector3> preyPositions;

    private int _height;
    private int _width;

    public int Height { get { return _height; } }
    public int Width { get { return _width; } }
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


        //PlantTransforms = new Transform[config.plantCount];
        //PlantLifetimes = new Lifetime[config.plantCount];
        //for (var i = 0; i < config.plantCount; i++)
        //{
        //    var go = Create(plantPrefab);
        //    PlantTransforms[i] = go.transform;
        //    PlantLifetimes[i] = go.GetComponent<Lifetime>();
        //}

        //PreyTransforms = new Transform[config.preyCount];
        //PreyLifetimes = new Lifetime[config.preyCount];
        //for (var i = 0; i < config.preyCount; i++)
        //{
        //    var go = Create(preyPrefab);
        //    PreyTransforms[i] = go.transform;
        //    PreyLifetimes[i] = go.GetComponent<Lifetime>();
        //}

        //PredatorTransforms = new Transform[config.predatorCount];
        //PredatorLifetimes = new Lifetime[config.predatorCount];
        //for (var i = 0; i < config.predatorCount; i++)
        //{
        //    var go = Create(predatorPrefab);
        //    PredatorTransforms[i] = go.transform;
        //    PredatorLifetimes[i] = go.GetComponent<Lifetime>();
        //}
    }

    private void Update()
    {
   
        Profiler.BeginSample("GetPosition");
        //GetPositions();
        Profiler.EndSample();

        plantPositions.Dispose();
        predatorPositions.Dispose();
        preyPositions.Dispose();
        Destroy();
    }

    private void Destroy()
    {

    }

    //private GameObject Create(GameObject prefab)
    //{
    //    var go = Instantiate(prefab);
    //    Respawn(go.transform);
    //    return go;
    //}
}
