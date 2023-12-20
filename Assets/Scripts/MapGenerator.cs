using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    public int mapSize = 256;
    public Tilemap tileMap;
    public List<RuleTile> ruleTiles;

    void Start()
    {
        Generate(mapSize);
    }

    private void Generate(int tiles)
    {
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

    private float GetPerlin(Vector3Int position, float scale, Vector2 offset)
    {
        float xCoord = (float)position.x / mapSize * scale + offset.x;
        float zCoord = (float)position.y / mapSize * scale + offset.y;

        return Mathf.PerlinNoise(xCoord, zCoord);
    }
}