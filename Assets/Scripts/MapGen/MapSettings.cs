using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapSettings", menuName = "Scriptable Objects/MapSettings")]
public class MapSettings : ScriptableObject
{
    [Range(0f, 0.5f)] public float surfaceRoughness = 0.25f;
    public NoiseSettings surfaceNoiseSettings;
    public List<NoiseSettings> noiseSettings;
    public List<MapModifier> modifiers;
    
    [Tooltip("Terrace count for map generator")]
    [Range(2, 8)] public int terraceCount = 3;

    [Tooltip("Map size in chunks (x16)")]
    [Range(8, 256)] public int mapSize = 10;

    [Tooltip("Chance to floodfill ores to neighbour tiles")]
    [Range(0.0f, 0.9f)] public float oreFieldSize = 0.4f;
}