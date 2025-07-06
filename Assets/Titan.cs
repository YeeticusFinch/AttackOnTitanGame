using DitzelGames.FastIK;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//[ExecuteInEditMode]
public class Titan : MonoBehaviour
{
    public bool forceStandFrame = false;
    public GameObject animatePack;
    public GameObject armature;

    Animate walkAnim;
    List<List<Animate.KeyFrame>> walkFrames = new List<List<Animate.KeyFrame>>();
    List<List<Animate.KeyFrame>> walkLegsOnlyFrames = new List<List<Animate.KeyFrame>>();
    int walkFrame = 0;
    Animate standAnim;
    List<Animate.KeyFrame> standFrames = new List<Animate.KeyFrame>();
    Animate climbAnim;
    List<List<Animate.KeyFrame>> climbFrames = new List<List<Animate.KeyFrame>>();
    Animate topClimbAnim;
    List<Animate.KeyFrame> topClimbFrames = new List<Animate.KeyFrame>();

    Animate jawOpenAnim;
    List<Animate.KeyFrame> jawOpenFrames = new List<Animate.KeyFrame>();

    Animate jawCloseAnim;
    List<Animate.KeyFrame> jawCloseFrames = new List<Animate.KeyFrame>();

    Animate standFeetOnlyAnim;
    List<Animate.KeyFrame> standFeetOnlyFrames = new List<Animate.KeyFrame>();

    Animate openHandsAnim;
    List<Animate.KeyFrame> openHandsFrames = new List<Animate.KeyFrame>();
    Animate closeHandsAnim;
    List<Animate.KeyFrame> closeHandsFrames = new List<Animate.KeyFrame>();

    Animate eatAnim;
    List<Animate.KeyFrame> eatFrames = new List<Animate.KeyFrame>();

    Rigidbody rb;

    public Vector3 target;
    public float moveSpeed = 20;
    public float maxMoveSpeed = 30;
    public float friction = 50;
    public float turnSpeed = 1;
    public float maxTurnSpeed = 2;
    public float eyeSpeed = 3;

    public float attackSpeed = 0.005f;

    public float width = 3;
    public float height = 10;

    public int followDistance = 2000;
    
    public float pathTol = 1;

    public GameObject[] eyes;

    public float stepHeight = 0.8f;
    public float climbTax = 2;
    public float maxClimbHeight = 1;
    public float climbRange = 2;

    double lastPathfind = 0;
    bool waitingForPathfind = false;

    int mode = 0;
    int modeChangeCooldown = 0;

    public float range = 0.5f;

    public GameObject[] hands;
    public GameObject mouth;

    public SoundManager.Sound stepSound;
    //public GameObject jaw;

    //public Collider mouthCollider;

    int eatStage = 0;

    bool killed = false;
    public void Kill()
    {
        if (!killed)
        {
            killed = true;

            foreach (Grabber g in GetComponentsInChildren<Grabber>())
            {
                g.ReleaseAll();
                g.Enable(false);
            }

            foreach (Damager d in GetComponentsInChildren<Damager>())
            {
                d.Enable(false);
            }
            foreach (Limb l in GetComponentsInChildren<Limb>().ToList())
            {
                if (!l.usable)
                {
                    GameObject.Destroy(l.gameObject);
                }
                else
                {
                    l.enabled = false;
                }
            }
            foreach (FastIKFabric f in GetComponentsInChildren<FastIKFabric>())
            {
                f.enabled = false;
            }

            for (int i = 0; i < armature.transform.childCount; i++)
            {
                Transform bone = armature.transform.GetChild(i);
                rb.isKinematic = true;
                bone.parent = null;
                Rigidbody r = bone.gameObject.AddComponent<Rigidbody>();
                r.mass = rb.mass * 3;
                //CharacterJoint cj = bone.gameObject.AddComponent<CharacterJoint>();
                //cj.connectedBody = rb;
                RagDoll(bone, 3);
            }
        }
    }

    public void RagDoll(Transform parentBone, int depth)
    {
        if (depth <= 0)
            return;
        depth--;
        for (int i = 0; i < parentBone.transform.childCount; i++)
        {
            Transform bone = parentBone.transform.GetChild(i);
            Rigidbody r = bone.gameObject.AddComponent<Rigidbody>();
            CharacterJoint cj = bone.gameObject.AddComponent<CharacterJoint>();
            cj.connectedBody = parentBone.gameObject.GetComponent<Rigidbody>();
            RagDoll(bone, depth);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        height = Map.MetersToUnits(height);
        rb = GetComponent<Rigidbody>();
        foreach (Animate a in animatePack.GetComponents<Animate>())
        {
            if (a.name == "Walk")
            {
                walkAnim = a;
            } else if (a.name == "Stand")
            {
                standAnim = a;
            } else if (a.name == "Climb")
            {
                climbAnim = a;
            } else if (a.name == "Climb Top")
            {
                topClimbAnim = a;
            }
            else if (a.name == "Mouth Open")
            {
                jawOpenAnim = a;
            }
            else if (a.name == "Mouth Close")
            {
                jawCloseAnim = a;
            }
            else if (a.name == "Stand Feet Only")
            {
                standFeetOnlyAnim = a;
            }
            else if (a.name == "Open Hands")
            {
                openHandsAnim = a;
            }
            else if (a.name == "Close Hands")
            {
                closeHandsAnim = a;
            }
            else if (a.name == "Eat")
            {
                eatAnim = a;
            }
        }
        StartCoroutine(InitAnim());
    }

    bool animInitted = false;

    IEnumerator InitAnim()
    {
        yield return new WaitForSeconds(0.2f);
        foreach (Animate.KeyFrame kf in walkAnim.keyframes)
        {
            int animeVal = 1;
            if (kf.name.Contains("1"))
                animeVal = 1;
            else if (kf.name.Contains("2"))
                animeVal = 2;
            else if (kf.name.Contains("3"))
                animeVal = 3;
            else if (kf.name.Contains("4"))
                animeVal = 4;
            int highestVal = 0;

            while (walkFrames.Count < animeVal)
            {
                walkFrames.Add(new List<Animate.KeyFrame>());
                walkLegsOnlyFrames.Add(new List<Animate.KeyFrame>());
            }
            walkFrames[animeVal-1].Add(kf);
            if (kf.name.Contains("Leg") || kf.name.Contains("leg") || kf.name.Contains("Foot") || kf.name.Contains("foot"))
                walkLegsOnlyFrames[animeVal - 1].Add(kf);
        }
        foreach (Animate.KeyFrame kf in jawOpenAnim.keyframes)
        {
            jawOpenFrames.Add(kf);
        }
        foreach (Animate.KeyFrame kf in jawCloseAnim.keyframes)
        {
            jawCloseFrames.Add(kf);
        }
        foreach (Animate.KeyFrame kf in standFeetOnlyAnim.keyframes)
        {
            standFeetOnlyFrames.Add(kf);
        }
        foreach (Animate.KeyFrame kf in openHandsAnim.keyframes)
        {
            openHandsFrames.Add(kf);
        }
        foreach (Animate.KeyFrame kf in closeHandsAnim.keyframes)
        {
            closeHandsFrames.Add(kf);
        }
        foreach (Animate.KeyFrame kf in eatAnim.keyframes)
        {
            eatFrames.Add(kf);
        }
        if (climbAnim != null)
        {
            foreach (Animate.KeyFrame kf in climbAnim.keyframes)
            {
                int animeVal = 1;
                if (kf.name.Contains("1"))
                    animeVal = 1;
                else if (kf.name.Contains("2"))
                    animeVal = 2;
                else if (kf.name.Contains("3"))
                    animeVal = 3;
                else if (kf.name.Contains("4"))
                    animeVal = 4;
                int highestVal = 0;

                while (climbFrames.Count < animeVal)
                {
                    climbFrames.Add(new List<Animate.KeyFrame>());
                }
                climbFrames[animeVal - 1].Add(kf);
            }
        }
        foreach (Animate.KeyFrame kf in standAnim.keyframes)
        {
            standFrames.Add(kf);
        }
        if (topClimbAnim != null)
        {
            foreach (Animate.KeyFrame kf in topClimbAnim.keyframes)
            {
                topClimbFrames.Add(kf);
            }
        }
        animInitted = true;
    }

    [System.NonSerialized]
    public bool walking = false;

    public LayerMask lm;

    List<Vector3> path;

    void OnPathFound(List<QPathFinder.Node> nodes)
    {
        //Debug.Log("Found Path");
        waitingForPathfind = false;
        path = new List<Vector3>();
        if (nodes != null)
        {
            foreach (QPathFinder.Node node in nodes)
            {
                path.Add(node.Position);
            }
        }
    }

    bool climbing = false;
    int climbFrame = 0;

    public Damager leftFootDamager;
    public Damager rightFootDamager;

    public Grabber mouthGrabber;

    public Grabber[] handGrabbers;

    public GameObject[] Hands;

    bool closedHands = false;

    int c = 0;

    Human targetHuman;

    private void FixedUpdate()
    {
        if (animInitted == false)
            return;
        if (killed)
            return;
        if (modeChangeCooldown > 0)
            modeChangeCooldown--;

        //Debug.Log("Jaw Open = " + jawOpen);

        int topAnimatePriority = 0;

        c++;
        c %= 10000;

        if (c % 30 == 0)
        {
            foreach (Human h in FindObjectsByType<Human>(FindObjectsSortMode.None))
                if (targetHuman == null || !targetHuman.Alive() || (h != null && h.Alive() && Vector3.Distance(transform.position + transform.up * height * 0.85f, h.transform.position) < Vector3.Distance(transform.position + transform.up * height * 0.85f, targetHuman.transform.position)))
                    targetHuman = h;
        }

        // If player is closer than go after the player
        if (c % 3 == 0 && targetHuman != Player.instance.GetComponent<Human>() && targetHuman != null)
            if (Vector3.Distance(Player.instance.transform.position, transform.position + transform.up * height * 0.85f) < Vector3.Distance(transform.position + transform.up * height * 0.85f, targetHuman.transform.position))
                targetHuman = Player.instance.GetComponent<Human>();

        target = targetHuman != null ? targetHuman.transform.position : target;

        if (targetHuman != null && targetHuman.grappled && targetHuman.grabber.GetComponentInParent<Titan>() == this)
        {
            mode = -1;
            modeChangeCooldown = 20;
        }

        Vector3 rayStart = transform.position + Vector3.up * height * 0.85f;


        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

        foreach (GameObject eye in eyes)
        {
            eye.transform.rotation = Quaternion.RotateTowards(eye.transform.rotation, Quaternion.LookRotation(target - eye.transform.position, Vector3.up) * Quaternion.AngleAxis(90, Vector3.right), eyeSpeed);
            float eyeAngle = Vector3.Angle(actualHead.transform.forward, eye.transform.up);
            //Debug.Log("EyeAngle = " + eyeAngle);
            float maxEyeAngle = 30;
            if (eyeAngle > maxEyeAngle)
            {
                eye.transform.rotation = Quaternion.RotateTowards(eye.transform.rotation, Quaternion.LookRotation(actualHead.transform.forward, Vector3.up) * Quaternion.AngleAxis(90, Vector3.right), eyeAngle - maxEyeAngle);
            }
            //eye.transform.eulerAngles += (Vector3.right * Vector3.SignedAngle(eye.transform.up, (target - eye.transform.position).normalized, eye.transform.right) + Vector3.up * Vector3.SignedAngle(eye.transform.up, (target - eye.transform.position).normalized, Vector3.up)).normalized * eyeSpeed;
        }

        //Debug.Log("Mode = " + mode);

        if (c % 10 == 0 && Random.Range(0, 100) < 5)
            mode = -2;

        if (mode == -2)
        {
            if (c % 10 == 0 && Random.Range(0, 100) < 20)
                mode = 0;
        }
        else if (mode == -1)
        {
            if (targetHuman == null)
            {
                mode = 0;
                return;
            }
            bool feetComplete = false;
            (topAnimatePriority, feetComplete) = RunAnimation(standFeetOnlyFrames, 2, topAnimatePriority, standFeetOnlyAnim);

            bool inMouth = targetHuman.grabber == mouthGrabber;
            bool inHand = false;
            int whichHand = -1;
            for (int i = 0; i < handGrabbers.Length; i++)
            {
                if (targetHuman.grabber == handGrabbers[i])
                {
                    //targetHuman.grabber = handGrabbers[i];
                    //handGrabbers[i].Add(targetHuman.GetComponent<Destroyable>());
                    whichHand = i;
                    inHand = true;
                    break;
                }
            }
            //bool inHand = !inMouth && Vector3.Distance(targetHuman.transform.position == handGrabbers[0] || targetHuman.grabber == handGrabbers[1];


            if (inHand)
            {
                //Debug.Log("In Hand");
                if (!closedHands)
                {
                    bool handComplete;
                    (topAnimatePriority, handComplete) = RunAnimation(closeHandsFrames, 5, topAnimatePriority, closeHandsAnim);
                    if (handComplete)
                        closedHands = true;
                }

                for (int i = 0; i < handGrabbers.Length; i++)
                    handGrabbers[i].Enable(false);

                if (eatStage == 0)
                {
                    bool complete;
                    (topAnimatePriority, complete) = RunAnimation(eatFrames, 6, topAnimatePriority, eatAnim);
                    (topAnimatePriority, complete) = RunAnimation(jawCloseFrames, 10, topAnimatePriority, jawCloseAnim);

                    Vector3 diff = (transform.position + transform.up * height * 0.85f + transform.forward * height * 0.21f - mouth.transform.position);
                    mouth.transform.position += diff.normalized * Mathf.Min(attackSpeed, diff.magnitude); // Mouth upwards

                    Vector3 diff2 = (transform.position + transform.up * height * 0.8f + transform.forward * height * 0.4f - handGrabbers[whichHand].transform.position);
                    hands[whichHand].transform.position += diff2.normalized * Mathf.Min(attackSpeed, diff2.magnitude); // Hand upwards

                    int otherHand = whichHand == 0 ? 1 : 0;

                    Vector3 diff3 = (transform.position + transform.up * height * 0.3f + transform.right * (hands[otherHand].name.Contains('L') ? -1 : 1) * 0.25f - hands[otherHand].transform.position);
                    hands[otherHand].transform.position += diff3.normalized * Mathf.Min(attackSpeed / 4, diff3.magnitude); // Return other hand to side
                    //Debug.Log("Diff=" + diff + ", Diff2=" + diff2);

                    if (diff.magnitude + diff2.magnitude < 0.02f && Random.Range(0, 60) < 2)
                        eatStage = 1;
                }
                else if (eatStage == 1)
                {
                    bool complete;
                    (topAnimatePriority, complete) = RunAnimation(jawOpenFrames, 10, topAnimatePriority, jawOpenAnim);

                    Vector3 offset = actualHead.transform.up * mouthHeight;
                    Vector3 diff = (target + offset - mouth.transform.position);
                    mouth.transform.position += diff.normalized * Mathf.Min(attackSpeed / 6, diff.magnitude); // Bring mouth to hand

                    //for (int i = 0; i < handGrabbers.Length; i++) {
                    //    if (targetHuman.grabber == handGrabbers[i])
                    {
                        diff = (mouthGrabber.transform.position - handGrabbers[whichHand].transform.position);
                        hands[whichHand].transform.position += diff.normalized * Mathf.Min(attackSpeed / 4, diff.magnitude); // Bring hand to mouth

                        //Debug.Log("Diff = " + diff);
                    }
                    //}
                }
            }
            else
            {
                eatStage = 0;
                //Debug.Log("Not In Hand");
                bool standComplete2;
                (topAnimatePriority, standComplete2) = RunAnimation(standFrames, 5, topAnimatePriority, standAnim);

                if (closedHands)
                {
                    bool handComplete;
                    (topAnimatePriority, handComplete) = RunAnimation(openHandsFrames, 5, topAnimatePriority, openHandsAnim);
                    if (handComplete)
                        closedHands = false;
                }

                if (targetHuman != null && targetHuman.Alive() && targetHuman.grabber == mouthGrabber)
                {
                    jawOpen = true;

                    //Debug.Log("In Mouth");

                    bool complete;
                    (topAnimatePriority, complete) = RunAnimation(jawCloseFrames, 10, topAnimatePriority, jawCloseAnim);
                    if (complete)
                        (topAnimatePriority, complete) = RunAnimation(jawOpenFrames, 10, topAnimatePriority, jawOpenAnim);
                }
                else
                {
                    mouthGrabber.Enable(false);
                    //mouthCollider.enabled = true;
                    bool complete;
                    (topAnimatePriority, standComplete2) = RunAnimation(standFrames, 5, topAnimatePriority, standAnim);
                    (topAnimatePriority, complete) = RunAnimation(jawCloseFrames, 10, topAnimatePriority, jawCloseAnim);
                    if (complete)
                        jawOpen = false;
                    mode = 0;
                }
            }
        }
        else if (mode == 0)
        {
            bool attacking = targetHuman != null && Vector3.Distance(transform.position + transform.up * 0.5f, target) < range * 1.6f;

            if (!attacking && jawOpen)
            {
                eatStage = 0;
                for (int i = 0; i < handGrabbers.Length; i++)
                    handGrabbers[i].Enable(false);

                mouthGrabber.Enable(false);
                //mouthCollider.enabled = true;
                bool complete;
                (topAnimatePriority, complete) = RunAnimation(jawCloseFrames, 10, topAnimatePriority, jawCloseAnim);
                if (complete)
                    jawOpen = false;
            }
            else if (attacking && !jawOpen)
            {
                eatStage = 0;
                for (int i = 0; i < handGrabbers.Length; i++)
                    if (handGrabbers[i].GetComponentInParent<Limb>().IsUsable())
                        handGrabbers[i].Enable(true);

                if (mouthGrabber.GetComponentInParent<Limb>().IsUsable())
                    mouthGrabber.Enable(true);
                //mouthCollider.enabled = false;
                bool complete;
                (topAnimatePriority, complete) = RunAnimation(jawOpenFrames, 10, topAnimatePriority, jawOpenAnim);
                if (complete)
                    jawOpen = true;
            }

            if (attacking && Vector3.Distance(transform.position + transform.up * 0.5f, target) < range * 0.9f && modeChangeCooldown < 1)
            {
                mode = 1;
                modeChangeCooldown = 20;
            }

            if (closedHands)
            {
                bool handComplete;
                (topAnimatePriority, handComplete) = RunAnimation(openHandsFrames, 5, topAnimatePriority, openHandsAnim);
                if (handComplete)
                    closedHands = false;
            }

            Vector3 move = (target - transform.position).normalized;
            RaycastHit hit;
            if (Physics.SphereCast(rayStart, width / 2, target - rayStart, out hit, followDistance, lm))
            {
                if (Vector3.Distance(hit.point, rayStart) > (Vector3.Distance(target, rayStart) - 0.5f))
                {
                    //Debug.Log("Going to target");
                    // go for the target
                }
                else
                {
                    //Debug.Log("Pathfinding");
                    move = Vector3.zero;
                    int startNode = Map.instance.fancyPF.FindNearestNode(transform.position, maxClimbHeight, climbTax);
                    int endNode = Map.instance.fancyPF.FindNearestNode(target, maxClimbHeight, climbTax);
                    //Debug.Log("startNode: " + startNode + ", endNode: " + endNode);
                    if (path == null || (path.Count > 0 && Map.instance.fancyPF.FindNearestNode(path[path.Count - 1], maxClimbHeight, climbTax) != endNode) || path.Count < 1)
                    {
                        if (!waitingForPathfind && Time.realtimeSinceStartupAsDouble - lastPathfind > 5)
                        {
                            //Debug.Log("Looking for path");
                            lastPathfind = Time.realtimeSinceStartupAsDouble;
                            waitingForPathfind = true;
                            Map.instance.fancyPF.FindShortestPathOfNodes(startNode, endNode, maxClimbHeight, climbTax, OnPathFound);
                        }
                    }

                    if (path != null && path.Count > 0)
                    {
                        if (Vector3.Distance(transform.position, path[0]) < pathTol)
                        {
                            path.Remove(path[0]);
                        }
                        if (path.Count > 0)
                        {
                            move = (path[0] - transform.position).normalized;
                            Vector3 previousPoint = transform.position;
                            foreach (Vector3 point in path)
                            {
                                Debug.DrawLine(previousPoint, point);
                                previousPoint = point;
                            }
                        }
                        else
                        {
                            move = (target - transform.position).normalized;
                            // go to target
                        }
                    }
                }
            }
            else
            {
                // go for the target
                //Debug.Log("Going to target");
            }
            //Vector3 move = (target - transform.position).normalized;

            Vector3 rayStart2 = transform.position + Vector3.up * height * 0.5f;
            Debug.DrawRay(rayStart2, transform.forward * climbRange, Color.red);
            if (move.y > stepHeight && Physics.SphereCast(rayStart2, width, transform.forward, out hit, climbRange, lm))
            {
                if (hit.collider.gameObject.tag == "Map")
                {
                    //Debug.Log("Climbing " + hit.collider.gameObject.name);
                    climbing = true;
                }
            }
            else
            {
                climbing = false;
            }


            if (!climbing && move.magnitude > 0 && Vector3.Dot(rb.velocity, move) < maxMoveSpeed)
            {
                if (leftFootDamager.GetComponentInParent<Limb>().IsUsable())
                    leftFootDamager.Enable(true);
                if (rightFootDamager.GetComponentInParent<Limb>().IsUsable())
                    rightFootDamager.Enable(true);
                if (Vector3.Angle(HorizontalComponent(move), transform.forward) < 15)
                {
                    walking = true;
                    rb.AddForce((HorizontalComponent(transform.forward).normalized * moveSpeed - HorizontalComponent(rb.velocity)).normalized * moveSpeed);
                }
                else
                {
                    walking = false;
                }
                if (Vector3.Angle(HorizontalComponent(transform.forward), HorizontalComponent(move)) > 2)
                {
                    Vector3 turnTorque = ((Vector3.up * Vector3.SignedAngle(HorizontalComponent(transform.forward), HorizontalComponent(move), Vector3.up)).normalized * maxTurnSpeed - rb.angularVelocity).normalized * turnSpeed;
                    rb.AddTorque(turnTorque);
                    //Debug.Log(turnTorque);
                    if (rb.angularVelocity.magnitude > maxTurnSpeed)
                        rb.angularVelocity = rb.angularVelocity.normalized * maxTurnSpeed;
                }
                else
                {
                    //Debug.Log("Stopped Spinning");
                    rb.angularVelocity = Vector3.zero;
                }
            }
            else
            {
                //Debug.Log("Stopped Walking");
                rb.AddForce(-HorizontalComponent(rb.velocity) * friction);
                walking = false;
                leftFootDamager.Enable(false);
                rightFootDamager.Enable(false);
            }

            if (climbing)
            {
                rb.AddForce((Vector3.up * (maxMoveSpeed * 0.5f - rb.velocity.y)).normalized * rb.mass * 12f);
                if (topAnimatePriority <= 3)
                {
                    bool allComplete = true;
                    topAnimatePriority = Mathf.Max(3, topAnimatePriority);
                    //Debug.Log("Titan climb " + climbFrame);
                    (topAnimatePriority, allComplete) = RunAnimation(climbFrames[climbFrame], 3, topAnimatePriority, climbAnim);

                    if (allComplete)
                    {
                        //foreach (Animate.KeyFrame kf in walkFrames[walkFrame])
                        //    kf.reset();
                        climbFrame++;
                        climbFrame %= climbFrames.Count;
                    }
                }
            }
            if (walking)
            {
                if (leftFootDamager.GetComponentInParent<Limb>().usable)
                    leftFootDamager.Enable(true);
                else
                    leftFootDamager.Enable(false);

                if (leftFootDamager.GetComponentInParent<Limb>().usable)
                    rightFootDamager.Enable(true);
                else
                    rightFootDamager.Enable(false);

                if (topAnimatePriority <= 1)
                {
                    bool allComplete = true;
                    topAnimatePriority = Mathf.Max(1, topAnimatePriority);
                    //Debug.Log("Titan walk " + walkFrame + " first frame: " + walkFrames[walkFrame][0].name);
                    (topAnimatePriority, allComplete) = RunAnimation(attacking ? walkLegsOnlyFrames[walkFrame] : walkFrames[walkFrame], 1, topAnimatePriority, walkAnim);

                    if (allComplete)
                    {
                        //foreach (Animate.KeyFrame kf in walkFrames[walkFrame])
                        //    kf.reset();
                        if (walkFrame == 0)
                            stepSound.play(leftFootDamager.transform.position);
                        if (walkFrame == 2)
                            stepSound.play(rightFootDamager.transform.position);
                        walkFrame++;
                        walkFrame %= walkFrames.Count;
                    }
                }
            }
            else
            {

                leftFootDamager.Enable(false);
                rightFootDamager.Enable(false);
            }

            if (attacking)
                ReachForTarget(topAnimatePriority);

        }
        else if (mode == 1) // Bite and grab attack
        {
            bool feetComplete = false;
            (topAnimatePriority, feetComplete) = RunAnimation(standFeetOnlyFrames, 2, topAnimatePriority, standFeetOnlyAnim);
            if (feetComplete)
            {
                walkAnim.ResetAll();

                if (Vector3.Distance(target, transform.position) > range && modeChangeCooldown == 0)
                {
                    mode = 0;
                    modeChangeCooldown = 20;
                }
                else if (Vector3.Distance(target, transform.position + transform.up * height / 2) < range)
                {
                    ReachForTarget(topAnimatePriority);
                }
                else if (jawOpen)
                {
                    for (int i = 0; i < handGrabbers.Length; i++)
                        handGrabbers[i].Enable(false);

                    mouthGrabber.Enable(false);
                    //mouthCollider.enabled = true;
                    bool complete;
                    (topAnimatePriority, complete) = RunAnimation(jawCloseFrames, 10, topAnimatePriority, jawCloseAnim);
                    if (complete)
                        jawOpen = false;

                    for (int i = 0; i < handGrabbers.Length; i++)
                        handGrabbers[i].Enable(false);
                }
            }
        }

        bool standComplete;
        (topAnimatePriority, standComplete) = RunAnimation(standFrames, 0, topAnimatePriority, standAnim);
        
    }

    void ReachForTarget(int topAnimatePriority)
    {

        eatStage = 0;
        //mouthCollider.enabled = false;
        if (mouthGrabber.GetComponentInParent<Limb>().IsUsable())
            mouthGrabber.Enable(true);
        //Vector3 offset = Quaternion.LookRotation((target - mouth.transform.position).normalized, transform.up) * Vector3.up * mouthHeight;
        Vector3 offset = actualHead.transform.up * mouthHeight;
        Vector3 diff = (target + offset - mouth.transform.position);
        mouth.transform.position += diff.normalized * Mathf.Min(0.005f, diff.magnitude);
        if (!jawOpen)
        {
            bool complete;
            (topAnimatePriority, complete) = RunAnimation(jawOpenFrames, 10, topAnimatePriority, jawOpenAnim);
            if (complete)
                jawOpen = true;
        }

        for (int i = 0; i < handGrabbers.Length; i++)
        {
            if (handGrabbers[i].GetComponentInParent<Limb>().IsUsable())
                handGrabbers[i].Enable(true);
            diff = (target - handGrabbers[i].transform.position);
            hands[i].transform.position += diff.normalized * Mathf.Min(attackSpeed, diff.magnitude);
        }
    }

    public float mouthHeight = 0.1f;
    bool jawOpen = false;
    public GameObject actualHead;

    private (int, bool) RunAnimation(List<Animate.KeyFrame> frames, int priority, int topPriority, Animate anim, int frameNum = -1)
    {
        bool allComplete = true;

        if (topPriority <= priority)
        {
            anim.enableAnim = true;
            topPriority = Mathf.Max(priority, topPriority);
            foreach (Animate.KeyFrame kf in frames)
            {
                if (frameNum == -1 || kf.name.Contains(frameNum.ToString()))
                {
                    if (!kf.isMoving())
                    {
                        kf.Move();
                    }
                    if (!kf.isComplete())
                    {
                        allComplete = false;
                    }
                }
            }
        }
        else
        {
            anim.enableAnim = false;
        }

        return (topPriority, allComplete);
    }

    private Vector3 HorizontalComponent(Vector3 v)
    {
        return Vector3.ProjectOnPlane(v, Vector3.up);
    }

    // Update is called once per frame
    void Update()
    {
        if (!Application.isPlaying && forceStandFrame)
        {
            Debug.Log("Forcing stand");
            if (walkAnim == null)
            {
                foreach (Animate a in animatePack.GetComponents<Animate>())
                {
                    if (a.name == "Stand")
                    {
                        standAnim = a;
                    }
                }
            }
            if (!walkAnim.initted)
                walkAnim.Init();
            if (standFrames.Count == 0)
            {

                foreach (Animate.KeyFrame kf in walkAnim.keyframes)
                {
                    int animeVal = 1;
                    if (kf.name.Contains("1"))
                        animeVal = 1;
                    else if (kf.name.Contains("2"))
                        animeVal = 2;
                    else if (kf.name.Contains("3"))
                        animeVal = 3;
                    else if (kf.name.Contains("4"))
                        animeVal = 4;

                    while (walkFrames.Count < animeVal)
                    {
                        walkFrames.Add(new List<Animate.KeyFrame>());
                    }
                    walkFrames[animeVal - 1].Add(kf);
                }
            }

            foreach (Animate.KeyFrame frame in standFrames)
            {
                frame.Teleport();
            }
        }
    }
}
