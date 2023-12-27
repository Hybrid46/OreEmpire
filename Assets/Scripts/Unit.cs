using UnityEngine;

public class Unit : MonoBehaviour
{
    public int sizeX;
    public int sizeY;

    public int armor;
    public int hp;

    private protected int oreContain;               // how much ore it can contain
    private protected bool Miner;                   // is miner?
    private protected MapGenerator.OreType oreType; // the type of ore for mining
    private protected float minerState;             // actual state of mining -> when 1 it finishes mining ore
    private protected float minerSpeed;             // mining speed

    void Start()
    {

    }

    void Update()
    {
        if (Miner) Mining();
    }

    private void Mining()
    {
        minerState += minerSpeed;

        if (minerState >= 1.0f)
        {
            GenerateOre();
            minerState -= 1.0f;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Bullet bullet;

        if (collision.gameObject.TryGetComponent(out bullet))
        {
            ChangeHP(-Mathf.Clamp(bullet.damage - armor, 0, 10000));
        }
    }

    private protected virtual void Death(bool force = false)
    {
        if (hp <= 0 || force) Destroy(gameObject);
    }

    private protected virtual void ChangeHP(int hpModifier)
    {
        hp += hpModifier;
        Death();
    }

    private protected virtual void GenerateOre()
    {
        //spawn one oretype
    }
}
