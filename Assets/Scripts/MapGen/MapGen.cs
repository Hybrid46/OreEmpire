using System;
using System.Collections.Generic;
using Unity.Burst;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class MapGen : MonoBehaviour
{
    public enum HeightLevel { Water, Ground, Cliff }

    public const int chunkSize = 16;
    public const int chunkHeight = 10;

    public int mapSizeInMeters;

    public MapSettings mapSettings;

    public float[,] heightMap;
    public HashSet<Chunk> chunks;
    public Material terrainMaterial;

    public Bounds worldBounds = new Bounds();

    public float GetHeight(Vector3 pos)
    {
        int pX = (int)pos.x;
        int pZ = (int)pos.z;

        if (pX < 0 || pX >= heightMap.GetLength(0) ||
            pZ < 0 || pZ >= heightMap.GetLength(1)) return 0f;

        return heightMap[(int)pos.x, (int)pos.z];
    }

    private void Start()
    {
        DateTime exectime = DateTime.Now;

        mapSizeInMeters = mapSettings.mapSize * chunkSize;
        heightMap = new float[mapSizeInMeters + 1, mapSizeInMeters + 1];

        GenerateHeightMap();
        NormalizeHeightMap();
        ApplyMapModifiers();

        Debug.Log($"Heigth map[{(mapSizeInMeters + 1) * (mapSizeInMeters + 1)}] generated in: {(DateTime.Now - exectime).Milliseconds} ms");
        exectime = DateTime.Now;

        chunks = new HashSet<Chunk>(mapSettings.mapSize * mapSettings.mapSize);
        GenerateChunks();

        Debug.Log($"Chunks[{chunks.Count}] Initialized in: {(DateTime.Now - exectime).Milliseconds} ms");
        exectime = DateTime.Now;

        ActivateChunks(true);

        Debug.Log($"Chunks[{chunks.Count}] activated: {(DateTime.Now - exectime).Milliseconds} ms");
    }

    [BurstCompile]
    private void NormalizeHeightMap()
    {
        float min = float.PositiveInfinity;
        float max = float.NegativeInfinity;

        //get min - max
        for (int y = 0; y <= mapSizeInMeters; y++)
        {
            for (int x = 0; x <= mapSizeInMeters; x++)
            {
                float height = heightMap[x, y];
                if (height >= max) max = height;
                if (height < min) min = height;
            }
        }

        //normalize to -> 0 - 1
        for (int y = 0; y <= mapSizeInMeters; y++)
        {
            for (int x = 0; x <= mapSizeInMeters; x++)
            {
                float height = heightMap[x, y];
                heightMap[x, y] = Remap(height, min, max, 0f, 1f);
            }
        }

        float Remap(float input, float oldLow, float oldHigh, float newLow, float newHigh)
        {
            float t = Mathf.InverseLerp(oldLow, oldHigh, input);
            return Mathf.Lerp(newLow, newHigh, t);
        }
    }

    public void GenerateChunks()
    {
        for (int y = 0; y < mapSettings.mapSize; y++)
        {
            for (int x = 0; x < mapSettings.mapSize; x++)
            {
                Vector3 worldPosition = new Vector3(x * chunkSize, 0f, y * chunkSize);

                GameObject chunk = new GameObject();
                chunk.transform.position = worldPosition;
                chunk.transform.SetParent(transform, true);
                Chunk chunkComponent = chunk.AddComponent<Chunk>();
                chunks.Add(chunkComponent);
                chunk.SetActive(false);

                worldBounds.Encapsulate(chunkComponent.GetBounds());
            }
        }
    }

    [BurstCompile]
    private void GenerateHeightMap()
    {
        heightMap = new float[mapSizeInMeters + 1, mapSizeInMeters + 1];

        for (int y = 0; y <= mapSizeInMeters; y++)
        {
            for (int x = 0; x <= mapSizeInMeters; x++)
            {
                Vector3 currentWorldPosition = new Vector3(x, 0f, y);

                heightMap[x, y] = GetHeightAverage(currentWorldPosition);
            }
        }
    }

    [BurstCompile]
    private float GetHeightAverage(Vector3 currentWorldPosition)
    {
        float sumHeight = 0f;

        foreach (NoiseSettings noiseSettings in mapSettings.noiseSettings)
        {
            sumHeight += GetPerlinValue(currentWorldPosition, noiseSettings);
        }

        return sumHeight / mapSettings.noiseSettings.Count;
    }

    [BurstCompile]
    private float GetPerlinValue(Vector3 worldPosition, NoiseSettings noiseSettings)
    {
        //TODO
        float scale = noiseSettings.scale/* * mapSettings.mapSize*/;
        float xCoord = worldPosition.x / mapSettings.mapSize * scale + noiseSettings.seedX;
        float zCoord = worldPosition.z / mapSettings.mapSize * scale + noiseSettings.seedY;

        return Mathf.Clamp01(Mathf.PerlinNoise(xCoord, zCoord));
        //For Job -> return noise.cnoise(new float2(xCoord, zCoord));
    }

    [BurstCompile]
    private void ApplyMapModifiers()
    {
        foreach (MapModifier mapModifier in mapSettings.modifiers)
        {
            Vector3[] pattern = StaticUtils.GetPatternCirlce(1f, mapModifier.range, false).ToArray();

            for (int y = 0; y <= mapSizeInMeters; y++)
            {
                for (int x = 0; x <= mapSizeInMeters; x++)
                {
                    Vector3 currentWorldPosition = new Vector3(x, 0f, y);

                    switch (mapModifier.modifierType)
                    {
                        case MapModifier.MapModifierType.Smoothing:
                            heightMap[x, y] = Mathf.Lerp(heightMap[x, y], StaticUtils.Averaging(currentWorldPosition, pattern, GetHeight), mapModifier.intensity);
                            break;

                        case MapModifier.MapModifierType.IDWSmoothing:
                            heightMap[x, y] = Mathf.Lerp(heightMap[x, y], StaticUtils.InverseDistanceWeightedInterpolation(GetHeight, currentWorldPosition, pattern), mapModifier.intensity);
                            break;

                        case MapModifier.MapModifierType.MoreWater:
                            heightMap[x, y] = Mathf.Lerp(heightMap[x, y], 0f, mapModifier.intensity);
                            break;

                        case MapModifier.MapModifierType.MoreHills:
                            heightMap[x, y] = Mathf.Lerp(heightMap[x, y], 1f, mapModifier.intensity);
                            break;
                    }
                }
            }
        }
    }

    private void ActivateChunks(bool active)
    {
        foreach (Chunk chunk in chunks) chunk.gameObject.SetActive(active);
    }

    public bool IsCoordOnMap(Vector3 coord) => coord.x > 0 && coord.x <= mapSizeInMeters && coord.z > 0 && coord.z <= mapSizeInMeters;
    public bool IsCoordOnMap(Vector2Int coord) => coord.x > 0 && coord.x <= mapSizeInMeters && coord.y > 0 && coord.y <= mapSizeInMeters;

    public bool IsWater(float height) => height <= mapSettings.waterLevel;
    public bool IsCliff(float height) => height >= mapSettings.cliffLevel;
    public bool IsGround(float height) => !IsWater(height) && !IsCliff(height);

    public HeightLevel GetHeightLevel(float height)
    {
        if (IsWater(height)) return HeightLevel.Water;
        if (IsCliff(height)) return HeightLevel.Cliff;
        return HeightLevel.Ground;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (Selection.activeGameObject != gameObject) return;

        //WorldBounds debug
        Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
        Gizmos.DrawCube(worldBounds.center, worldBounds.size);
        Gizmos.color = new Color(1f, 0f, 0f, 1f);
        Gizmos.DrawWireCube(worldBounds.center, worldBounds.size);

        //draw pattern
        //Gizmos.color = new Color(1f, 1f, 1f, 0.6f);
        //foreach (Vector3 pos in StaticUtils.GetPatternCirlce(1f, 10f).array) Gizmos.DrawWireCube(pos, Vector3.one * 0.5f);

        //visualize height map
        if (heightMap != null)
        {
            for (int y = 0; y <= mapSizeInMeters; y++)
            {
                for (int x = 0; x <= mapSizeInMeters; x++)
                {
                    Vector3 currentWorldPosition = new Vector3(x, heightMap[x, y] * 20f, y);

                    HeightLevel level = GetHeightLevel(heightMap[x, y]);

                    if (level == HeightLevel.Water) Gizmos.color = Color.blue;
                    if (level == HeightLevel.Cliff) Gizmos.color = Color.gray;
                    if (level == HeightLevel.Ground) Gizmos.color = Color.green;

                    Gizmos.DrawWireCube(currentWorldPosition, Vector3.one * 0.5f);
                }
            }
        }
    }
#endif
}
