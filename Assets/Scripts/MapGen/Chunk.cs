using System;
using Unity.Burst;
using UnityEngine;

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
        m_meshRenderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        gameObject.SetActive(false);
        GenerateHeightMap();
        //HeightMapToTexture(); //testing only
        //m_meshRenderer.sharedMaterial.SetTexture("_BaseMap", heightTexture);
        GenerateDensityMap();
        GenerateMesh();

    }

    [BurstCompile]
    private void GenerateHeightMap()
    {
        heightMap = new float[MapGen.chunkSize + 1, MapGen.chunkSize + 1];
        Vector3 transformWorldPosition = transform.position;

        for (int y = 0; y <= MapGen.chunkSize; y++)
        {
            for (int x = 0; x <= MapGen.chunkSize; x++)
            {
                Vector3 currentWorldPosition = transformWorldPosition + new Vector3(x, 0f, y);

                heightMap[x, y] = GetHeightAverage(currentWorldPosition);
            }
        }
    }

    [BurstCompile]
    private float GetHeightAverage(Vector3 currentWorldPosition)
    {
        float sumHeight = 0f;

        foreach (NoiseSettings noiseSettings in GameManager.instance.mapGen.mapSettings.noiseSettings)
        {
            sumHeight += Mathf.Clamp(GetHeight(currentWorldPosition, noiseSettings), 0.1f, 0.9f);
        }

        return sumHeight / GameManager.instance.mapGen.mapSettings.noiseSettings.Count;
    }

    [BurstCompile]
    private float GetHeight(Vector3 worldPosition, NoiseSettings noiseSettings)
    {
        float xCoord = worldPosition.x / GameManager.instance.mapGen.mapSettings.mapSize * noiseSettings.scale + noiseSettings.seedX;
        float zCoord = worldPosition.z / GameManager.instance.mapGen.mapSettings.mapSize * noiseSettings.scale + noiseSettings.seedY;

        return Mathf.PerlinNoise(xCoord, zCoord) * noiseSettings.maxHeight + noiseSettings.minHeight;
        //For Job -> return noise.cnoise(new float2(xCoord, zCoord)) * noiseSettings.maxHeight + noiseSettings.minHeight;
    }

    [BurstCompile]
    private void GenerateDensityMap()
    {
        densityMap = new Point[MapGen.chunkSize + 1, MapGen.chunkHeight, MapGen.chunkSize + 1];

        for (int z = 0; z <= MapGen.chunkSize; z++)
        {
            for (int x = 0; x <= MapGen.chunkSize; x++)
            {
                for (int y = 0; y < MapGen.chunkHeight; y++)
                {
                    Vector3 localPosition = new Vector3(x, y, z);

                    float height = heightMap[x, z];

                    densityMap[x, y, z] = new Point(localPosition, y > height * 10 ? 0f : 1f, Color.white); //TODO height and color calc
                }
            }
        }
    }

    [BurstCompile]
    private void GenerateMesh()
    {
        m_meshFilter.sharedMesh = new Mesh();
        MarchingCubes marchingCubes = new MarchingCubes(densityMap, 0.5f);
        marchingCubes.CreateMeshData(m_meshFilter.sharedMesh, densityMap);
    }

    public float GetHeight(Vector3 position) => heightMap[(int)position.x, (int)position.z];

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

    private void OnDrawGizmos()
    {
        if (m_meshFilter == null) return;

        Bounds meshBounds = m_meshFilter.sharedMesh.bounds;
        Gizmos.color = new Color(0f, 1f, 1f, 0.4f);
        Gizmos.DrawCube(transform.position + meshBounds.center, meshBounds.size * 0.99f);
        Gizmos.DrawWireCube(transform.position + meshBounds.center, meshBounds.size * 0.99f);
    }
}
