using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float zoomSpeed = 5f;
    private readonly (float min, float max) minMaxOrthoSize = (5f, 100f);

    private Camera m_Camera;
    private Transform m_Transform;

    private void Start()
    {
        m_Camera = Camera.main;
        m_Transform = transform;
    }

    void Update()
    {
        // Camera zoom with the middle mouse wheel
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        // Camera movement
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontal, vertical, 0f).normalized;
        m_Transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        // Clamp camera position to prevent it from going off indefinitely
        float xClamp = Mathf.Clamp(m_Transform.position.x, 0, MapGenerator.instance.mapSize);
        m_Camera.orthographicSize = Mathf.Clamp(m_Camera.orthographicSize - scrollWheel * zoomSpeed, minMaxOrthoSize.min, minMaxOrthoSize.max);
        float zClamp = Mathf.Clamp(m_Transform.position.z, 0, MapGenerator.instance.mapSize);

        m_Transform.position = new Vector3(xClamp, 1f, zClamp);
    }
}
