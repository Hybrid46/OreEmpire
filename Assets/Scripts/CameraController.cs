using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float zoomSpeed = 5f;

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
        m_Camera.orthographicSize = Mathf.Max(1f, m_Camera.orthographicSize - scrollWheel * zoomSpeed);

        // Camera movement
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontal, vertical, 0f).normalized;
        m_Transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        // Clamp camera position to prevent it from going off indefinitely
        float cameraSize = m_Camera.orthographicSize * m_Camera.aspect;
        float xClamp = Mathf.Clamp(m_Transform.position.x, -cameraSize, cameraSize);
        float yClamp = Mathf.Clamp(m_Transform.position.y, -m_Camera.orthographicSize, m_Camera.orthographicSize);

        m_Transform.position = new Vector3(xClamp, yClamp, m_Transform.position.z);

        //TODO: keep the camera inside world bounds
    }
}
