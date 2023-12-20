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
                tileMap.SetTile(new Vector3Int(x,y), ruleTiles[0]);
            }
        }

        tileMap.SetTile(new Vector3Int(2, 2), null);
        tileMap.SetTile(new Vector3Int(6, 6), null);
        tileMap.SetTile(new Vector3Int(6, 7), null);
    }
}
