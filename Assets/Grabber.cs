using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grabber : Damager
{
    public float escapeLikelyhood = 0.5f;
    public Vector3 grabbedDir = Vector3.up;
    public bool onlyGrabHumans = true;

    public float grabTime = 0;

    List<Destroyable> grabShortList = new List<Destroyable>();

    Limb limb;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        limb = GetComponentInParent<Limb>();
    }

    // Update is called once per frame
    void Update()
    {
        if (limb.HP > 0)
        {

            if (targets.Count > 0)
            {
                foreach (Destroyable t in targets)
                {
                    if (t != null && t.grabber == this)
                    {
                        if (t.grabCooldown > 0)
                            Free(t);
                        else if (!onlyGrabHumans || t.GetComponent<Human>() != null)
                        {
                            t.transform.position = transform.position;
                            float angle = Vector3.Angle(t.transform.up, transform.TransformDirection(grabbedDir.normalized));
                            if (angle > 2)
                            {
                                // Calculate the target rotation
                                Quaternion targetRotation = Quaternion.FromToRotation(t.transform.up, transform.TransformDirection(grabbedDir.normalized));

                                // Rotate towards the target rotation by 5 degrees per frame
                                //t.transform.rotation = Quaternion.RotateTowards(t.transform.rotation, targetRotation, 2 * Time.deltaTime);
                                t.transform.rotation = targetRotation;

                                //t.transform.rotation = Quaternion.RotateTowards(t.transform.rotation, Quaternion.LookRotation(transform.TransformDirection(grabbedDir.normalized), t.transform.forward) * Quaternion.AngleAxis(90, t.transform.right), 0.1f);
                                //t.transform.rotation = Quaternion.RotateTowards(Quaternion.LookRotation(t.transform.up), Quaternion.LookRotation(transform.TransformDirection(grabbedDir.normalized)), 1);
                            }
                        }
                    }
                }
            }
        } else
        {
            // Drop everything if the limb is dead
            ReleaseAll();
        }
    }

    public void ReleaseAll()
    {
        if (targets.Count > 0)
        {
            foreach (Destroyable t in targets)
            {
                if (t.grabber == this)
                {
                    t.grappled = false;
                    t.grabber = null;
                }
            }
            targets.Clear();
        }
    }

    public void Remove(Destroyable t)
    {
        if (targets.Contains(t))
            targets.Remove(t);
    }

    public void Add(Destroyable t)
    {
        if (!targets.Contains(t))
        {
            targets.Add(t);
            t.grappled = true;
            t.grabber = this;
        }
    }

    public void Free(Destroyable t)
    {
        Remove(t);
        t.grappled = false;
        t.grabber = null;
    }

    protected new void OnTriggerEnter(Collider other)
    {
        Destroyable obj = other.GetComponent<Destroyable>();

        if (other.GetComponent<Damager>() != null || other.GetComponentInParent<Damager>() != null)
            return;

        if (obj == null)
            obj = other.GetComponentInParent<Destroyable>();

        if (isDamageable(obj))
        {
            if (obj.grabCooldown == 0)
            {
                if (grabTime == 0)
                {
                    Grab(obj);
                } else
                {
                    grabShortList.Add(obj);
                    StartCoroutine(GrabIn(grabTime, obj));
                }
            }
        }
    }

    void Grab(Destroyable obj)
    {
        //Debug.Log(obj.gameObject.name + " hit by " + gameObject.name);

        Damage(obj);

        if (!onlyGrabHumans || obj.GetComponent<Human>() != null)
        {
            obj.grappled = true;
            obj.grabber = this;
            targets.Add(obj);
        }
    }

    protected new void OnTriggerExit(Collider other)
    {

        Destroyable obj = other.GetComponent<Destroyable>();
        if (obj == null)
            obj = other.GetComponentInParent<Destroyable>();

        if (obj != null && grabShortList.Contains(obj))
            grabShortList.Remove(obj);
        /*
        Destroyable obj = other.GetComponent<Destroyable>();
        if (obj == null)
            obj = other.GetComponentInParent<Destroyable>();

        if (obj.grabber == this)
        {
            obj.grabber = null;
            obj.grappled = false;
        }

        if (targets.Contains(obj))
        {
            targets.Remove(obj);
        }
        */
    }

    IEnumerator GrabIn(float time, Destroyable obj)
    {
        float interval = 0.2f;
        for (float t = 0; t < time; t += interval)
        {
            yield return new WaitForSeconds(Mathf.Min(interval, time));
            if (!grabShortList.Contains(obj))
            {
                break;
            }
        }

        if (grabShortList.Contains(obj))
        {
            Grab(obj);
        }
    }
}
