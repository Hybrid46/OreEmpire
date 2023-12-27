using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Tilemaps;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class MapGenerator : Singleton<MapGenerator>
{
    [Range(32, 1024)] public int mapSize = 256;
    [Range(0.0f, 1.0f)][Tooltip("Terrain roughness. Higher numbers gives more randomness.")] public float terrainRoughness = 1.0f;
    [Range(1.0f, 100.0f)][Tooltip("Terrain micro roughness. Higher numbers gives more micro randomness.")] public float terrainMicroRoughness = 10.0f;
    [Range(0.0f, 1.0f)][Tooltip("Average terrain height.")] public float averageHeight = 0.5f;
    [Range(1, 10)][Tooltip("Iterations for Diamond Square Algo -> Higher values makes the terrain more natural.")] public int dsqIterations = 5;
    [Range(0, 10)][Tooltip("Iteration count for smoothing the terrain.")] public int smoothingIterations = 1;

    public Tilemap terrainTileMap;
    public Tilemap propsRocksTileMap;
    public List<RuleTile> ruleTiles;

    [Tooltip("Seeds for map generation. dsqIterations count and seed count must be qual!")] public List<Vector2> seeds;

    [SerializeField] private Texture2D mapTexture;

    [Flags]
    public enum TileHeightType : byte
    {
        None = 0,
        Ground = 1 << 0,
        Water = 1 << 1,
        Cliff = 1 << 2,
    }

    public float[,] heightMap { get; private set; }

    public TileHeightType[,] heightMapType { get; private set; }

    public enum MapType { Summer, Winter, DeepOcean }

    [Serializable]
    public struct MapSettings
    {
        public MapType type;
        [Range(0.0f, 0.9f)] public float oreFieldSize;
        public float oreFieldSpacing;
        [Range(0.0f, 1.0f)] public float cliffContinuity;
    }

    public MapSettings mapSettings;

    public enum OreType
    {
        None,
        Copper,
        Sand,
        Water
    }

    public OreType[,] oreMap;

    [Serializable]
    public struct Ore
    {
        public OreType type;
        public RuleTile tile;
    }

    public List<Ore> ores;
    public Dictionary<OreType, RuleTile> oresD;

    void Start()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        //TODO -> job
        Generate();
        InitializeOres();
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

        stopwatch.Start();
        GenerateOresAndCliffs();
        stopwatch.Stop();
        Debug.Log($"GenerateOresAndCliffs time -> {stopwatch.ElapsedMilliseconds} ms");
        stopwatch.Reset();
    }

    public float GetHeight(int x, int y) => heightMap[x, y];

    public float GetHeight(Vector3Int pos) => heightMap[pos.x, pos.y];

    private void Generate()
    {
        heightMap = new float[mapSize, mapSize];

        DiamondSquareGenerator dsqg = new DiamondSquareGenerator(mapSize, terrainRoughness, averageHeight, terrainMicroRoughness, seeds.ToArray());
        heightMap = dsqg.GenerateTerrain(dsqIterations);

        terrainTileMap.enabled = false;

        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                Vector3Int position = new Vector3Int(x, y);
                int tileIndex = (int)(heightMap[x, y] * (ruleTiles.Count - 1));

                terrainTileMap.SetTile(position, ruleTiles[tileIndex]);
            }
        }

        terrainTileMap.enabled = true;
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

    private void InitializeOres()
    {
        oreMap = new OreType[mapSize, mapSize];

        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                oreMap[x, y] = OreType.None;
            }
        }

        oresD = new Dictionary<OreType, RuleTile>(ores.Count);

        ores.ForEach(ore => oresD.Add(ore.type, ore.tile));
    }

    private void GenerateOresAndCliffs()
    {
        List<Vector2> poissonPoints = GeneratePoissonPoints(5.0f, 20);

        //generate ores and cliffs
        foreach (Vector2 point in poissonPoints)
        {
            Vector2Int roundedPoint = Vector2Int.RoundToInt(point);

            //TODO floodfill ores and make cliffs

            //testing
            OreType oreType = ores[Random.Range(1, ores.Count) - 1].type;

            FloodFillOres(roundedPoint.x, roundedPoint.y, oreType);
        }
    }

    private RuleTile GetOreRuleTile(OreType oreType) => oresD[oreType];

    void FloodFillOres(int x, int y, OreType oreType)
    {
        // Stop if out of bounds, chance of spawning less then needed or already filled
        if (!IsOnMap(x,y) ||
            oreMap[x, y] != OreType.None ||
            Random.Range(0.0f, 1.0f) < mapSettings.oreFieldSize)
        {
            return;
        }

        // Fill the current cell wit ore
        oreMap[x, y] = oreType;
        propsRocksTileMap.SetTile(new Vector3Int(x, y), GetOreRuleTile(oreType));

        // Recursively fill in 4 directions
        FloodFillOres(x + 1, y, oreType);
        FloodFillOres(x - 1, y, oreType);
        FloodFillOres(x, y + 1, oreType);
        FloodFillOres(x, y - 1, oreType);
    }

    private List<Vector2> GeneratePoissonPoints(float minDistance, int maxAttempts)
    {
        Vector2 bounds = new Vector2(mapSize - 1, mapSize - 1);
        List<Vector2> points = new List<Vector2>();
        float cellSize = minDistance / Mathf.Sqrt(2);
        int[,] grid = new int[Mathf.CeilToInt(bounds.x / cellSize), Mathf.CeilToInt(bounds.y / cellSize)];
        List<Vector2> activeList = new List<Vector2>();

        Vector2 firstPoint = new Vector2(Random.Range(0, bounds.x), Random.Range(0, bounds.y));
        points.Add(firstPoint);
        activeList.Add(firstPoint);
        grid[Mathf.FloorToInt(firstPoint.x / cellSize), Mathf.FloorToInt(firstPoint.y / cellSize)] = points.Count;

        while (activeList.Count > 0)
        {
            int randomIndex = Random.Range(0, activeList.Count);
            Vector2 point = activeList[randomIndex];
            bool foundPoint = false;

            for (int i = 0; i < maxAttempts; i++)
            {
                float angle = Random.Range(0, Mathf.PI * 2);
                float radius = Random.Range(minDistance, minDistance * 2);
                Vector2 newPoint = new Vector2(point.x + radius * Mathf.Cos(angle), point.y + radius * Mathf.Sin(angle));

                if (newPoint.x >= 0 && newPoint.x < bounds.x && newPoint.y >= 0 && newPoint.y < bounds.y)
                {
                    int cellX = Mathf.FloorToInt(newPoint.x / cellSize);
                    int cellY = Mathf.FloorToInt(newPoint.y / cellSize);
                    bool canPlace = true;

                    for (int x = Mathf.Max(0, cellX - 2); x < Mathf.Min(grid.GetLength(0), cellX + 3); x++)
                    {
                        for (int y = Mathf.Max(0, cellY - 2); y < Mathf.Min(grid.GetLength(1), cellY + 3); y++)
                        {
                            if (grid[x, y] > 0 && Vector2.Distance(points[grid[x, y] - 1], newPoint) < minDistance)
                            {
                                canPlace = false;
                                break;
                            }
                        }
                    }

                    if (canPlace)
                    {
                        points.Add(newPoint);
                        activeList.Add(newPoint);
                        grid[cellX, cellY] = points.Count;
                        foundPoint = true;
                    }
                }
            }

            if (!foundPoint)
            {
                activeList.RemoveAt(randomIndex);
            }
        }

        return points;
    }

    public bool IsOnMap(int x, int y) => x >= 0 && x < mapSize && y >= 0 && y < mapSize;
}