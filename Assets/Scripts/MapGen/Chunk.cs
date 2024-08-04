using System;
using Unity.Burst;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
//[RequireComponent(typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    private float[,] heightMap;
    private Point[,,] densityMap;
    private MeshFilter m_meshFilter;
    private MeshRenderer m_meshRenderer;
    //[SerializeField] private Texture2D heightTexture; //testing only

    void Start()
    {
        m_meshFilter = GetComponent<MeshFilter>();
        m_meshRenderer = GetComponent<MeshRenderer>();
        m_meshRenderer.sharedMaterial = GameManager.instance.mapGen.terrainMaterial;
        GetHeightMap();
        //HeightMapToTexture(); //testing only
        //m_meshRenderer.sharedMaterial.SetTexture("_BaseMap", heightTexture);
        GenerateDensityMap();
        GenerateMesh();
        //CleanUp();
    }

    [BurstCompile]
    private void GetHeightMap()
    {
        heightMap = new float[MapGen.chunkSize + 1, MapGen.chunkSize + 1];
        StaticUtils.Copy2DArrayFast(GameManager.instance.mapGen.heightMap, heightMap, (int)transform.position.x, (int)transform.position.z, MapGen.chunkSize + 1, MapGen.chunkSize + 1);
    }

    [BurstCompile]
    private void GenerateDensityMap()
    {
        densityMap = new Point[MapGen.chunkSize + 1, MapGen.chunkSize + 1, MapGen.chunkSize + 1];
        int groundLevel = 8;

        for (int z = 0; z < MapGen.chunkSize + 1; z++)
        {
            for (int x = 0; x < MapGen.chunkSize + 1; x++)
            {
                float height = heightMap[x, z];
                MapGen.HeightLevel level = GameManager.instance.mapGen.GetHeightLevel(height);

                for (int y = 0; y < MapGen.chunkSize + 1; y++)
                {
                    Vector3 localPosition = new Vector3(x, y, z);
                    float surfaceNoise = GetSurfacePerlinNoise(transform.position + localPosition);

                    if (y == 0)                     //bottom
                    {
                        densityMap[x, y, z] = new Point(localPosition, 0.5f + surfaceNoise, Color.white);
                        continue;
                    }

                    if (y == MapGen.chunkSize) //top
                    {
                        densityMap[x, y, z] = new Point(localPosition, surfaceNoise, Color.white);
                        continue;
                    }

                    if (level == MapGen.HeightLevel.Water)
                    {
                        float density = 0f;

                        if (y < groundLevel) density = surfaceNoise;

                        densityMap[x, y, z] = new Point(localPosition, density, Color.blue);
                    }

                    if (level == MapGen.HeightLevel.Cliff)
                    {
                        float density = 0.5f + surfaceNoise;

                        densityMap[x, y, z] = new Point(localPosition, density, Color.gray);
                    }

                    if (level == MapGen.HeightLevel.Ground)
                    {
                        float density = surfaceNoise;

                        if (y <= groundLevel) density += 0.5f;

                        densityMap[x, y, z] = new Point(localPosition, density, Color.green);
                    }
                }
            }
        }

        float GetSurfacePerlinNoise(Vector3 worldPosition)
        {
            float scale = GameManager.instance.mapGen.mapSettings.surfaceNoiseSettings.scale;
            float xCoord = worldPosition.x / GameManager.instance.mapGen.mapSettings.mapSize * scale + GameManager.instance.mapGen.mapSettings.surfaceNoiseSettings.seedX + worldPosition.y;
            float zCoord = worldPosition.z / GameManager.instance.mapGen.mapSettings.mapSize * scale + GameManager.instance.mapGen.mapSettings.surfaceNoiseSettings.seedY + worldPosition.y;

            return Mathf.Clamp01(Mathf.PerlinNoise(xCoord, zCoord)) * GameManager.instance.mapGen.mapSettings.surfaceRoughness;
        }
    }

    [BurstCompile]
    private void GenerateMesh()
    {
        //CPU chunk generation
        //m_meshFilter.sharedMesh = new Mesh();
        //MarchingCubes marchingCubes = new MarchingCubes(densityMap, 0.5f);
        //marchingCubes.CreateMeshData(m_meshFilter.sharedMesh, densityMap);
        //return;

        ////MeshUtilities.FlattenTriangleNormals(m_meshFilter.sharedMesh);
        ////MeshUtilities.ConvertTrianglesToQuads(m_meshFilter.sharedMesh);

        //GPU chunk generation
        Vector4[] pointData = new Vector4[densityMap.GetLength(0) * densityMap.GetLength(1) * densityMap.GetLength(2)];

        for (int z = 0; z < densityMap.GetLength(2); z++)
        {
            for (int y = 0; y < densityMap.GetLength(1); y++)
            {
                for (int x = 0; x < densityMap.GetLength(0); x++)
                {
                    int index = StaticUtils.Array3DTo1D(x, y, z, densityMap.GetLength(0), densityMap.GetLength(1));
                    pointData[index] = new Vector4(x, y, z, densityMap[x, y, z].density);
                }
            }
        }

        MarchingCubesComputeHandler mcch = GameManager.instance.mapGen.marchingCubesComputeHandler;
        m_meshFilter.sharedMesh = mcch.ComputeMesh(pointData);
    }

    [BurstCompile]
    public float GetHeight(Vector3 position) => heightMap[(int)position.x, (int)position.z];

    [BurstCompile]
    public Bounds GetBounds()
    {
        Vector3 size = new Vector3(MapGen.chunkSize, MapGen.chunkSize, MapGen.chunkSize);
        return new Bounds(transform.position + size * 0.5f, size);
    }

    /*
    //testing only
    [BurstCompile]
    private void HeightMapToTexture()
    {
        heightTexture = new Texture2D(MapGen.chunkSize, MapGen.chunkSize, TextureFormat.RGB24, false);
        heightTexture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < MapGen.chunkSize; y++)
        {
            for (int x = 0; x < MapGen.chunkSize; x++)
            {
                heightTexture.SetPixel(x, y, Color.white * heightMap[x, y]);
            }
        }

        heightTexture.Apply();
    }
    */

    public void CleanUp()
    {
        densityMap = null;
        heightMap = null;
    }

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
        DestroyImmediate(gameObject);
#else
        Destroy(m_meshFilter);
        Destroy(gameObject);
#endif
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (m_meshFilter == null) return;
        if (Selection.activeGameObject == null || Selection.activeGameObject != gameObject) return;

        //ChunkBounds debug
        Bounds meshBounds = m_meshFilter.sharedMesh.bounds;
        Gizmos.color = new Color(0f, 1f, 1f, 0.4f);
        Gizmos.DrawCube(transform.position + meshBounds.center, meshBounds.size * 0.99f);
        Gizmos.color = new Color(0f, 1f, 1f, 1f);
        Gizmos.DrawWireCube(transform.position + meshBounds.center, meshBounds.size * 0.99f);

        //Density debug
        //Density debug
        if (densityMap != null)
        {
            for (int z = 0; z <= MapGen.chunkSize; z++)
            {
                for (int x = 0; x <= MapGen.chunkSize; x++)
                {
                    for (int y = 0; y < MapGen.chunkSize; y++)
                    {
                        Vector3 localPosition = new Vector3(x, y, z);
                        float height = heightMap[x, z];
                        float density = densityMap[x, y, z].density;

                        //if (density > 0.5f)
                        //{
                        //    Gizmos.color = new Color(0f, 0f, 2f, 1f);
                        //}
                        //else
                        //{
                        //    Gizmos.color = new Color(2f, 0f, 0f, 1f);
                        //}

                        Gizmos.color = new Color(density, density, density);

                        Gizmos.DrawCube(transform.position + localPosition, Vector3.one * density);
                        Gizmos.DrawWireCube(transform.position + localPosition, Vector3.one * density);
                    }
                }
            }
        }
    }
#endif
}