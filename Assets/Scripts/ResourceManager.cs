using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public enum ResourceType
    {
        None,
        Copper,
        Sand,
        Water
    }

    public class Resource
    {
        public ResourceType type;
        public GameObject prefab;
    }

    public List<Resource> resources;
    public Dictionary<ResourceType, GameObject> resourceLUT;
    public Dictionary<ResourceType, Material> resourceMaterialLUT;

    private void Start()
    {
        resources.ForEach(resource => { resourceLUT.Add(resource.type, resource.prefab); });
        resources.ForEach(resource => { resourceMaterialLUT.Add(resource.type, resource.prefab.GetComponent<Renderer>().sharedMaterial); });
    }
}
