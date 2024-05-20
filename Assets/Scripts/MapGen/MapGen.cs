using System;
using System.Collections.Generic;
using UnityEngine;

public class MapGen : MonoBehaviour
{
    public const int chunkSize = 16;
    public const int chunkHeight = 10;

    public int mapSizeInMeters;

    public MapSettings mapSettings;

    public HashSet<Chunk> chunks;

    private void Start()
    {
        DateTime exectime = DateTime.Now;

        mapSizeInMeters = mapSettings.mapSize * chunkSize;
        chunks = new HashSet<Chunk>(mapSettings.mapSize * mapSettings.mapSize);
        GenerateChunks();

        Debug.Log($"Chunks Initialized in: {(DateTime.Now - exectime).Milliseconds} ms");

        exectime = DateTime.Now;

        Debug.Log($"chunks {chunks.Count}");
    }

    private void Update()
    {
        ActivateChunks();
    }

    public void GenerateChunks()
    {
        for (int y = 0; y < mapSizeInMeters; y += chunkSize)
        {
            for (int x = 0; x < mapSizeInMeters; x += chunkSize)
            {
                Vector3 worldPosition = new Vector3(x, 0f, y);

                GameObject chunk = new GameObject();
                chunk.transform.position = worldPosition;
                chunk.transform.SetParent(transform, true);
                chunks.Add(chunk.AddComponent<Chunk>());
            }
        }
    }

    private void ActivateChunks()
    {
        foreach (Chunk chunk in chunks) chunk.gameObject.SetActive(true);
    }
}