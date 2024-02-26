using UnityEngine;

[CreateAssetMenu]
public class Ore : ScriptableObject
{
    public MapGenerator.OreType type;
    public RuleTile tile;
    public Sprite sprite;
    public Material material;


}
