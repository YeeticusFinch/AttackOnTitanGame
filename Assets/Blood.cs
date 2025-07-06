using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Blood : MonoBehaviour
{
    public static Blood instance;

    [SerializeField]
    private GameObject chunks;
    [SerializeField]
    private GameObject spray;
    [SerializeField]
    private GameObject splatter;
    [SerializeField]
    private GameObject slice;
    [SerializeField]
    private GameObject crit;

    Dictionary<GameObject, (Transform, Vector3)> parents = new Dictionary<GameObject, (Transform, Vector3)>();

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (parents.Count > 0)
        {
            foreach (GameObject p in parents.Keys.ToList())
            {
                if (p == null)
                    parents.Remove(p);
                else
                {
                    Transform t;
                    Vector3 v;
                    (t, v) = parents[p];
                    if (t != null)
                        p.transform.position = t.TransformPoint(v);
                }
            }
        }
    }

    void DoParticles(GameObject particle, Vector3 pos, Vector3 dir, float duration, float scale, Transform parent)
    {
        GameObject p = Instantiate(particle);
        p.transform.position = pos;
        p.transform.rotation = dir == Vector3.up ? Quaternion.LookRotation(dir, Vector3.forward) : Quaternion.LookRotation(dir, Vector3.up);
        p.transform.localScale = new Vector3(scale, scale, scale);
        //p.transform.parent = parent;

        parents.Add(p, (parent, parent.InverseTransformPoint(pos)));

        float longestDuration = duration;

        if (p.GetComponent<ParticleSystem>() != null)
        {
            ParticleSystem ps = p.GetComponent<ParticleSystem>();
            longestDuration = AdjustPS(ps, duration, scale);
        }

        foreach (ParticleSystem ps in p.GetComponentsInChildren<ParticleSystem>())
        {
            float dur = AdjustPS(ps, duration, scale);
            if (longestDuration < dur)
                longestDuration = dur;
        }

        StartCoroutine(Kill(p, longestDuration * 1.4f));
    }
    
    public void Spray(Vector3 pos, Vector3 dir, float duration, float scale, Transform parent)
    {
        DoParticles(spray, pos, dir, duration, scale, parent);
    }

    public void Chunks(Vector3 pos, Vector3 dir, float duration, float scale, Transform parent)
    {
        DoParticles(chunks, pos, dir, duration, scale, parent);
    }

    public void Splatter(Vector3 pos, Vector3 dir, float duration, float scale, Transform parent)
    {
        DoParticles(splatter, pos, dir, duration, scale, parent);
    }

    public void Slice(Vector3 pos, Vector3 dir, float duration, float scale, Transform parent)
    {
        DoParticles(slice, pos, dir, duration, scale, parent);
    }

    public void Crit(Vector3 pos, Vector3 dir, float duration, float scale, Transform parent)
    {
        DoParticles(crit, pos, dir, duration, scale, parent);
    }

    float AdjustPS(ParticleSystem ps, float duration, float scale)
    {
        ps.Stop();

        var main = ps.main;
        if (duration > 0)
            main.duration = duration;
        main.loop = false;
        main.scalingMode = ParticleSystemScalingMode.Local;
        main.startSize = main.startSize.constant * scale;

        ps.Play();

        return main.duration;
    }

    IEnumerator Kill(GameObject o, float time)
    {
        yield return new WaitForSeconds(time);
        if (parents.ContainsKey(o))
            parents.Remove(o);
        GameObject.Destroy(o);
    }
}
