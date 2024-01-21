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

            Vector3Int roundedPosition = StaticUtils.MouseToGridPosition();
            MapGenerator.instance.transportManager.AddTransporter(roundedPosition, Vector3.right, 2.0f);
        }

        //add ore test
        if (Input.GetKeyUp(KeyCode.O))
        {
            Vector3Int roundedPosition = StaticUtils.MouseToGridPosition();

            MapGenerator.instance.transportManager.transportBelts[roundedPosition.x, roundedPosition.z].AddOre(MapGenerator.OreType.Copper, 0);
        }
    }

    private void Build(GameObject building, int sizeX = 1, int sizeZ = 1)
    {
        Vector3Int roundedPosition = StaticUtils.MouseToGridPosition();

        if (IsBuildeable(roundedPosition.x, roundedPosition.z, sizeX, sizeZ))
        {
            Instantiate(building, roundedPosition, transform.rotation);
            Vector3Int[] gridsBuilded = StaticUtils.GetPattern(sizeX, sizeZ);

            foreach (Vector3Int gridBuilded in gridsBuilded)
            {
                MapGenerator.instance.grid[gridBuilded.x, gridBuilded.z].built = true;
            }
        }
    }

    private bool IsBuildeable(int PosX, int PosZ, int sizeX, int sizeZ)
    {
        //TODO resource check

        for (int z = 0; z < sizeZ; z++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                //Is in map?
                if (MapGenerator.instance.IsOnMap(PosX + x, PosZ + z) == false) return false;
                //Is building on ground?
                if (MapGenerator.instance.grid[PosX + x, PosZ + z].heightType != MapGenerator.TileHeightType.Ground) return false;
                //Is it already built there?
                if (MapGenerator.instance.grid[PosX + x, PosZ + z].built == true) return false;
            }
        }

        return true;
    }
}
