using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ODM : MonoBehaviour
{
    [NonSerialized]
    public GameObject hook;
    public GameObject hookAim;

    [NonSerialized]
    public Player player;

    [NonSerialized]
    public bool hookHit = false;
    [NonSerialized]
    public bool retracting = false;
    int hookFly = 0;

    float hookSpeed = 0.4f;
    int hookLifetime = 100;
    float force = 10f;

    float cableLength = 0;

    public LineRenderer lr;
    //public RoundedCornerLine lr;

    float startOffset = 0.1f;

    float maxSpeed = 8;

    public KeyCode pullButton;

    Vector3[] randDir = new Vector3[10];

    // Start is called before the first frame update
    void Start()
    {
        if (lr == null)
            lr = GetComponent<LineRenderer>();
        for (int i = 0; i < randDir.Length; i++)
        {
            randDir[i] = (new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), Random.Range(-10, 10))).normalized;
        }
    }

    public bool findNearest(int yMin, int yMax)
    {
        bool foundIt = false;
        Vector3 ogEuler = transform.localEulerAngles;
        for (int i = 0; i < 45; i++)
        {
            if (yMin == 0)
            {
                for (int j = 0; j <= yMax; j++)
                {
                    transform.localEulerAngles = ogEuler + new Vector3(i, j);
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position + transform.forward * startOffset, transform.forward, out hit, hookSpeed * hookLifetime, player.lm))
                    {
                        foundIt = true;
                        return true;
                    }
                    transform.localEulerAngles = ogEuler + new Vector3(-i, j);
                    if (Physics.Raycast(transform.position + transform.forward * startOffset, transform.forward, out hit, hookSpeed * hookLifetime, player.lm))
                    {
                        foundIt = true;
                        return true;
                    }
                }
            } else
            {
                for (int j = yMax; j >= yMin; j--)
                {
                    transform.localEulerAngles = ogEuler + new Vector3(i, j);
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position + transform.forward * startOffset, transform.forward, out hit, hookSpeed * hookLifetime, player.lm))
                    {
                        foundIt = true;
                        return true;
                    }
                    transform.localEulerAngles = ogEuler + new Vector3(-i, j);
                    if (Physics.Raycast(transform.position + transform.forward * startOffset, transform.forward, out hit, hookSpeed * hookLifetime, player.lm))
                    {
                        foundIt = true;
                        return true;
                    }
                }
            }
            if (foundIt)
                break;
        }
        if (!foundIt)
            transform.localEulerAngles = ogEuler;

        return foundIt;
    }

    // Update is called once per frame
    void Update()
    {
        if (hook != null)
        {
            if (hookHit)
            {
                lr.positionCount = 2;
                lr.SetPositions(new Vector3[] { transform.position, hook.transform.position });
                //lr._points = new Vector3[] { transform.position, hook.transform.position };
            }
            else
            {
                Vector3[] positions = new Vector3[randDir.Length + 2];
                positions[0] = transform.position;
                positions[positions.Length - 1] = hook.transform.position;
                float dist = Vector3.Distance(hook.transform.position, transform.position);
                for (int i = 0; i < randDir.Length; i++)
                {
                    positions[i + 1] = (hook.transform.position * (i + 1) + transform.position * (positions.Length - (i + 1))) / positions.Length + randDir[i] * 0.05f * Mathf.Clamp(dist, 0, 1);
                    randDir[i] += (new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), Random.Range(-10, 10))).normalized * 0.2f;
                    randDir[i] = randDir[i].normalized;
                }
                lr.positionCount = positions.Length;
                lr.SetPositions(positions);
            }
            hookAim.SetActive(false);
        } else
        {
            hookAim.SetActive(true);
            lr.positionCount = 2;
            lr.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
            RaycastHit hit;
            if (Physics.Raycast(transform.position + transform.forward * startOffset, transform.forward, out hit, hookSpeed * hookLifetime, player.lm))
            {
                hookAim.transform.position = hit.point;
            } else
            {
                hookAim.transform.position = transform.position + transform.forward * 10;
            }
        }
    }

    private void FixedUpdate()
    {
        if (hook != null)
        {
            if (hookHit == false)
            {
                if (retracting)
                {
                    if (hook.transform.parent != null)
                        hook.transform.parent = null;
                    if (hookFly > hookLifetime || Vector3.Distance(hook.transform.position, transform.position) < hookSpeed)
                    {
                        GameObject.Destroy(hook);
                        retracting = false;

                        //FancySound.Stop(player.odmRetract, gameObject);
                        player.odmRetract.stop();
                    }
                    hook.transform.position += (transform.position - hook.transform.position).normalized * hookSpeed;
                }
                else
                {
                    if (hookFly > hookLifetime)
                    {
                        hookFly = 0;
                        retracting = true;
                        hook.transform.parent = null;
                    }
                    RaycastHit hit;
                    if (Physics.Raycast(hook.transform.position, hook.transform.forward, out hit, hookSpeed, player.lm))
                    {
                        hook.transform.position = hit.point;
                        hook.transform.parent = hit.collider.gameObject.transform;
                        hookHit = true;
                        cableLength = Vector3.Distance(player.transform.position, hook.transform.position) * 1.1f;
                    }
                    else
                        hook.transform.position += hook.transform.forward * hookSpeed;
                }
                hookFly++;
            } else
            {
                if (Vector3.Distance(player.transform.position, hook.transform.position) > cableLength)
                {
                    if (!player.pulling)
                    {
                        //player.rb.MovePosition(hook.transform.position + (player.transform.position - hook.transform.position).normalized * cableLength);
                        if (Vector3.Dot(player.rb.velocity, (player.transform.position - hook.transform.position).normalized) > 0.01f)
                            player.rb.velocity = Vector3.ProjectOnPlane(player.rb.velocity, (hook.transform.position - player.transform.position).normalized);

                        targetPos = (hook.transform.position + (player.transform.position - hook.transform.position).normalized * cableLength);

                        //Debug.Log("CableLength=" + cableLength + ", targetPos=" + targetPos);
                    }
                    else
                        targetPos = Vector3.zero;
                    //player.rb.AddForce(((hook.transform.position - player.transform.position).normalized * maxSpeed * 0.5f - player.rb.velocity).normalized * force);

                    //player.rb.AddForce(Vector3.Project(player.rb.GetAccumulatedForce(), (hook.transform.position - player.transform.position).normalized));
                }
                else
                    targetPos = Vector3.zero;

                if (targetPos.magnitude > 0.01f)
                {
                    Debug.DrawLine(hook.transform.position, targetPos, Color.green);
                    Vector3 fancyForce = (targetPos - transform.position) * 100;
                    player.rb.AddForce(fancyForce);
                    //Debug.Log("Adding force " + fancyForce);
                }
                /*
                if (Vector3.Distance(player.transform.position, hook.transform.position) > cableLength) {
                    Debug.Log("Cable Length Exceeded");

                    
                    if (Vector3.Dot(-player.rb.GetAccumulatedForce(), (hook.transform.position - player.transform.position)) > 0) {
                        Debug.Log("Force in opposite direction");
                        player.rb.AddForce(Vector3.Project(-player.rb.GetAccumulatedForce(), (hook.transform.position - player.transform.position).normalized), ForceMode.Impulse);
                    }
                    
                }
                */
            }
        }

    }

    Vector3 targetPos;

    public void Fire(KeyCode newPullButton = KeyCode.P)
    {
        //if (newPullButton != KeyCode.P)
            pullButton = newPullButton;
        if (hook == null)
        {
            //FancySound.Play(player.odmFire, transform.position, gameObject, true, 0.7f);
            //StartSound.instance.sound.PlayAtObject(player.odmFire, gameObject, 1, 1, 0.1f, -1, 1);

            player.pulling = false;
            retracting = false;
            hookFly = 0;
            hookHit = false;
            hook = GameObject.Instantiate(Resources.Load("Hook")) as GameObject;
            hook.transform.position = transform.position + transform.forward * startOffset;
            hook.transform.rotation = transform.rotation;

            player.odmFire.play(transform.position, gameObject);
        } else if (hookHit)
        {
            Pull();
        }
    }

    public void Pull()
    {
        if (hook != null && hookHit)
        {
            //FancySound.Play(player.odmPull, transform.position, gameObject, false, 1, 1);
            //StartSound.instance.sound.PlayAtObject(player.odmPull, gameObject, 1, 1, 0.1f, -1, 1);

            if (Vector3.Dot(player.rb.velocity, (hook.transform.position - player.transform.position).normalized) < maxSpeed)
            {
                player.rb.AddForce(((hook.transform.position - player.transform.position).normalized * maxSpeed - player.rb.velocity).normalized * force);
                //player.rb.AddForce((hook.transform.position - player.transform.position).normalized * force);
            }
            //player.rb.AddForce(-Physics.gravity * player.rb.mass * 0.5f);
            cableLength = Vector3.Distance(hook.transform.position, player.transform.position);
            //float fancyDot = Vector3.Dot(player.rb.GetAccumulatedForce(), (hook.transform.position - player.transform.position).normalized);
            //if (fancyDot > 0.1f)
            //    cableLength -= 0.01f * fancyDot;
            player.pulling = true;

            player.odmPull.play(transform.position, gameObject);
        }
    }

    public void Retract()
    {
        if (hook != null && !retracting)
        {
            //FancySound.Play(player.odmRetract, transform.position, gameObject, false, 0.7f);
            //StartSound.instance.sound.PlayAtObject(player.odmRetract, gameObject, 1, 1, 0.3f, -1, 1);

            pullButton = KeyCode.P;
            hookFly = 0;
            hookHit = false;
            retracting = true;

            player.odmRetract.play(transform.position, gameObject);
        }
    }
}
