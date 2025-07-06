using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Limb : Destroyable
{
    [System.NonSerialized]
    public bool usable = true;

    public new int maxHP = 10;

    public float toughness = 0.2f;

    public bool weakSpot = false;

    public float healRate = 0.1f;
    public int healDelay = 10;

    public GameObject[] mesh;

    Vector3 initScale;

    //public List<GameObject> meshes = new List<GameObject>();

    //List<GameObject> deadStuff = new List<GameObject>();

    bool sliced = false;

    //public GameObject mesh;

    // Start is called before the first frame update
    new void Start()
    {
        base.maxHP = maxHP;
        base.Start();

        initScale = transform.localScale;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool IsUsable()
    {
        if (!usable)
            return false;
        foreach (Limb l in GetComponentsInParent<Limb>())
            if (!l.usable)
                return false;
        return true;
        //return usable && Vector3.Distance(initScale, transform.localScale) < 0.1f;
    }

    void AddMesh(GameObject parent, GameObject[] mesh)
    {
        foreach (GameObject o in mesh)
        {
            GameObject yeet = new GameObject();
            MeshFilter mf = yeet.AddComponent<MeshFilter>();
            MeshRenderer mr = yeet.AddComponent<MeshRenderer>();
            mf.mesh = o.GetComponent<SkinnedMeshRenderer>() != null ? o.GetComponent<SkinnedMeshRenderer>().sharedMesh : o.GetComponent<MeshFilter>().mesh;
            mr.materials = o.GetComponent<SkinnedMeshRenderer>() != null ? o.GetComponent<SkinnedMeshRenderer>().materials : o.GetComponent<MeshRenderer>().materials;
            yeet.transform.parent = parent.transform;
            CopyScale(o.transform, yeet.transform);
            yeet.transform.position = o.transform.position;
            yeet.transform.rotation = o.transform.rotation;
            yeet.AddComponent<CapsuleCollider>();
            //yeet.transform.localScale = yeet.transform.InverseTransformScale(o.transform.TransformScale(o.transform.localScale));
        }
    }

    void CopyScale(Transform source, Transform target)
    {
        // Step 1: Calculate the world scale of the source object
        Vector3 sourceWorldScale = source.lossyScale;

        // Step 2: If the target has a parent, adjust the scale relative to the parent's scale
        if (target.parent != null)
        {
            Vector3 parentScale = target.parent.lossyScale;
            target.localScale = new Vector3(
                sourceWorldScale.x / parentScale.x,
                sourceWorldScale.y / parentScale.y,
                sourceWorldScale.z / parentScale.z
            );
        }
        else
        {
            // If no parent, directly assign the world scale
            target.localScale = sourceWorldScale;
        }
    }


    new private void FixedUpdate()
    {
        base.FixedUpdate();

        if (HP <= 0 && !sliced)
        {
            if (weakSpot)
                GetComponentInParent<Titan>().Kill();
            else
            {
                sliced = true;
                usable = false;

                Blood.instance.Spray(transform.position, (new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1))).normalized, 1.2f, 1.3f, transform);
                Blood.instance.Chunks(transform.position, (new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1))).normalized, 0.6f, 0.9f, transform);

                GameObject deadThing = new GameObject();
                deadThing.transform.parent = null;
                deadThing.name = "Dead Limb";

                AddMesh(deadThing, mesh);

                //Destroy(deadThing.GetComponent<Limb>());
                foreach (Limb l in GetComponentsInChildren<Limb>().ToList())
                {
                    AddMesh(deadThing, l.mesh);
                    //l.enabled = false;
                    //Destroy(l);
                }

                foreach (Damager d in deadThing.GetComponentsInChildren<Damager>().ToList())
                {
                    d.enabled = false;
                    Destroy(d);
                }
                Rigidbody dead_rb = deadThing.AddComponent<Rigidbody>();
                dead_rb.velocity = new Vector3(Random.Range(-1, 1), Random.Range(0, 1), Random.Range(-1, 1));
                dead_rb.angularVelocity = new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1));
                StartCoroutine(DestroyIn(deadThing, Random.Range(4, 8)));

                transform.localScale = initScale * 0.01f;

                StartCoroutine(Heal());
            }
        }
    }

    public override void Damage(float amount)
    {
        base.Damage(amount);
    }

    IEnumerator DestroyIn(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);

        GameObject.Destroy(obj);
    }

    IEnumerator Heal()
    {
        yield return new WaitForSeconds(healDelay);

        HP += healRate;
        sliced = false;

        while (HP > 0 && HP < maxHP)
        {
            transform.localScale = initScale * (float)HP / maxHP;
            yield return new WaitForSeconds(0.1f);
            HP += healRate;
        }
        if (HP > maxHP - healRate)
        {
            HP = maxHP;
            usable = true;
        } else if (HP < healRate)
        {
            // Died before getting fully healed
        }
    }
}
