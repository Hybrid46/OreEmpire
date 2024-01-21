using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public int initialCount = 10;
    public int maxCount = 100;
    public GameObject objectType;
    public Stack<GameObject> stack;

    private void Start()
    {
        for (int i = 0; i < initialCount; i++)
        {
            GameObject instance = Instantiate(objectType);
        }
    }

    private void Update()
    {
        if (stack.Count > maxCount)
        {
            GameObject instance = stack.Pop();
#if UNITY_EDITOR
            DestroyImmediate(instance);
#else
            Destroy(instance);
#endif
        }
    }

    public GameObject RequestObject()
    {
        if (stack.Count > 0)
        {
            stack.Pop();
        }
        else //we depleted the stack so need more instances
        {
            GameObject instance = Instantiate(objectType);
            return instance;
        }

        return null;
    }

    public void StoreObject(GameObject storeGameObject)
    {
        stack.Push(storeGameObject);
        storeGameObject.transform.position = new Vector3(-1000, -1000, 0);
    }
}
