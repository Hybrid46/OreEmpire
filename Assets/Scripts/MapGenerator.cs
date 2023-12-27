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

    private RuleTile waterTile; //this ruletile contains the water or lava ruletile which determines is the TileHeightType -> Water ?

    [Tooltip("Seeds for map generation. dsqIterations count and seed count must be qual!")] public List<Vector2> seeds;

    [SerializeField] private Texture2D mapTexture;

    [Serializable] public enum TileHeightType : byte { Ground, Water, Cliff }
    [Serializable] public enum MapType { Summer, Winter, DeepOcean }
    [Serializable] public enum TransportDirection { None, Up, Down, Right, Left } //TODO for ore transportation belts -> should I use this with a dictionary O(1+hash) instead of a switch O(1-4)?

    [Serializable]
    public struct Ore
    {
        public OreType type;
        public RuleTile tile;
    }

    [Serializable]
    public struct GridCell
    {
        public float height;
        public TileHeightType heightType;
        public OreType oreType;
        public bool built;

        public GridCell(float height)
        {
            this.height = height;
            this.heightType = TileHeightType.Ground;
            this.oreType = OreType.None;
            this.built = false;
        }
    }

    public GridCell[,] grid { get; private set; }

    [Serializable]
    public struct MapSettings
    {
        public MapType type;
        [Range(0.0f, 0.9f)] public float oreFieldSize;
        public float oreFieldSpacing;
        [Range(0.0f, 1.0f)] public float cliffContinuity; //TODO -> use it in floodfill when generating cliffs
    }

    public MapSettings mapSettings;

    public enum OreType
    {
        None,
        Copper,
        Sand,
        Water
    }

    public List<Ore> ores;
    public Dictionary<OreType, RuleTile> oresD;

    void Start()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        //TODO -> job
        InitializeOres();
        InitRuleTiles();
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

        stopwatch.Start();
        GenerateOresAndCliffs();
        stopwatch.Stop();
        Debug.Log($"GenerateOresAndCliffs time -> {stopwatch.ElapsedMilliseconds} ms");
        stopwatch.Reset();
    }

    private void Generate()
    {
        grid = new GridCell[mapSize, mapSize];

        DiamondSquareGenerator dsqg = new DiamondSquareGenerator(mapSize, terrainRoughness, averageHeight, terrainMicroRoughness, seeds.ToArray());
        float[,] heightMap = dsqg.GenerateTerrain(dsqIterations);

        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                grid[x, y] = new GridCell(heightMap[x, y]);

                Vector3Int position = new Vector3Int(x, y);
                int tileIndex = (int)(grid[x, y].height * (ruleTiles.Count - 1));

                terrainTileMap.SetTile(position, ruleTiles[tileIndex]);

                if (ruleTiles[tileIndex] == waterTile) grid[x, y].heightType = TileHeightType.Water;
                //if (ruleTiles[tileIndex] == cliffTile) grid[x, y].heightType = TileHeightType.Cliff; //TODO cliff generation
            }
        }
    }

    private void GenerateMapTexture()
    {
        mapTexture = new Texture2D(mapSize, mapSize, TextureFormat.RGB24, false);

        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                float height = grid[x, y].height;
                mapTexture.SetPixel(x, y, new Color(height, height, height));
            }
        }

        mapTexture.Apply();
    }

    private void SmoothTerrain(int iterations)
    {
        for (int iteration = 0; iteration < iterations; iteration++)
        {
            for (int x = 0; x < mapSize - 1; x += 2)
            {
                for (int y = 0; y < mapSize - 1; y += 2)
                {
                    SmoothSquare(x, y);
                }
            }
        }

        void SmoothSquare(int x, int y)
        {
            float average = (grid[x, y].height + grid[x + 1, y].height +
                             grid[x, y + 1].height + grid[x + 1, y + 1].height) * 0.25f;

            grid[x, y].height = average;
            grid[x + 1, y].height = average;
            grid[x, y + 1].height = average;
            grid[x + 1, y + 1].height = average;
        }
    }

    private void InitializeOres()
    {
        oresD = new Dictionary<OreType, RuleTile>(ores.Count);

        ores.ForEach(ore => oresD.Add(ore.type, ore.tile));
    }

    private void InitRuleTiles()
    {
        ruleTiles.ForEach(tile => { if (tile.name.Contains("Water") || tile.name.Contains("Lava")) waterTile = tile; });
    }

    private void GenerateOresAndCliffs()
    {
        List<Vector2> poissonPoints = GeneratePoissonPoints(mapSettings.oreFieldSpacing, 20);

        foreach (Vector2 point in poissonPoints)
        {
            Vector2Int roundedPoint = Vector2Int.RoundToInt(point);

            //TODO make cliffs

            OreType oreType = ores[Random.Range(1, ores.Count) - 1].type;

            FloodFillOres(roundedPoint.x, roundedPoint.y, oreType);
        }
    }

    void FloodFillOres(int x, int y, OreType oreType)
    {
        // Stop if out of bounds, chance of spawning less then needed or already filled
        if (!IsOnMap(x, y) ||
            grid[x, y].oreType != OreType.None ||
            Random.Range(0.0f, 1.0f) < mapSettings.oreFieldSize)
        {
            return;
        }

        // Fill the current cell wit ore
        grid[x, y].oreType = oreType;
        propsRocksTileMap.SetTile(new Vector3Int(x, y), oresD[oreType]);

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

    public bool IsOnMap(int x, int y)
    {
        if (x < 0 || x >= mapSize) return false;
        if (y < 0 || y >= mapSize) return false;
        return true;
    }
}