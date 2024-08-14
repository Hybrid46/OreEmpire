using System;
using System.Collections.Generic;
using Unity.Burst;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class MapGen : MonoBehaviour
{
    public const int chunkSize = 16;

    public int mapSizeInUnits;

    public MapSettings mapSettings;

    public float[,] heightMap;
    public HashSet<Chunk> chunks;
    public Material terrainMaterial;
    public ComputeShader marchingCompute;
    public MarchingCubesComputeHandler marchingCubesComputeHandler;

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

        mapSizeInUnits = mapSettings.mapSize * chunkSize;
        heightMap = new float[mapSizeInUnits + 1, mapSizeInUnits + 1];

        GenerateHeightMap();
        NormalizeHeightMap();
        ApplyMapModifiers();

        Debug.Log($"Heigth map[{(mapSizeInUnits + 1) * (mapSizeInUnits + 1)}] generated in: {(DateTime.Now - exectime).Milliseconds} ms");
        exectime = DateTime.Now;

        chunks = new HashSet<Chunk>(mapSettings.mapSize * mapSettings.mapSize);
        marchingCubesComputeHandler = new MarchingCubesComputeHandler(marchingCompute);
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
        for (int y = 0; y <= mapSizeInUnits; y++)
        {
            for (int x = 0; x <= mapSizeInUnits; x++)
            {
                float height = heightMap[x, y];
                if (height >= max) max = height;
                if (height < min) min = height;
            }
        }

        //normalize to -> 0 - 1
        for (int y = 0; y <= mapSizeInUnits; y++)
        {
            for (int x = 0; x <= mapSizeInUnits; x++)
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
        heightMap = new float[mapSizeInUnits + 1, mapSizeInUnits + 1];

        for (int y = 0; y <= mapSizeInUnits; y++)
        {
            for (int x = 0; x <= mapSizeInUnits; x++)
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

            for (int y = 0; y <= mapSizeInUnits; y++)
            {
                for (int x = 0; x <= mapSizeInUnits; x++)
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

                        case MapModifier.MapModifierType.MicroSmoothing:
                            throw new NotImplementedException();
                            //break;

                    }
                }
            }
        }
    }

    private void ActivateChunks(bool active)
    {
        foreach (Chunk chunk in chunks) chunk.gameObject.SetActive(active);
    }

    public bool IsCoordOnMap(Vector3 coord) => coord.x > 0 && coord.x <= mapSizeInUnits && coord.z > 0 && coord.z <= mapSizeInUnits;
    public bool IsCoordOnMap(Vector2Int coord) => coord.x > 0 && coord.x <= mapSizeInUnits && coord.y > 0 && coord.y <= mapSizeInUnits;

    private void OnDestroy()
    {
        marchingCubesComputeHandler.Destroy();
        marchingCubesComputeHandler = null;
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
            for (int y = 0; y <= mapSizeInUnits; y++)
            {
                for (int x = 0; x <= mapSizeInUnits; x++)
                {
                    Vector3 currentWorldPosition = new Vector3(x, heightMap[x, y] * 20f, y);

                    Gizmos.color = Color.white * heightMap[x, y] / chunkSize;

                    Gizmos.DrawWireCube(currentWorldPosition, Vector3.one * 0.5f);
                }
            }
        }
    }
#endif
}
