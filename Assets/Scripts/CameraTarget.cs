using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    private int ready = 0;
    private int treshold = 100;

    void Update()
    {
        ready++;
        if (ready < treshold) return;
        if (ready > treshold) ready = treshold;

        //ready == treshold -> Update
        if (Input.GetKey(KeyCode.W)) transform.position += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) transform.position += Vector3.back;

        if (Input.GetKey(KeyCode.D)) transform.position += Vector3.right;
        if (Input.GetKey(KeyCode.A)) transform.position += Vector3.left;
    }
}
