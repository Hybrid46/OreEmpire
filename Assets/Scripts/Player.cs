using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Serializable]
    public struct Resources
    {
        public int copper;

        public Resources(int copper)
        {
            this.copper = copper;
        }
    }

    public Resources resources;
}
