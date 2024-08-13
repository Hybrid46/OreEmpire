using UnityEngine;

public static class QuaternionExtensions
{
    public static Quaternion Diff(this Quaternion to, Quaternion from) => to * Quaternion.Inverse(from);

    public static Quaternion Add(this Quaternion start, Quaternion add) => add * start;
}