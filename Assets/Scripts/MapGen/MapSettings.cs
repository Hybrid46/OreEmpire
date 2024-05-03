using UnityEngine;

[CreateAssetMenu(fileName = "MapSettings", menuName = "Scriptable Objects/MapSettings")]
public class MapSettings : ScriptableObject
{
    public int mapSize = 256;

    public float waterLevel = 0.2f;
    public Color waterColor = Color.blue;

    [Range(0.0f, 0.9f)] public float oreFieldSize = 0.4f;

}
