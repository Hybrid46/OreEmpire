using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Tilemaps;
using Debug = UnityEngine.Debug;

public class MapGenerator : MonoBehaviour
{
    [Range(32, 1024)] public int mapSize = 256;
    [Range(0.0f, 1.0f)][Tooltip("Terrain roughness. Higher numbers gives more randomness.")] public float terrainRoughness = 1.0f;
    [Range(1.0f, 100.0f)][Tooltip("Terrain micro roughness. Higher numbers gives more micro randomness.")] public float terrainMicroRoughness = 10.0f;
    [Range(0.0f, 1.0f)][Tooltip("Average terrain height.")] public float averageHeight = 0.5f;
    [Range(1, 10)][Tooltip("Iterations for Diamond Square Algo -> Higher values makes the terrain more natural.")] public int dsqIterations = 5;
    [Range(0, 10)][Tooltip("Iteration count for smoothing the terrain.")] public int smoothingIterations = 1;

    public Tilemap tileMap;
    public List<RuleTile> ruleTiles;

    [Tooltip("Seeds for map generation. dsqIterations count and seed count must be qual!")] public List<Vector2> seeds;

    public float[,] heightMap { get; private set; }
    [SerializeField] private Texture2D mapTexture;

    void Start()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        //TODO -> job
        Generate();
        stopwatch.Stop();
        Debug.Log($"Generate time -> {stopwatch.ElapsedMilliseconds} ms");
        stopwatch.Reset();

        stopwatch.Start();
        SmoothTerrain(smoothingIterations);
        stopwatch.Stop();
        Debug.Log($"SmoothTerrain time -> {stopwatch.ElapsedMilliseconds} ms");
        stopwatch.Reset();

        stopwatch.Start();
        GenerateMapTexture();
        stopwatch.Stop();
        Debug.Log($"GenerateMapTexture time -> {stopwatch.ElapsedMilliseconds} ms");
        stopwatch.Reset();
    }

    public float GetHeight(int x, int y) => heightMap[x, y];

    public float GetHeight(Vector3Int pos) => heightMap[pos.x, pos.y];

    private void Generate()
    {
        heightMap = new float[mapSize, mapSize];

        DiamondSquareGenerator dsqg = new DiamondSquareGenerator(mapSize, terrainRoughness, averageHeight, terrainMicroRoughness, seeds.ToArray());
        heightMap = dsqg.GenerateTerrain(dsqIterations);

        tileMap.enabled = false;

        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                Vector3Int position = new Vector3Int(x, y);

                if (heightMap[x, y] > 0.5f) tileMap.SetTile(position, ruleTiles[0]);
            }
        }

        tileMap.enabled = true;
    }

    private void GenerateMapTexture()
    {
        mapTexture = new Texture2D(mapSize, mapSize, TextureFormat.RGB24, false);

        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                float height = heightMap[x, y];
                mapTexture.SetPixel(x, y, new Color(height, height, height));
            }
        }

        mapTexture.Apply();
    }

    private void SmoothTerrain(int iterations)
    {
        for (int iteration = 0; iteration < iterations; iteration++)
        {
            for (int x = 0; x < heightMap.GetLength(0) - 1; x += 2)
            {
                for (int y = 0; y < heightMap.GetLength(1) - 1; y += 2)
                {
                    SmoothSquare(x, y);
                }
            }
        }

        void SmoothSquare(int x, int y)
        {
            float average = (heightMap[x, y] + heightMap[x + 1, y] +
                             heightMap[x, y + 1] + heightMap[x + 1, y + 1]) * 0.25f;

            heightMap[x, y] = average;
            heightMap[x + 1, y] = average;
            heightMap[x, y + 1] = average;
            heightMap[x + 1, y + 1] = average;
        }
    }

}