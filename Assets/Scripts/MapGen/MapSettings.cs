using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapSettings", menuName = "Scriptable Objects/MapSettings")]
public class MapSettings : ScriptableObject
{
    public List<NoiseSettings> noiseSettings;
    public List<MapModifier> modifiers;
    [Range(0.2f, 0.9f)] public float waterLevel = 0.2f;
    [Range(0.2f, 0.9f)] public float cliffLevel = 0.5f;
    [Range(8, 256)] public int mapSize = 10;
    [Range(0.0f, 0.9f)][Tooltip("Chance to floodfill ores to neighbour tiles")] public float oreFieldSize = 0.4f;
}