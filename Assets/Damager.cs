using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damager : MonoBehaviour
{
    [System.NonSerialized]
    protected bool damaging;

    protected Collider trigger;

    protected bool playerOwner;
    protected bool titanOwner;

    protected Player player;
    protected Titan titan;

    public float damage = 0;
    public bool continuousDamage = false;
    public int damageFreq = 5;

    public bool clearOnDisable = false;

    protected List<Destroyable> targets = new List<Destroyable>();

    // Start is called before the first frame update
    protected void Start()
    {
        trigger = GetComponent<Collider>();
        trigger.isTrigger = true;
        trigger.enabled = false;

        player = GetComponentInParent<Player>();

        if (player != null)
            playerOwner = true;
        else
            playerOwner = false;

        if (!playerOwner)
        {
            titan = GetComponentInParent<Titan>();
            if (titan != null)
                titanOwner = true;
        }

    }

    int c = 0;
    protected void FixedUpdate()
    {
        c++;
        c %= 10000;

        if (!damaging && clearOnDisable)
        {
            targets.Clear();
        }

        if (continuousDamage && targets.Count > 0 && c % (int)(50/damageFreq) == 0)
        {
            foreach (Destroyable t in targets)
                Damage(t);
        }
    }

    protected void Damage(Destroyable t)
    {
        if (isDamageable(t))
        {
            Debug.Log(t.gameObject.name + " hit by " + gameObject.name);


            if (playerOwner && t.HP > 0)
            {
                float r1 = Random.Range(0.01f, 1f);
                float r2 = Random.Range(0.01f, 1f);
                float r3 = Random.Range(0.01f, 1f);
                float r4 = Random.Range(0.01f, 0.1f);
                Vector3 randomHitPos = (r1 * trigger.ClosestPoint(t.transform.position) + r2 * t.transform.position + r3 * trigger.transform.position + r4 * new Vector3(Random.Range(-1, 1), Random.Range(0, 1), Random.Range(-1, 1))) / (r1 + r2 + r3);

                Limb l = t.GetComponent<Limb>();

                if (l != null && l.usable && t.GetComponent<Limb>().weakSpot)
                {
                        Blood.instance.Crit(randomHitPos, Vector3.zero, -1, 0.9f, t.transform);
                        player.swordCrit.play(randomHitPos);
                }
                else if (l == null || l.usable)
                {
                    Blood.instance.Slice(randomHitPos, Vector3.zero, -1, 0.7f, t.transform);
                    player.swordSlice.play(randomHitPos);
                }
            }

            t.Damage(damage);
        }
    }

    protected bool isDamageable(GameObject g)
    {

        Destroyable obj = g.GetComponent<Destroyable>();
        if (obj == null)
            obj = g.GetComponentInParent<Destroyable>();

        if (obj == null)
            return false;

        return isDamageable(obj);
    }

    protected bool isDamageable(Destroyable t)
    {
        return t != null && !(playerOwner && t.isPlayer) && !(titanOwner && t.isTitan);
    }

    public void Enable(bool damaging)
    {
        this.damaging = damaging;
        trigger.enabled = damaging;
    }

    public bool IsEnabled()
    {
        return damaging;
    }

    protected void OnTriggerEnter(Collider other)
    {
        Destroyable obj = other.GetComponent<Destroyable>();
        if (obj == null)
            obj = other.GetComponentInParent<Destroyable>();

        if (isDamageable(obj))
        {
            Damage(obj);

            if (continuousDamage)
                targets.Add(obj);
        }
    }

    protected void OnTriggerExit(Collider other)
    {
        Destroyable obj = other.GetComponent<Destroyable>();
        if (obj == null)
            obj = other.GetComponentInParent<Destroyable>();

        if (targets.Contains(obj))
            targets.Remove(obj);
    }
}
