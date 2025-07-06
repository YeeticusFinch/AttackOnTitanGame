using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyable : MonoBehaviour
{
    [System.NonSerialized]
    public float maxHP;

    [System.NonSerialized]
    public float HP;

    [System.NonSerialized]
    public bool isPlayer = false;

    [System.NonSerialized]
    public bool isTitan = false;

    protected bool dead = false;
    protected bool spawned = false;

    [System.NonSerialized]
    public bool grappled = false;

    [System.NonSerialized]
    public Grabber grabber;

    [System.NonSerialized]
    public int grabCooldown = 0;

    // Start is called before the first frame update
    protected void Start()
    {
        spawned = true;
        HP = maxHP;

        if (GetComponent<Player>() != null || GetComponentInParent<Player>() != null)
            isPlayer = true;

        if (GetComponent<Titan>() != null || GetComponentInParent<Titan>() != null)
            isTitan = true;
    }

    // Update is called once per frame
    void Update()
    {
    }

    protected void FixedUpdate()
    {
        if (grabCooldown > 0)
            grabCooldown--;

        if (HP <= 0)
        {
            HP = 0;
            if (!dead && spawned)
            {
                Die();
            }
        }
    }

    protected void Die()
    {
        //Debug.Log("Dead");
        dead = true;
    }

    public virtual void Damage(float amount)
    {
        HP -= amount;
    }
}
