using DitzelGames.FastIK;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

//[ExecuteInEditMode]
public class Player : Human
{
    public GameObject sparks;
    int flopping = 0;
    //public bool forceStandFrame = false;
    public TextMeshProUGUI fancyText;
    public static Player instance;
    public LayerMask lm;
    public LayerMask camLm;
    public GameObject head_bone;
    public GameObject cameraAxis;
    public Camera cam;

    bool useStandAnim;

    public float moveSpeed = 5;
    public float maxMoveSpeed = 10;
    public float friction = 0.5f;
    public float jumpPower = 5;
    public float groundScan = 0.2f;

    [System.NonSerialized]
    public float camDist = 35f;

    float camZ = 0;
    float camX = 0;
    
    [System.NonSerialized]
    public Rigidbody rb;

    Animate standAnim;
    List<Animate.KeyFrame> standFrames = new List<Animate.KeyFrame>();

    Animate walkAnim;
    Dictionary<string, Animate.KeyFrame> walkFrames = new Dictionary<string, Animate.KeyFrame>();
    int walkFrame = 0;
    
    Animate odmAnim;
    Dictionary<string, Animate.KeyFrame> odmFrames = new Dictionary<string, Animate.KeyFrame>();

    Animate slideAnim;
    Dictionary<string, Animate.KeyFrame> slideFrames = new Dictionary<string, Animate.KeyFrame>();

    Animate hangAnim;
    Dictionary<string, Animate.KeyFrame> hangFrames = new Dictionary<string, Animate.KeyFrame>();

    Animate spinAttackAnim;
    Dictionary<string, Animate.KeyFrame> spinAttackFrames = new Dictionary<string, Animate.KeyFrame>();

    Animate flailAnim;
    Dictionary<string, Animate.KeyFrame> flailFrames = new Dictionary<string, Animate.KeyFrame>();

    List<Animate> swordSwingAnim = new List<Animate>();
    List<List<List<Animate.KeyFrame>>> swordSwing = new List<List<List<Animate.KeyFrame>>>();

    public ODM leftODM;
    public ODM rightODM;

    public Sword leftSword;
    public Sword rightSword;

    Vector2 odmOffset = Vector2.zero;

    float speed = 0;

    public SoundManager.Sound odmFire;
    public SoundManager.Sound odmPull;
    public SoundManager.Sound odmRetract;
    public SoundManager.Sound odmFan;
    public SoundManager.Sound swordWhoosh;
    public SoundManager.Sound swordSlice;
    public SoundManager.Sound swordCrit;
    public SoundManager.Sound swordDraw;
    public SoundManager.Sound hit;
    public SoundManager.Sound boneBreak;
    public SoundManager.Sound hurt;

    public Damager attacker;
    public Damager spinAttacker;

    //public AudioClip odmFire;
    //public AudioClip odmPull;
    //public AudioClip odmRetract;

    // Start is called before the first frame update
    new void Start()
    {
        base.maxHP = 20;
        base.Start();

        instance = this;
        leftODM.player = this;
        rightODM.player = this;
        foreach (Animate anime in GetComponents<Animate>())
        {
            if (anime.name == "Walk")
                walkAnim = anime;
            if (anime.name == "Stand")
            {
                useStandAnim = true;
                standAnim = anime;
            }
            if (anime.name == "ODM")
                odmAnim = anime;
            if (anime.name == "Slide")
                slideAnim = anime;
            if (anime.name == "Flail")
                flailAnim = anime;
            if (anime.name == "Hang")
                hangAnim = anime;
            if (anime.name == "Spin Attack")
                spinAttackAnim = anime;
            if (anime.name.Contains("Sword Swing"))
            {
                int value = 1;
                if (anime.name.Contains("1"))
                    value = 1;
                else if (anime.name.Contains("2"))
                    value = 2;
                else if (anime.name.Contains("3"))
                    value = 3;
                else if (anime.name.Contains("4"))
                    value = 4;

                //Debug.Log("Adding sword swings");
                while (swordSwing.Count < value)
                    swordSwing.Add(new List<List<Animate.KeyFrame>>());
                //Debug.Log("Adding sword swing anims");
                while (swordSwingAnim.Count < value)
                    swordSwingAnim.Add(null);

                swordSwingAnim[value - 1] = anime;
            }
        }
        rb = GetComponent<Rigidbody>();
        StartCoroutine(InitAnim());
        Cursor.lockState = CursorLockMode.Locked;
    }

    IEnumerator InitAnim()
    {
        yield return new WaitForSeconds(0.2f);
        if (!useStandAnim)
        {
            foreach (Animate.KeyFrame kf in walkAnim.keyframes)
            {
                if (kf.name.Contains(" Stand") || kf.name.Contains(" stand"))
                    standFrames.Add(kf);
                else
                    walkFrames.Add(kf.name, kf);
            }
        } else
        {
            walkFrames = new Dictionary<string, Animate.KeyFrame>();
            foreach (Animate.KeyFrame kf in walkAnim.keyframes)
            {
                walkFrames.Add(kf.name, kf);
            }
            standFrames = new List<Animate.KeyFrame>();
            foreach (Animate.KeyFrame kf in standAnim.keyframes)
            {
                standFrames.Add(kf);
            }
        }
        foreach (Animate.KeyFrame kf in spinAttackAnim.keyframes)
        {
            spinAttackFrames.Add(kf.name, kf);
        }
        foreach (Animate.KeyFrame kf in flailAnim.keyframes)
        {
            flailFrames.Add(kf.name, kf);
        }
        foreach (Animate.KeyFrame kf in hangAnim.keyframes)
        {
            hangFrames.Add(kf.name, kf);
        }
        foreach (Animate.KeyFrame kf in slideAnim.keyframes)
        {
            slideFrames.Add(kf.name, kf);
        }
        foreach (Animate.KeyFrame kf in odmAnim.keyframes)
        {
            odmFrames.Add(kf.name, kf);
        }
        foreach (Animate anime in swordSwingAnim)
        {
            int animeVal = 1;
            if (anime.name.Contains("1"))
                animeVal = 1;
            else if (anime.name.Contains("2"))
                animeVal = 2;
            else if (anime.name.Contains("3"))
                animeVal = 3;
            else if (anime.name.Contains("4"))
                animeVal = 4;
            int highestVal = 0;
            foreach (Animate.KeyFrame kf in anime.keyframes)
            {
                int value = 1;
                if (kf.name.Contains("1"))
                    value = 1;
                else if (kf.name.Contains("2"))
                    value = 2;
                else if (kf.name.Contains("3"))
                    value = 3;
                else if (kf.name.Contains("4"))
                    value = 4;
                else if (kf.name.Contains("5"))
                    value = 5;
                if (value > highestVal)
                    highestVal = value;

                //Debug.Log("Adding sword swing keyframes");
                while (swordSwing[animeVal - 1].Count < value)
                    swordSwing[animeVal - 1].Add(new List<Animate.KeyFrame>());

                swordSwing[animeVal - 1][value - 1].Add(kf);
            }

        }
    }

    int shakingFree = 0;

    // Update is called once per frame
    void Update()
    {
        {
            if (Input.GetKey(KeyCode.Equals))
            {
                camDist /= 1.01f;
            } else if (Input.GetKey(KeyCode.Minus))
            {
                camDist *= 1.01f;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                if (Time.timeScale != 0f)
                    Time.timeScale = 0f;
                else
                    Time.timeScale = 1;
            }


            cameraAxis.transform.eulerAngles = new Vector3(camX, camZ, 0);

            if (!Alive())
                Kill();
            else if (Time.timeScale > 0.01f)
            {
                if (grappled && Input.GetKeyDown(KeyCode.Space)) {
                    if (shakingFree == 0)
                    {
                        // Tries to shake free
                        //Debug.Log("Trying to shake free");
                        shakingFree = 4;
                        if (grabber != null && Random.Range(0, 100) < grabber.escapeLikelyhood * 100)
                        {
                            grabber.Free(this);
                            rb.AddForce(Vector3.up * jumpPower * 1.5f, ForceMode.Impulse);
                            grabCooldown = 10;
                        }
                    }
                }
                if (Input.GetKey(KeyCode.X))
                {
                    if (Input.GetKeyDown(KeyCode.X))
                    {
                        leftODM.Retract();
                        rightODM.Retract();
                    }
                    leftODM.transform.rotation = Quaternion.LookRotation(Vector3.down);
                    rightODM.transform.rotation = Quaternion.LookRotation(Vector3.down);
                    //leftODM.transform.eulerAngles = new Vector3(90, 0, 0);
                    //rightODM.transform.eulerAngles = new Vector3(90, 0, 0);
                    if (!leftODM.hookHit && !leftODM.retracting)
                        leftODM.Fire(KeyCode.X);
                    if (!rightODM.hookHit && !rightODM.retracting)
                        rightODM.Fire(KeyCode.X);
                }
                else if (Input.GetKey(KeyCode.E))
                {
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        leftODM.Retract();
                        rightODM.Retract();
                    }
                    leftODM.transform.rotation = Quaternion.LookRotation(Vector3.up, transform.up);
                    rightODM.transform.rotation = Quaternion.LookRotation(Vector3.up, transform.up);
                    //leftODM.transform.eulerAngles = new Vector3(-90, 0, 0);
                    //rightODM.transform.eulerAngles = new Vector3(-90, 0, 0);
                    ;

                    if (leftODM.findNearest(-90, 0) && !leftODM.hookHit && !leftODM.retracting)
                        leftODM.Fire(KeyCode.E);
                    if (rightODM.findNearest(0, 90) && !rightODM.hookHit && !rightODM.retracting)
                        rightODM.Fire(KeyCode.E);
                }
                else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.W))
                {
                    if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.W))
                    {
                        leftODM.Retract();
                        rightODM.Retract();
                    }
                    leftODM.transform.eulerAngles = cam.transform.eulerAngles;
                    rightODM.transform.eulerAngles = cam.transform.eulerAngles;


                    if (leftODM.findNearest(-90, 0) && !leftODM.hookHit && !leftODM.retracting)
                        leftODM.Fire(KeyCode.W);
                    if (rightODM.findNearest(0, 90) && !rightODM.hookHit && !rightODM.retracting)
                        rightODM.Fire(KeyCode.W);
                }
                else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.S))
                {
                    if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.S))
                    {
                        leftODM.Retract();
                        rightODM.Retract();
                    }
                    leftODM.transform.eulerAngles = cam.transform.eulerAngles + cam.transform.up * 180;
                    rightODM.transform.eulerAngles = cam.transform.eulerAngles + cam.transform.up * 180;


                    if (leftODM.findNearest(-90, 0) && !leftODM.hookHit && !leftODM.retracting)
                        leftODM.Fire(KeyCode.S);
                    if (rightODM.findNearest(0, 90) && !rightODM.hookHit && !rightODM.retracting)
                        rightODM.Fire(KeyCode.S);
                }
                else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.D))
                {
                    if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.D))
                    {
                        leftODM.Retract();
                        rightODM.Retract();
                    }
                    leftODM.transform.eulerAngles = cam.transform.eulerAngles + cam.transform.up * 90;
                    rightODM.transform.eulerAngles = cam.transform.eulerAngles + cam.transform.up * 90;


                    if (leftODM.findNearest(-90, 0) && !leftODM.hookHit && !leftODM.retracting)
                        leftODM.Fire(KeyCode.D);
                    if (rightODM.findNearest(0, 90) && !rightODM.hookHit && !rightODM.retracting)
                        rightODM.Fire(KeyCode.D);
                }
                else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.A))
                {
                    if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.A))
                    {
                        leftODM.Retract();
                        rightODM.Retract();
                    }
                    leftODM.transform.eulerAngles = cam.transform.eulerAngles + cam.transform.up * (-90);
                    rightODM.transform.eulerAngles = cam.transform.eulerAngles + cam.transform.up * (-90);


                    if (leftODM.findNearest(-90, 0) && !leftODM.hookHit && !leftODM.retracting)
                        leftODM.Fire(KeyCode.A);
                    if (rightODM.findNearest(0, 90) && !rightODM.hookHit && !rightODM.retracting)
                        rightODM.Fire(KeyCode.A);
                }
                else
                {
                    leftODM.transform.eulerAngles = new Vector3(camX + odmOffset.x, camZ + odmOffset.y, 0);
                    rightODM.transform.eulerAngles = new Vector3(camX + odmOffset.x, camZ - odmOffset.y, 0);
                }
            }

            RaycastHit hit;
            if (Physics.Raycast(cameraAxis.transform.position - cameraAxis.transform.forward * 0.01f, (-cameraAxis.transform.forward*35+Vector3.up*7.5f).normalized, out hit, camDist * 0.6f/35, camLm))
            {
                float dist = cameraAxis.transform.InverseTransformPoint(hit.point).z;
                cam.transform.localPosition = (Vector3.forward - Vector3.up * 7.5f/35).normalized * dist * 0.9f;
            }
            else
            {
                cam.transform.localPosition = -Vector3.forward * camDist + cam.transform.parent.InverseTransformDirection(Vector3.up) * camDist * 7.5f/35;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (Cursor.lockState == CursorLockMode.Locked)
                    Cursor.lockState = CursorLockMode.None;
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }

            if (Cursor.lockState == CursorLockMode.Locked)
            {
                camZ += Input.GetAxis("Mouse X");
                camX -= Input.GetAxis("Mouse Y");
            }


            if (Mathf.Abs(Input.mouseScrollDelta.y) > 0)
            {
                if (Input.GetKey(KeyCode.LeftAlt))
                {
                    odmOffset = odmOffset + Vector2.right * Input.mouseScrollDelta.y * 2;
                }
                else
                {
                    odmOffset = odmOffset + Vector2.up * Input.mouseScrollDelta.y * 2;
                }
                odmOffset = new Vector2(Mathf.Clamp(odmOffset.x, -45, 45), Mathf.Clamp(odmOffset.y, -90, 0));
            }

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                leftODM.Retract();
                rightODM.Retract();
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                if ((leftODM.hook != null && leftODM.hookHit) || (rightODM.hook != null && rightODM.hookHit))
                {
                    rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
                    if (leftODM.hook != null)
                        rb.AddForce((leftODM.hook.transform.position - transform.position).normalized * jumpPower * 0.8f, ForceMode.Impulse);
                    if (rightODM.hook != null)
                        rb.AddForce((rightODM.hook.transform.position - transform.position).normalized * jumpPower * 0.8f, ForceMode.Impulse);
                    leftODM.Retract();
                    rightODM.Retract();
                    if (flopping == 0)
                    {
                        AddFlop(1);
                    }
                    else
                    {
                        rb.angularVelocity += (new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1))) * rb.velocity.magnitude;
                    }
                }
            }
        }
    }

    //[System.NonSerialized]
    public int collidersHit = 0;
    int swingFrame = -1;
    int swingEnd = 0;
    int swingAnim = 0;

    int spinAttack = 0;
    int specialAttackCooldown = 0;

    [System.NonSerialized]
    public bool pulling = false;

    int c = 0;

    bool pullingLast = false;
    
    private new void FixedUpdate()
    {
        base.FixedUpdate();
        int topAnimatePriority = 0;
        c++;
        c %= 10000;

        if (flopping > 0)
        {
            rb.freezeRotation = false;
        } else
        {
            rb.freezeRotation = true;
        }

        fancyText.text = "HP: " + Mathf.Round(HP) + "/" + maxHP;

        if (!Alive())
        {
            pulling = false;
            leftSword.swinging = false;
            rightSword.swinging = false;
            spinAttacker.Enable(false);
            attacker.Enable(false);
            return;
        }

        if (Input.GetKey(leftODM.pullButton))
        {
            leftODM.Pull();
        }
        if (Input.GetKey(rightODM.pullButton))
        {
            rightODM.Pull();
        }
        if (c % 5 == 0)
        {
            pulling = false;

            if (specialAttackCooldown > 0)
                specialAttackCooldown--;

            if (spinAttack > 0)
            {
                spinAttack--;
                if (spinAttack == 0)
                {
                    leftSword.swinging = false;
                    rightSword.swinging = false;
                    spinAttacker.Enable(false);
                }
            }
            if (shakingFree > 0)
            {
                shakingFree--;
                if (shakingFree == 0)
                {
                    leftSword.swinging = false;
                    rightSword.swinging = false;
                    
                    //spinAttacker.Enable(false);
                }
            }
        }

        //Debug.Log("Spin Attack = " + spinAttack);

        if (shakingFree > 1)
        {
            bool allComplete = true;
            (topAnimatePriority, allComplete) = RunAnimation(spinAttackFrames, 10, topAnimatePriority, spinAttackAnim);

            if (shakingFree % 2 == 0)
                swordWhoosh.play(transform.position, gameObject);

            {
                leftSword.swinging = true;
                rightSword.swinging = true;
                //spinAttacker.Enable(true);

                float spinSpeed = Mathf.Clamp((10 - spinAttack) * 10 + 20, 40, 60);

                rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, Quaternion.LookRotation(transform.right, transform.up), spinSpeed));
            }
        }

        if (grappled)
        {

        }
        else
        {
            if ((Input.GetKey(KeyCode.Q) && specialAttackCooldown <= 0) || spinAttack > 0)
            {
                int maxSpinAttack = 20;
                bool allComplete = true;
                (topAnimatePriority, allComplete) = RunAnimation(spinAttackFrames, 10, topAnimatePriority, spinAttackAnim);

                if (spinAttack == 0)
                {
                    spinAttack = maxSpinAttack;
                    specialAttackCooldown = 100;
                }

                if (spinAttack % 2 == 0)
                    swordWhoosh.play(transform.position, gameObject);

                if (allComplete || spinAttack > 0)
                {
                    leftSword.swinging = true;
                    rightSword.swinging = true;
                    spinAttacker.Enable(true);

                    float spinSpeed = Mathf.Clamp((maxSpinAttack - spinAttack) * 10 + 20, 40, 60);

                    rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, Quaternion.LookRotation(transform.right, transform.up), spinSpeed));
                }
            }
            else if (Input.GetMouseButton(0) || swingFrame > -1 || swingEnd > 0)
            {
                Turn(cam.transform.forward, transform.up);
                if (swingFrame == -1 && swingEnd == 0)
                    swingFrame = 0;
                if (swingFrame == 1)
                {
                    if (Vector3.Dot(rb.velocity, HorizontalComponent(cam.transform.forward)) < maxMoveSpeed * 1.5f)
                        rb.AddForce((HorizontalComponent(cam.transform.forward) * moveSpeed * 1.5f - HorizontalComponent(rb.velocity)).normalized * moveSpeed * 1.5f);
                }
                if (swingAnim == 0 && swingFrame >= 1)
                {
                    if (swingFrame == 1)
                        swordWhoosh.play(transform.position, gameObject);
                    leftSword.swinging = false;
                    rightSword.swinging = true;
                    attacker.Enable(true);
                }
                else if (swingAnim == 1 && (swingFrame >= 1 || (swingAnim == -1 && swingEnd > 3)))
                {
                    if (swingFrame == 1 || swingFrame == 2 || swingFrame == 4)
                        swordWhoosh.play(transform.position, gameObject);
                    leftSword.swinging = true;
                    rightSword.swinging = false;
                    attacker.Enable(true);
                }
                else
                {
                    leftSword.swinging = false;
                    rightSword.swinging = false;
                    attacker.Enable(false);
                }
                //Debug.Log("swing " + swingFrame);
                topAnimatePriority = Mathf.Max(10, topAnimatePriority);
                bool allComplete = true;
                (topAnimatePriority, allComplete) = RunAnimation(swordSwing[swingAnim][swingEnd > 0 ? swordSwing[swingAnim].Count - 1 : swingFrame], 10, topAnimatePriority, swordSwingAnim[swingAnim]);

                if (allComplete)
                {
                    if (swingEnd <= 0)
                    {
                        foreach (Animate.KeyFrame kf in swordSwing[swingAnim][swingFrame])
                            kf.reset();
                        swingFrame++;
                        if (swingFrame == swordSwing[swingAnim].Count)
                        {
                            swingEnd = 5;
                            swingFrame = -1;
                        }
                    }
                    else if (swingEnd > 0)
                    {
                        swingEnd--;
                        if (swingEnd == 0)
                        {
                            //foreach (Animate.KeyFrame kf in swordSwing[swingAnim][swordSwing[swingAnim].Count - 1])
                            //    kf.reset();
                            swingAnim++;
                            swingAnim %= swordSwing.Count;
                        }
                    }
                }
            }
            if (Input.GetMouseButton(1))
            {
                if (!leftODM.hookHit && !leftODM.retracting)
                    leftODM.findNearest(-90, 0);
                if (!rightODM.hookHit && !rightODM.retracting)
                    rightODM.findNearest(0, 90);

                leftODM.Fire();
                rightODM.Fire();
            }

            Vector3 wDir = HorizontalComponent(cameraAxis.transform.forward).normalized;
            Vector3 dDir = HorizontalComponent(cameraAxis.transform.right).normalized;

            Vector3 move = ((Input.GetKey(KeyCode.W) ? 1 : (Input.GetKey(KeyCode.S) ? -1 : 0)) * wDir + (Input.GetKey(KeyCode.D) ? 1 : (Input.GetKey(KeyCode.A) ? -1 : 0)) * dDir).normalized;
            if (spinAttack > 0)
            {
                move = (move + cam.transform.forward).normalized;
            }
            //bool odmHanging = false;

            if (leftODM.hookHit || rightODM.hookHit)
            {
                bool a = leftODM.hook != null && leftODM.hookHit;
                bool b = rightODM.hook != null && rightODM.hookHit;

                Vector3 point = transform.position;

                if (a || b)
                {
                    if (a && b)
                        point = (leftODM.hook.transform.position + rightODM.hook.transform.position) / 2;
                    else if (a)
                        point = leftODM.hook.transform.position;
                    else if (b)
                        point = rightODM.hook.transform.position;

                    //odmHanging = true;

                    Vector3 swing = Vector3.Dot(move, wDir) * cam.transform.forward + Vector3.Dot(move, dDir) * cam.transform.right;
                    /*
                    if (Vector3.Dot(swing, (transform.position - point).normalized) > 0.01f)
                    {
                        swing = Vector3.ProjectOnPlane(swing, (point-transform.position).normalized);
                    }
                    */

                    /*
                    if (a)
                    {
                        float leftPullAmount = Mathf.Max(0, Vector3.Dot(swing, (leftODM.hook.transform.position - transform.position).normalized));
                        if (leftPullAmount > 0.1f)
                            leftODM.Pull();
                    }
                    if (b)
                    {
                        float rightPullAmount = Mathf.Max(0, Vector3.Dot(swing, (rightODM.hook.transform.position - transform.position).normalized));
                        if (rightPullAmount > 0.1f)
                            rightODM.Pull();
                    }
                    */

                    swing = Vector3.ProjectOnPlane(swing, (point - transform.position).normalized);

                    if (swing.magnitude > 0.1f)
                        rb.AddForce((swing * moveSpeed - rb.velocity).normalized * moveSpeed * 0.3f);

                    if (Vector3.Angle(transform.forward, swing) > 5)
                        Turn(swing, transform.up);

                    bool complete;
                    (topAnimatePriority, complete) = RunAnimation(hangFrames, 1, topAnimatePriority, hangAnim);
                }

                if (pulling)
                {
                    pullingLast = true;

                    RaycastHit hit;
                    if (rb.velocity.magnitude > 0.5f && Physics.Raycast(transform.position, -transform.up, out hit, groundScan * 1.1f, lm))
                    {
                        // Swinging close to  ground
                        bool completed;
                        //odmAnim.enabled = false;
                        (topAnimatePriority, completed) = RunAnimation(slideFrames, 3, topAnimatePriority, slideAnim);
                        if (!sparks.activeSelf)
                            sparks.SetActive(true);

                        Turn(point - transform.position, Vector3.up, true);
                    }
                    else
                    {
                        if (sparks.activeSelf)
                            sparks.SetActive(false);
                        bool completed;
                        //slideAnim.enabled = false;
                        (topAnimatePriority, completed) = RunAnimation(odmFrames, 2, topAnimatePriority, odmAnim);

                        Turn(point - transform.position, transform.up, false);

                    }
                }
                else
                {
                    if (sparks.activeSelf)
                        sparks.SetActive(false);
                }
            }
            else
            {
                if (sparks.activeSelf)
                    sparks.SetActive(false);
            }

            if (collidersHit > 0)
            {
                if (flopping > 0)
                {
                    flopping--;
                    move = Vector3.zero;
                }
                else
                {
                    if (move.magnitude > 0 && Vector3.Dot(rb.velocity, move) < maxMoveSpeed)
                    {
                        rb.AddForce((move * moveSpeed - HorizontalComponent(rb.velocity)).normalized * moveSpeed);
                        if (Vector3.Angle(HorizontalComponent(transform.forward), move) > 5)
                            Turn(move, Vector3.up);
                        else
                        {
                            rb.angularVelocity = Vector3.zero;
                        }
                    }
                    else
                    {
                        rb.AddForce(-HorizontalComponent(rb.velocity) * friction);
                    }
                }

                if (move.magnitude > 0)
                {
                    bool allComplete = true;
                    if (!useStandAnim)
                    {
                        // OLD SHIT
                        topAnimatePriority = Mathf.Max(1, topAnimatePriority);
                        string[] frameNames = new string[] { "Left Leg Walk " + (walkFrame + 1), "Right Leg Walk " + (walkFrame + 1), "Left Knee Walk " + (walkFrame + 1), "Right Knee Walk " + (walkFrame + 1), "Body Walk " + (walkFrame + 1), "Left Shoulder Walk", "Right Shoulder Walk" };
                        foreach (string fn in frameNames)
                        {
                            if (!walkFrames[fn].isComplete())
                            {
                                //Debug.Log(fn + " Not Complete!!!");
                                allComplete = false;
                                if (!walkFrames[fn].isMoving())
                                {
                                    //Debug.Log(fn + " Not Moving!!!");
                                    walkFrames[fn].Move();
                                }
                            }
                        }
                    }
                    else
                    {
                        (topAnimatePriority, allComplete) = RunAnimation(walkFrames, 3, topAnimatePriority, walkAnim, walkFrame + 1);
                    }
                    if (allComplete)
                    {
                        //foreach (string fn in frameNames)
                        //    walkFrames[fn].reset();
                        walkFrame++;
                        walkFrame %= 4;
                    }
                }
                else
                {
                    // Stationary on the ground
                }

                bool complete;
                (topAnimatePriority, complete) = RunAnimation(standFrames, 0, topAnimatePriority, useStandAnim ? standAnim : walkAnim);

                if (topAnimatePriority == 0)
                {
                    walkFrame = 0;
                    if (flopping == 0)
                        Turn(transform.forward, Vector3.up);
                }

            }
            else
            {
                // In the air
                if (flopping > 1 && Random.Range(1, 9) == 2)
                    flopping--;
                speed = rb.velocity.magnitude;

                if (flopping > 0)
                {
                    bool allComplete = true;

                    (topAnimatePriority, allComplete) = RunAnimation(flailFrames, 0, topAnimatePriority, flailAnim, flailNum + 1);

                    if (allComplete)
                    {
                        int prevFlail = flailNum;
                        while (flailNum == prevFlail)
                        {
                            flailNum = (int)(Random.Range(0, 3));
                        }
                    }
                }
            }


            /*if (c % 20 == 0)
            {
                foreach (Animate.KeyFrame kf in standFrames)
                    kf.reset();
            }*/

            /*
            RaycastHit hit;
            if (Physics.SphereCast(transform.position, 4f, -Vector3.up, out hit, 0.2f, lm))
            {
                if (Vector3.Angle(transform.up, Vector3.up) > 1)
                {
                    //transform.eulerAngles += Quaternion.FromToRotation(transform.up, Vector3.up).eulerAngles * 0.5f;
                    transform.eulerAngles = new Vector3(transform.eulerAngles.x / 2, transform.eulerAngles.y, transform.eulerAngles.z / 2);
                    //transform.eulerAngles -= Mathf.Sign(transform.eulerAngles.x) * Vector3.right + Mathf.Sign(transform.eulerAngles.z) * Vector3.forward;
                }
                else if (Vector3.Angle(transform.up, Vector3.up) > 0.0001f)
                {
                    transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
                    //transform.eulerAngles += Quaternion.FromToRotation(transform.up, Vector3.up).eulerAngles;
                }
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }
            else
            {
                rb.constraints = RigidbodyConstraints.None;
            }
            */

            //Debug.DrawRay(transform.position + Vector3.forward * 0.2f, -transform.up * groundScan, Color.red);

            if (jumpCooldown == 0 && Input.GetKey(KeyCode.Space) && Physics.Raycast(transform.position, -transform.up/*, out hit*/, groundScan, lm))
            {
                //Debug.DrawLine(transform.position, hit.point);
                rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
                jumpCooldown = 3;
            }
            else if (jumpCooldown > 0)
            {
                jumpCooldown--;
            }
        }

        /*
        fancyText.text = "Current Movements: ";
        foreach (Animate.KeyFrame kf in Animate.movements.Values)
            fancyText.text += "\n" + kf.animate.name + ":" + kf.name + ", ";
            */
        
    }

    int jumpCooldown = 0;
    int flailNum = 0;

    private void AddFlop(int amount)
    {
        flopping += amount;
        rb.freezeRotation = false;
        rb.angularVelocity += (new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1))) * amount;
    }

    private (int, bool) RunAnimation(Dictionary<string, Animate.KeyFrame> frames, int priority, int topPriority, Animate anim, int frameNum = -1)
    {
        bool allComplete = true;

        if (topPriority <= priority)
        {
            anim.enableAnim = true;
            topPriority = Mathf.Max(priority, topPriority);
            foreach (string fn in frames.Keys)
            {
                if (frameNum == -1 || fn.Contains(frameNum.ToString()))
                {
                    if (!frames[fn].isMoving())
                    {
                        frames[fn].Move();
                    }
                    if (!frames[fn].isComplete())
                    {
                        allComplete = false;
                    }
                }
            }
        } else
        {
            anim.enableAnim = false;
        }

        return (topPriority, allComplete);
    }

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
        } else
        {
            anim.enableAnim = false;
        }

        return (topPriority, allComplete);
    }

    private void Turn(Vector3 lookDir, Vector3 upwards, bool horizontalOnly = true)
    {
        if (spinAttack == 0)
        {
            lookDir = lookDir.normalized;
            if (horizontalOnly)
            {
                lookDir = HorizontalComponent(lookDir);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lookDir, upwards), 10);
            }
            else
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lookDir, upwards), 10);
            }
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        collidersHit++;

        //float speedDiff = (int)(Mathf.Abs(speed - rb.velocity.magnitude));
        //float speedDiff = Vector3.Dot(collision.relativeVelocity, collision.impulse
        float speedDiff = collision.impulse.magnitude;
        if (speedDiff > 4)
        {
            AddFlop((int)(speedDiff * 8));
            hit.play(transform.position, gameObject);

            if (speedDiff > 3)
            {
                float dmg = (speedDiff - 3) / 1.5f;
                HP -= dmg;
                Blood.instance.Splatter(transform.position, (new Vector3(Random.Range(-1, 1), Random.Range(0, 2), Random.Range(-1, 1))).normalized, 0.25f, 0.5f, transform);
                HurtSound(dmg);
            }
            if (spinAttack > 1)
                spinAttack = 1;
        }
        speed = rb.velocity.magnitude;

        //AddFlop((int)(rb.velocity.magnitude * flopping));
    }


    private void OnCollisionExit(Collision collision)
    {
        collidersHit--;
    }

    private Vector3 HorizontalComponent(Vector3 v)
    {
        return Vector3.ProjectOnPlane(v, Vector3.up);
    }

    void HurtSound(float dmg)
    {
        //hurt.play(transform.position, gameObject);

        if (dmg > 2)
        {
            boneBreak.play(transform.position, gameObject);
        }
    }

    public override void Damage(float amount)
    {
        base.Damage(amount);
        HurtSound(amount);
        flopping += Mathf.RoundToInt(amount);
        rb.velocity += amount * (new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1)));
        rb.angularVelocity += amount * (new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1)));

        Blood.instance.Splatter(transform.position, (new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1))).normalized, 0.4f, 0.5f, transform);
        Blood.instance.Chunks(transform.position, (new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1))).normalized, 0.3f, 0.2f, transform);
    }

    bool killed = false;
    public GameObject armature;
    public void Kill()
    {
        if (!killed)
        {
            killed = true;

            GetComponent<Collider>().enabled = false;
            
            foreach (Damager d in GetComponentsInChildren<Damager>())
            {
                d.Enable(false);
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
                r.mass = rb.mass;
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
            SphereCollider c = bone.gameObject.AddComponent<SphereCollider>();
            c.radius = 0.0001f;
            cj.connectedBody = parentBone.gameObject.GetComponent<Rigidbody>();
            RagDoll(bone, depth);
        }
    }
}
