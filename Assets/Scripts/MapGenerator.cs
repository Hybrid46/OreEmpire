using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    public Tilemap tileMap;
    public List<RuleTile> ruleTiles;

    void Start()
    {
        Generate(16);
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
    }
}
