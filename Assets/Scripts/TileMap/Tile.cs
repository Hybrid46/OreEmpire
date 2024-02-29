using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum RotationDirection { Up, Down, Right, Left }

    public TileData tileData;
    public RotationDirection rotationDirection;
}
