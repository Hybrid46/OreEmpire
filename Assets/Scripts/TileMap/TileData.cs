using UnityEngine;

public class TileData : ScriptableObject
{
    public enum TileHeightType { Water, Ground, Hill}

    public string tileName;
    public Material tileMaterial;
    public TileHeightType tileHeightType;
}
