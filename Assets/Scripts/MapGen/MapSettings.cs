using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MapSettings", menuName = "Scriptable Objects/MapSettings")]
public class MapSettings : ScriptableObject
{
    [Serializable]
    public class NoiseSettings
    {
        //[Range(1000, 1000000)] public int seedX = UnityEngine.Random.Range(1000, 1000000);
        //[Range(1000, 1000000)] public int seedY = UnityEngine.Random.Range(1000, 1000000);
        [Range(1000, 1000000)] public int seedX;
        [Range(1000, 1000000)] public int seedY;
        public float scale;
        public float amplitude;
        public float minHeight;
        public float maxHeight;
    }

    public NoiseSettings noiseSettings;
    public float waterLevel = 0.2f;
    public int mapSize = 10;
    [Range(0.0f, 0.9f)] public float oreFieldSize = 0.4f;

}
