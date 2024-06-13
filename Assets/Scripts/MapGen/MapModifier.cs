using UnityEngine;

[CreateAssetMenu(fileName = "MapModifier", menuName = "Scriptable Objects/MapModifier")]
public class MapModifier : ScriptableObject
{
    public enum MapModifierType { IDWSmoothing, Smoothing, MoreWater, MoreHills }

    [Range(1, 10)] public int range;
    [Range(0.01f, 1f)] public float intensity;
    public MapModifierType modifierType = MapModifierType.Smoothing;
}