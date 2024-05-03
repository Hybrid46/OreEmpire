using System;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    private float[,] heightMap = new float[MapGen.chunkSize, MapGen.chunkSize];
    private Point[,,] densityMap = new Point[MapGen.chunkSize, MapGen.chunkHeight, MapGen.chunkSize];
    private MeshFilter m_meshFilter;

    void Start()
    {
        m_meshFilter = GetComponent<MeshFilter>();
        GenerateHeightMap();
        HeightMapToDensityMap();
        GenerateMesh();
    }

    private void GenerateHeightMap()
    {

    }

    public void HeightMapToDensityMap()
    {

    }

    private void GenerateMesh()
    {
        MarchingCubes marchingCubes = new MarchingCubes(densityMap, 0.5f);
        marchingCubes.CreateMeshData(m_meshFilter.sharedMesh, densityMap);
    }

    public float GetHeight(Vector3 position) => heightMap[(int)position.x, (int)position.z];

    public void SaveChunkToDisk()
    {
        throw new NotImplementedException("Chunk not saved!");
    }

    public void LoadChunkFromDisk()
    {
        throw new NotImplementedException("Chunk not loaded!");
    }

    public void DestroyChunk()
    {
#if UNITY_EDITOR
        DestroyImmediate(m_meshFilter);
#else
        Destroy(m_meshFilter);
#endif
    }

    private void OnDestroy()
    {
        DestroyChunk();
    }

    private void OnDrawGizmos()
    {
        if (m_meshFilter == null) return;

        Bounds meshBounds = m_meshFilter.sharedMesh.bounds;
        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(meshBounds.center, meshBounds.size);
        Gizmos.DrawWireCube(meshBounds.center, meshBounds.size);
    }
}
