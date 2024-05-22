using System;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

public class MapGen : MonoBehaviour
{
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

        //TODO: height gen here then copy to chunks
        //TODO: map modifiers like smoothing can be used here -> StaticUtils.GetPatternCirlce()

        mapSizeInMeters = mapSettings.mapSize * chunkSize;
        heightMap = new float[mapSizeInMeters + 1, mapSizeInMeters + 1];

        GenerateHeightMap();

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
            sumHeight += Mathf.Clamp(GetHeight(currentWorldPosition, noiseSettings), 0.1f, 0.9f);
        }

        return sumHeight / mapSettings.noiseSettings.Count;
    }

    [BurstCompile]
    private float GetHeight(Vector3 worldPosition, NoiseSettings noiseSettings)
    {
        float xCoord = worldPosition.x / mapSettings.mapSize * noiseSettings.scale + noiseSettings.seedX;
        float zCoord = worldPosition.z / mapSettings.mapSize * noiseSettings.scale + noiseSettings.seedY;

        return Mathf.PerlinNoise(xCoord, zCoord) * noiseSettings.maxHeight + noiseSettings.minHeight;
        //For Job -> return noise.cnoise(new float2(xCoord, zCoord)) * noiseSettings.maxHeight + noiseSettings.minHeight;
    }

    private void ActivateChunks(bool active = true)
    {
        foreach (Chunk chunk in chunks) chunk.gameObject.SetActive(active);
    }

    private void OnDrawGizmos()
    {
        //WorldBounds debug
        Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
        Gizmos.DrawCube(worldBounds.center, worldBounds.size);
        Gizmos.color = new Color(1f, 0f, 0f, 1f);
        Gizmos.DrawWireCube(worldBounds.center, worldBounds.size);

    }
}
