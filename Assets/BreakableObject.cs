using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableObject : Destroyable
{
    new public float maxHP = 5;

    public float flyFactor = 0.2f;

    Rigidbody rb;

    // Start is called before the first frame update
    new void Start()
    {
        base.maxHP = maxHP;
        base.Start();
    }

    new void FixedUpdate()
    {
        if (HP <= 0 && !dead && spawned)
        {
            Die();
        }
    }

    new void Die()
    {
        base.Die();

        //Debug.Log("Breaking!");

        transform.parent = null;

        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.velocity += (new Vector3(Random.Range(-1, 1), Random.Range(0, 1), Random.Range(-1, 1))) * flyFactor;
        rb.angularVelocity += (new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1))) * flyFactor;
    }
}
