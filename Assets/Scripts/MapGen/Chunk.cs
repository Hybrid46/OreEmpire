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
        densityMap = new Point[MapGen.chunkSize + 1, MapGen.chunkHeight, MapGen.chunkSize + 1];
        int groundLevel = 4;

        for (int z = 0; z <= MapGen.chunkSize; z++)
        {
            for (int x = 0; x <= MapGen.chunkSize; x++)
            {
                float height = heightMap[x, z];
                float surfaceNoise = GetSurfacePerlinNoise(transform.position + new Vector3(x, 0f, z));
                MapGen.HeightLevel level = GameManager.instance.mapGen.GetHeightLevel(height);

                for (int y = 0; y < MapGen.chunkHeight; y++)
                {
                    Vector3 localPosition = new Vector3(x, y, z);

                    if (y == 0)                     //bottom
                    {
                        densityMap[x, y, z] = new Point(localPosition, 0.5f + surfaceNoise, Color.white);
                        continue;
                    }

                    if (y == MapGen.chunkHeight - 1) //top
                    {
                        densityMap[x, y, z] = new Point(localPosition, 0f, Color.white);
                        continue;
                    }

                    if (level == MapGen.HeightLevel.Water)
                    {
                        densityMap[x, y, z] = new Point(localPosition, 0f, Color.white);
                    }

                    if (level == MapGen.HeightLevel.Cliff)
                    {
                        densityMap[x, y, z] = new Point(localPosition, 0.5f + surfaceNoise, Color.white);
                    }

                    if (level == MapGen.HeightLevel.Ground)
                    {
                        if (y <= groundLevel)
                        {
                            densityMap[x, y, z] = new Point(localPosition, 0.5f + surfaceNoise, Color.white);
                        }
                        else
                        {
                            densityMap[x, y, z] = new Point(localPosition, 0f, Color.white);
                        }
                    }                    
                }
            }
        }

        float GetSurfacePerlinNoise(Vector3 worldPosition)
        {
            float scale = GameManager.instance.mapGen.mapSettings.surfaceNoiseSettings.scale;
            float xCoord = worldPosition.x / GameManager.instance.mapGen.mapSettings.mapSize * scale + GameManager.instance.mapGen.mapSettings.surfaceNoiseSettings.seedX;
            float zCoord = worldPosition.z / GameManager.instance.mapGen.mapSettings.mapSize * scale + GameManager.instance.mapGen.mapSettings.surfaceNoiseSettings.seedY;

            return Mathf.Clamp01(Mathf.PerlinNoise(xCoord, zCoord)) * GameManager.instance.mapGen.mapSettings.surfaceRoughness;
        }
    }

    [BurstCompile]
    private void GenerateMesh()
    {
        m_meshFilter.sharedMesh = new Mesh();
        MarchingCubes marchingCubes = new MarchingCubes(densityMap, 0.5f);
        marchingCubes.CreateMeshData(m_meshFilter.sharedMesh, densityMap);
    }

    [BurstCompile]
    public float GetHeight(Vector3 position) => heightMap[(int)position.x, (int)position.z];

    [BurstCompile]
    public Bounds GetBounds()
    {
        Vector3 size = new Vector3(MapGen.chunkSize, MapGen.chunkHeight, MapGen.chunkSize);
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
                    for (int y = 0; y < MapGen.chunkHeight; y++)
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