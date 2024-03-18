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
    public Ex4Config config;
    public GameObject predatorPrefab;
    public GameObject preyPrefab;
    public GameObject plantPrefab;

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

    void Start()
    {
        var size = (float)config.gridSize;
        var ratio = Camera.main!.aspect;
        _height = (int)Math.Round(Math.Sqrt(size / ratio));
        _width = (int)Math.Round(size / _height);
    }


}
