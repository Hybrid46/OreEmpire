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

    private void Start()
    {
        DateTime exectime = DateTime.Now;

        mapSizeInMeters = mapSettings.mapSize * chunkSize;
        heightMap = new float[mapSizeInMeters + 1, mapSizeInMeters + 1];

        GenerateHeightMap();
        ApplyMapModifiers();

        Debug.Log($"Heigth map[{(mapSizeInMeters + 1) * (mapSizeInMeters + 1)}] generated in: {(DateTime.Now - exectime).Milliseconds} ms");
        exectime = DateTime.Now;

        chunks = new HashSet<Chunk>(mapSettings.mapSize * mapSettings.mapSize);
        GenerateChunks();

        Debug.Log($"Chunks[{chunks.Count}] Initialized in: {(DateTime.Now - exectime).Milliseconds} ms");
        exectime = DateTime.Now;

        ActivateChunks();

        Debug.Log($"Chunks[{chunks.Count}] activated: {(DateTime.Now - exectime).Milliseconds} ms");
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
        float xCoord = worldPosition.x / mapSettings.mapSize * noiseSettings.scale + noiseSettings.seedX;
        float zCoord = worldPosition.z / mapSettings.mapSize * noiseSettings.scale + noiseSettings.seedY;

        return Mathf.Clamp01(Mathf.PerlinNoise(xCoord, zCoord) * noiseSettings.maxHeight + noiseSettings.minHeight);
        //For Job -> return noise.cnoise(new float2(xCoord, zCoord)) * noiseSettings.maxHeight + noiseSettings.minHeight;
    }

    [BurstCompile]
    private void ApplyMapModifiers()
    {
        foreach (MapModifier mapModifier in mapSettings.modifiers)
        {
            Vector3[] pattern = StaticUtils.GetPatternCirlce(1f, mapModifier.range).array;

            for (int y = 0; y <= mapSizeInMeters; y++)
            {
                for (int x = 0; x <= mapSizeInMeters; x++)
                {
                    Vector3 currentWorldPosition = new Vector3(x, 0f, y);

                    if (mapModifier.modifierType == MapModifier.MapModifierType.Smoothing)
                    {
                        heightMap[x, y] = Mathf.Lerp(heightMap[x, y], Smoothing(currentWorldPosition, pattern), mapModifier.intensity);
                    }

                    if (mapModifier.modifierType == MapModifier.MapModifierType.IDWSmoothing)
                    {
                        heightMap[x, y] = Mathf.Lerp(heightMap[x, y], GetHeightMapIDW(currentWorldPosition, pattern), mapModifier.intensity);
                    }

                    if (mapModifier.modifierType == MapModifier.MapModifierType.MoreWater)
                    {
                        heightMap[x, y] = Mathf.Lerp(heightMap[x, y], 0f, mapModifier.intensity);
                    }

                    if (mapModifier.modifierType == MapModifier.MapModifierType.MoreHills)
                    {
                        heightMap[x, y] = Mathf.Lerp(heightMap[x, y], 1f, mapModifier.intensity);
                    }
                }
            }
        }
    }

    [BurstCompile]
    private float Smoothing(Vector3 position, Vector3[] pattern)
    {
        float heightValue = 0f;
        float sampleCount = 0f;

        for (int p = 0; p < pattern.Length; p++)
        {
            Vector3 currentPos = position + pattern[p];

            if (!IsCoordOnMap(currentPos)) continue;

            heightValue += heightMap[(int)currentPos.x, (int)currentPos.z];
            sampleCount++;
        }

        return heightValue / sampleCount;
    }

    [BurstCompile]
    private float GetHeightMapIDW(Vector3 position, Vector3[] pattern)
    {
        float heightValue = 0f;
        float inverseDistance = 0;

        for (int p = 0; p < pattern.Length; p++)
        {
            Vector3 currentPos = position + pattern[p];
            float distance = Vector3.Distance(currentPos, position);

            //check map bounds
            //not generated yet! if (!worldBounds.Contains(currentPos)) continue;
            if (!IsCoordOnMap(currentPos)) continue;

            //if (distance < 1f) distance = 1f; //center -> div by zero

            distance = distance / distance;
            heightValue += heightMap[(int)currentPos.x, (int)currentPos.z] / distance;
            inverseDistance += 1.0f / distance;
        }

        return heightValue / inverseDistance;
    }

    private void ActivateChunks(bool active = true)
    {
        foreach (Chunk chunk in chunks) chunk.gameObject.SetActive(active);
    }

    public bool IsCoordOnMap(Vector3 coord) => coord.x > 0 && coord.x <= mapSizeInMeters && coord.z > 0 && coord.z <= mapSizeInMeters;

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
