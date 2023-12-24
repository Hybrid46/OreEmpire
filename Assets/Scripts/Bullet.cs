using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int hp;
    public int damage;
    public Vector3 velocity;
    private Transform m_transform;

    void Start()
    {
        m_transform = transform;
    }

    void Update()
    {
        m_transform.position += velocity;

        if (MapGenerator.instance.IsOnMap(m_transform.position.x, m_transform.position.y)) Death(true);
    }

    private protected virtual void Death(bool force = false)
    {
        if (hp <= 0 || force) Destroy(gameObject);
    }

    private void Destroy()
    {
        Destroy(gameObject);
    }
}
