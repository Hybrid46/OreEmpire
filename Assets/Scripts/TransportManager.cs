using System;
using System.Collections.Generic;
using UnityEngine;
using OreType = MapGenerator.OreType;

//This script will manage the belt transport system
public class TransportManager
{
    public Transporter[,] transportBelts;
    private HashSet<Vector2Int> transporterPositions; //contains all transporter coordinates for fast iteration and lookup on every transporter

    public struct Transporter
    {
        public float speed;
        public Vector2[] directions;
        public OreType[] ores;
        public float[] interpolations;

        public Transporter(Vector2 outDirection, float speed)
        {
            this.speed = speed;
            this.directions = new Vector2[2] { Vector2.zero, outDirection };
            this.ores = new OreType[2] { OreType.None, OreType.None };
            this.interpolations = new float[2] { 0.0f, 0.0f };
        }

        public void Set(Vector2Int outDirection, float speed)
        {
            this.directions = new Vector2[2] { Vector2.zero, outDirection };
            this.speed = speed;
        }

        public void AddOre(OreType oreType, int index)
        {
            if (!IsLoadable(index))
            {
                Debug.LogError("Belt loading error!");
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
        transporterPositions = new HashSet<Vector2Int>(mapSize * mapSize);

        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                transportBelts[x, y] = new Transporter(Vector2Int.zero, 0.0f);
            }
        }
    }

    public void AddTransporter(Vector2Int position, Vector2Int direction, float speed)
    {
        transporterPositions.Add(position);
        transportBelts[position.x, position.y].Set(direction, speed);
    }

    public void RemoveTransporter(Vector2Int position)
    {
        if (!transporterPositions.Contains(position)) return; //TODO should check where the call comes from to prevent an unnecessary function call

        transporterPositions.Remove(position);
        transportBelts[position.x, position.y].Set(Vector2Int.zero, 0.0f);
    }

    public void RotateTransporter(Vector2Int position, Vector2Int direction) => throw new NotImplementedException();

    public void UpdateTransporters(Camera camera, Vector2Int minVisibleTile, Vector2Int maxVisibleTile)
    {
        //Ore[0] to center from incoming direction
        //Ore[1] to out direction from center

        foreach (Vector2Int transporterPosition in transporterPositions)
        {
            for (int index = 0; index < 2; index++)
            {
                //is transporter empty here?
                if (transportBelts[transporterPosition.x, transporterPosition.y].ores[index] == OreType.None) continue;

                Vector2 direction = transportBelts[transporterPosition.x, transporterPosition.y].directions[index];
                float interpolation = transportBelts[transporterPosition.x, transporterPosition.y].interpolations[index]; //should I invert input when index == 0? -> (0.5f - x)

                Vector2 orePosition = transporterPosition + direction * interpolation;

                //draw ore if visible
                if (IsTransporterVisible(transporterPosition, minVisibleTile, maxVisibleTile))
                {
                    DrawTransporterOre(camera, transportBelts[transporterPosition.x, transporterPosition.y].ores[index], orePosition);
                }

                //increase interpolation -> move ore on belt
                transportBelts[transporterPosition.x, transporterPosition.y].interpolations[index] += transportBelts[transporterPosition.x, transporterPosition.y].speed * Time.deltaTime;

                //ore moved to destionation
                if (transportBelts[transporterPosition.x, transporterPosition.y].interpolations[index] >= 0.5f) //we cannot use values higher than 0.5 because of the coordinates!
                {
                    transportBelts[transporterPosition.x, transporterPosition.y].interpolations[index] = 0.5f; //if it can't transfer we should keep it here!

                    //transfer
                    if (index == 0) //Input to Center
                    {
                        if (transportBelts[transporterPosition.x, transporterPosition.y].ores[1] == OreType.None)
                        {
                            transportBelts[transporterPosition.x, transporterPosition.y].AddOre(transportBelts[transporterPosition.x, transporterPosition.y].ores[index], 1);
                            transportBelts[transporterPosition.x, transporterPosition.y].RemoveOre(index);
                        }
                    }
                    else            //Center to Output
                    {
                        //if the transport system contains belt in out direction we transfer the ore to it
                        Vector2Int nextPosition = transporterPosition + Vector2Int.RoundToInt(transportBelts[transporterPosition.x, transporterPosition.y].directions[index]);

                        if (transporterPositions.Contains(nextPosition) && transportBelts[nextPosition.x, nextPosition.y].ores[0] == OreType.None)
                        {
                            transportBelts[nextPosition.x, nextPosition.y].AddOre(transportBelts[transporterPosition.x, transporterPosition.y].ores[index], 0);
                            transportBelts[transporterPosition.x, transporterPosition.y].RemoveOre(index);
                        }
                    }
                }
            }
        }
    }

    private void DrawTransporterOre(Camera camera, OreType oreType, Vector2 orePosition)
    {
        Rect rect = new Rect(camera.WorldToScreenPoint(orePosition), Vector2.one * 32.0f);

        Graphics.DrawTexture(rect, MapGenerator.instance.oreTileLUT[oreType].m_DefaultSprite.texture);
    }

    private bool IsTransporterVisible(Vector2Int transporterPosition, Vector2Int minVisibleTile, Vector2Int maxVisibleTile) => transporterPosition.x >= minVisibleTile.x && transporterPosition.x <= maxVisibleTile.x &&
                                                                                                                               transporterPosition.y >= minVisibleTile.y && transporterPosition.y <= maxVisibleTile.y;
}