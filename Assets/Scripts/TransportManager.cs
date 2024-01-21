using System;
using System.Collections.Generic;
using UnityEngine;
using OreType = MapGenerator.OreType;

//This script will manage the belt transport system
public class TransportManager
{
    public Transporter[,] transportBelts;
    private HashSet<Vector3Int> transporterPositions; //contains all transporter coordinates for fast iteration and lookup on every transporter

    public struct Transporter
    {
        public float speed;
        public Vector3[] directions;
        public OreType[] ores;
        public float[] interpolations;

        public Transporter(Vector3 outDirection, float speed)
        {
            this.speed = speed;
            this.directions = new Vector3[2] { Vector3.zero, outDirection };
            this.ores = new OreType[2] { OreType.None, OreType.None };
            this.interpolations = new float[2] { 0.0f, 0.0f };
        }

        public void Set(Vector3 outDirection, float speed)
        {
            this.directions = new Vector3[2] { Vector3.zero, outDirection };
            this.speed = speed;
        }

        public void AddOre(OreType oreType, int index)
        {
            if (!IsLoadable(index))
            {
                Debug.Log("Belt loading error!");
                return;
            }

            ores[index] = oreType;
            interpolations[index] = 0.0f;
        }

        public void RemoveOre(int index)
        {
            ores[index] = OreType.None;
            interpolations[index] = 0.0f;
        }

        public bool IsLoadable(int index) => ores[index] == OreType.None;
    }

    public TransportManager() => Initialize();

    private void Initialize()
    {
        int mapSize = MapGenerator.instance.mapSize;

        transportBelts = new Transporter[mapSize, mapSize];
        transporterPositions = new HashSet<Vector3Int>(mapSize * mapSize);

        for (int z = 0; z < mapSize; z++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                transportBelts[x, z] = new Transporter(Vector3.zero, 0.0f);
            }
        }
    }

    public void AddTransporter(Vector3Int position, Vector3 direction, float speed)
    {
        transporterPositions.Add(position);
        transportBelts[position.x, position.z].Set(direction, speed);

        Debug.Log($"Transporter added! pos -> {position} dir -> {direction} speed -> {speed}");
    }

    public void RemoveTransporter(Vector3Int position)
    {
        if (!transporterPositions.Contains(position)) return; //TODO should check where the call comes from to prevent an unnecessary function call

        transporterPositions.Remove(position);
        transportBelts[position.x, position.z].Set(Vector3.zero, 0.0f);
    }

    public void RotateTransporter(Vector3Int position, Vector3 direction) => throw new NotImplementedException();

    public void UpdateTransporters()
    {
        //Ore[0] to center from incoming direction
        //Ore[1] to out direction from center

        foreach (Vector3Int transporterPosition in transporterPositions)
        {
            for (int index = 0; index < 2; index++)
            {
                //is transporter empty here?
                if (transportBelts[transporterPosition.x, transporterPosition.z].ores[index] == OreType.None) continue;

                Vector3 direction = transportBelts[transporterPosition.x, transporterPosition.z].directions[index];
                float interpolation = transportBelts[transporterPosition.x, transporterPosition.z].interpolations[index]; //should I invert input when index == 0? -> (0.5f - x)

                Vector3 orePosition = transporterPosition + direction * interpolation;

                //draw ore if visible
                //if (IsTransporterVisible(transporterPosition, minVisibleTile, maxVisibleTile))
                //{
                    DrawTransporterOre(transportBelts[transporterPosition.x, transporterPosition.z].ores[index], orePosition);
                //}

                //increase interpolation -> move ore on belt
                transportBelts[transporterPosition.x, transporterPosition.z].interpolations[index] += transportBelts[transporterPosition.x, transporterPosition.z].speed * Time.deltaTime;

                //ore moved to destionation
                if (transportBelts[transporterPosition.x, transporterPosition.z].interpolations[index] >= 0.5f) //we cannot use values higher than 0.5 because of the coordinates!
                {
                    transportBelts[transporterPosition.x, transporterPosition.z].interpolations[index] = 0.5f; //if it can't transfer we should keep it here!

                    //transfer
                    if (index == 0) //Input to Center
                    {
                        if (transportBelts[transporterPosition.x, transporterPosition.z].ores[1] == OreType.None)
                        {
                            transportBelts[transporterPosition.x, transporterPosition.z].AddOre(transportBelts[transporterPosition.x, transporterPosition.z].ores[index], 1);
                            transportBelts[transporterPosition.x, transporterPosition.z].RemoveOre(index);
                        }
                    }
                    else            //Center to Output
                    {
                        //if the transport system contains belt in out direction we transfer the ore to it
                        Vector3Int nextPosition = transporterPosition + Vector3Int.RoundToInt(transportBelts[transporterPosition.x, transporterPosition.z].directions[index]);

                        if (transporterPositions.Contains(nextPosition) && transportBelts[nextPosition.x, nextPosition.z].ores[0] == OreType.None)
                        {
                            transportBelts[nextPosition.x, nextPosition.z].AddOre(transportBelts[transporterPosition.x, transporterPosition.z].ores[index], 0);
                            transportBelts[transporterPosition.x, transporterPosition.z].RemoveOre(index);
                        }
                    }
                }
            }
        }
    }

    private void DrawTransporterOre(OreType oreType, Vector3 orePosition)
    {
        GraphicDrawer.instance.AddInstance(MapGenerator.instance.oreMaterialLUT[oreType], orePosition);
    }
}