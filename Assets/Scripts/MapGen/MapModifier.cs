using UnityEngine;

[CreateAssetMenu(fileName = "MapModifier", menuName = "Scriptable Objects/MapModifier")]
public class MapModifier : ScriptableObject
{
    public enum Type { Smoothing, Roughing }

    [Range(1, 10)] public int range;
    [Range(0.1f, 1f)] public float intensity;
}
