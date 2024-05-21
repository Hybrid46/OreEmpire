using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NoiseSettings", menuName = "Scriptable Objects/NoiseSettings")]
public class NoiseSettings : ScriptableObject
{
    [Range(1000, 1000000)] public int seedX;
    [Range(1000, 1000000)] public int seedY;
    public float scale;
    public float amplitude;
    public float minHeight;
    public float maxHeight;
}
