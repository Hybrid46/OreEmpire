using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    public int mapSize = 256;
    public Tilemap tileMap;
    public List<RuleTile> ruleTiles;

    private float[,] heightMap;
    [SerializeField] private Texture2D mapTexture;

    void Start()
    {
        Generate(mapSize);
        GenerateMapTexture();
    }

    private void Generate(int tiles)
    {
        heightMap = new float[mapSize, mapSize];

        //fill heightmap with diamond square algo first
        DiamondSquareGenerator dsqg = new DiamondSquareGenerator(mapSize, 0.9f, 0.5f);
        heightMap = dsqg.GenerateTerrain();

        //fill heightmap with perlin sampling to make more randomness


        for (int y = 0; y < tiles; y++)
        {
            for (int x = 0; x < tiles; x++)
            {
                Vector3Int position = new Vector3Int(x, y);
                float noiseValue = GetPerlin(position, 20f, Vector2.zero);

                if (noiseValue > 0.5f) tileMap.SetTile(position, ruleTiles[0]);
            }
        }

        tileMap.SetTile(new Vector3Int(2, 2), null);
        tileMap.SetTile(new Vector3Int(6, 6), null);
        tileMap.SetTile(new Vector3Int(6, 7), null);
    }

    private void GenerateMapTexture()
    {
        mapTexture = new Texture2D(mapSize, mapSize, TextureFormat.RGB24, false);

        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                float height = heightMap[x, y];
                mapTexture.SetPixel(x, y, new Color(height, height, height));
            }
        }

        mapTexture.Apply();
    }

    private float GetPerlin(Vector3Int position, float scale, Vector2 offset)
    {
        float xCoord = (float)position.x / mapSize * scale + offset.x;
        float zCoord = (float)position.y / mapSize * scale + offset.y;

        return Mathf.PerlinNoise(xCoord, zCoord);
    }
}