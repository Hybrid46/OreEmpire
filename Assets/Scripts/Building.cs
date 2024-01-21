using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public List<GameObject> buildings;

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.C)) Build(buildings[0], 3, 3);
        if (Input.GetKeyUp(KeyCode.M)) Build(buildings[1], 2, 2);

        if (Input.GetKeyUp(KeyCode.T))
        {
            Build(buildings[2]);

            Vector3 mousePos = StaticUtils.MouseToWorldPosition();
            mousePos.z = 0;
            Vector3Int roundedPosition = Vector3Int.RoundToInt(mousePos);
            MapGenerator.instance.transportManager.AddTransporter(new Vector2Int(roundedPosition.x, roundedPosition.y), Vector2Int.right, 2.0f);
        }

        //add ore test
        if (Input.GetKeyUp(KeyCode.O))
        {
            Vector3 mousePos = StaticUtils.MouseToWorldPosition();
            mousePos.z = 0;
            Vector3Int roundedPosition = Vector3Int.RoundToInt(mousePos);

            MapGenerator.instance.transportManager.transportBelts[roundedPosition.x, roundedPosition.y].AddOre(MapGenerator.OreType.Copper, 0);
        }
    }

    private void Build(GameObject building, int sizeX = 1, int sizeY = 1)
    {
        Vector3 mousePos = StaticUtils.MouseToWorldPosition();
        mousePos.z = 0;
        Vector3Int roundedPosition = Vector3Int.RoundToInt(mousePos);

        if (IsBuildeable(roundedPosition.x, roundedPosition.y, sizeX, sizeY))
        {
            Instantiate(building, roundedPosition, Quaternion.identity);
            Vector2Int[] gridsBuilded = StaticUtils.GetPattern(sizeX, sizeY);

            foreach (Vector2Int gridBuilded in gridsBuilded)
            {
                MapGenerator.instance.grid[gridBuilded.x, gridBuilded.y].built = true;
            }
        }
    }

    private bool IsBuildeable(int PosX, int PosY, int sizeX, int sizeY)
    {
        //TODO resource check

        for (int y = 0; y < sizeY; y++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                //Is in map?
                if (MapGenerator.instance.IsOnMap(PosX + x, PosY + y) == false) return false;
                //Is building on ground?
                if (MapGenerator.instance.grid[PosX + x, PosY + y].heightType != MapGenerator.TileHeightType.Ground) return false;
                //Is it already built there?
                if (MapGenerator.instance.grid[PosX + x, PosY + y].built == true) return false;
            }
        }

        return true;
    }
}
