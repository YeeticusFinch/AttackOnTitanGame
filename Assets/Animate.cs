using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Animate : MonoBehaviour
{

    public static Dictionary<GameObject, KeyFrame> movements = new Dictionary<GameObject, KeyFrame>();

    public bool playerAnimate;
    public string name;
    public float divisionMult = 1;
    public int overridePriority = -1;
    public KeyFrame[] keyframes;

    public Dictionary<KeyFrame, int> index = new Dictionary<KeyFrame, int>();
    public Dictionary<KeyFrame, double> timeStamp = new Dictionary<KeyFrame, double>();
    public Dictionary<KeyFrame, Vector3> startEuler = new Dictionary<KeyFrame, Vector3>();
    public Dictionary<KeyFrame, Vector3> startPos = new Dictionary<KeyFrame, Vector3>();
    public Dictionary<KeyFrame, bool> completed = new Dictionary<KeyFrame, bool>();

    [System.NonSerialized]
    public Player player;
    
    [System.Serializable]
    public struct KeyFrame
    {
        public KeyFrame(string name, GameObject obj, bool rotate, bool translate, Vector3 euler, Vector3 pos, int divisions, int priority, Animate animate, bool invert_x, bool invert_y, bool invert_z, bool invert_check_x, bool invert_check_y, bool invert_check_z, bool print)
        {
            this.name = name;
            this.obj = obj;
            this.translate = translate;
            this.rotate = rotate;
            this.euler = euler;
            this.pos = pos;
            this.divisions = divisions;
            this.animate = animate;
            this.priority = priority;
            this.invert_x = invert_x;
            this.invert_y = invert_y;
            this.invert_z = invert_z;
            this.invert_check_x = invert_check_x;
            this.invert_check_y = invert_check_y;
            this.invert_check_z = invert_check_z;
            this.print = print;
        }

        public string name;
        public GameObject obj;
        public bool translate;
        public bool rotate;
        public Vector3 euler;
        public Vector3 pos;
        public int divisions;
        public int priority;
        public Animate animate;
        public bool invert_x;
        public bool invert_y;
        public bool invert_z;
        public bool invert_check_x;
        public bool invert_check_y;
        public bool invert_check_z;
        public bool print;

        public void Teleport()
        {
            Move();
            Step(true);
            animate.completed[this] = true;
        }

        public void Move()
        {
            animate.completed[this] = false;
            if (Animate.movements.ContainsKey(obj) && priority <= Animate.movements[obj].priority && !Animate.movements[obj].isComplete())
            {
                // Let the previous animation continue
            } else
            {
                //Debug.Log(name + " Starting movement");
                animate.index[this] = 1;
                //Debug.Log("Movement: " + name + " index: " + animate.index);
                if (rotate)
                    animate.startEuler[this] = getCurrRot();
                //Debug.Log(name + "startEuler = " + animate.startEuler[this]);
                if (translate)
                    animate.startPos[this] = obj.transform.localPosition;
                if (Animate.movements.ContainsKey(obj))
                {
                    Animate.movements[obj].animate.index[Animate.movements[obj]] = 0;
                    Animate.movements[obj] = this;
                }
                else
                {
                    Animate.movements.Add(obj, this);
                }
            }
        }

        
        public Vector3 getCurrRot()
        {
            return applyInverts(FixAngle(obj.transform.localEulerAngles));
        }

        public Vector3 applyInverts(Vector3 vec)
        {
            return new Vector3(invert_x ? 360 - vec.x : vec.x, invert_y ? 360 - vec.y : vec.y, invert_z ? 360 - vec.z : vec.z);
        }

        public Vector3 applyCheckInverts(Vector3 vec)
        {
            return new Vector3(invert_check_x ? 360 - vec.x : vec.x, invert_check_y ? 360 - vec.y : vec.y, invert_check_z ? 360 - vec.z : vec.z);
        }

        public void reset()
        {
            animate.index[this] = 0;
        }

        public bool isMoving()
        {
            //Debug.Log(name + " index: " + animate.index);
            return animate.index[this] <= divisions && animate.index[this] > 0;
        }

        public bool isComplete()
        {
            if (animate!=null && animate.completed.ContainsKey(this) && animate.completed[this])
                return true;
            //return animate.index[this] >= divisions;
            /*if (animate.timeStamp[this] - Time.realtimeSinceStartupAsDouble < 0.5f)
            {
                animate.timeStamp[this] = Time.realtimeSinceStartupAsDouble;
                return animate.index[this] >= divisions;
            }
            animate.timeStamp[this] = Time.realtimeSinceStartupAsDouble;
            */
            /*
            if (rotate)
            {
                if (print)
                    Debug.Log(name + " target: " + FixAngle(euler) + " actual: " + FixAngle(currRot) + " diff: " + AngleDiff(FixAngle(euler), FixAngle(currRot)));
                if (AngleDiff(FixAngle(euler), FixAngle(currRot)).magnitude < speed * 1.5f)
                {
                    obj.transform.localEulerAngles = euler;
                    // reached
                }
                else
                    return false; // didn't reach
            }
            if (translate) {
                if (Vector3.Distance(pos, obj.transform.localPosition) < speed * 1.5f)
                {
                    obj.transform.localPosition = pos;
                    // reached
                }
                else
                    return false;
            } */
            //Debug.Log(name + " complete: " + (animate.index[this] > divisions ? "true" : "false"));
            //return animate.index[this] > divisions;

            //Debug.Log(name + " target: " + FixAngle(euler) + " actual: " + FixAngle(currRot) + " diff: " + AngleDiff(FixAngle(euler), FixAngle(currRot)));
            bool result = true;
            if (rotate)
            {
                Vector3 currRot = applyCheckInverts(getCurrRot());
                if (print)
                    Debug.Log(name + " current: " + FixAngle(FixAngle2(currRot)) + ", target: " + FixAngle(FixAngle2(euler)) + ", diff: " + FixAngle(AngleDiff2(FixAngle(FixAngle2(euler)), FixAngle(FixAngle2(currRot)))).magnitude);
                if (FixAngle(AngleDiff2(FixAngle(FixAngle2(euler)), FixAngle(FixAngle2(currRot)))).magnitude > 1 && AngleDiff(FixAngle(euler), FixAngle(currRot)).magnitude > 1)
                    result = false;
            }
            if (translate)
                if (Vector3.Distance(pos, obj.transform.localPosition) > 0.01f)
                    result = false;
            if (result)
                animate.index[this] = 0;

            animate.completed[this] = result;
            return result;
        }

        public void Step(bool overrideDivisions = false)
        {
            //Vector3 currRot = getCurrRot();
            //Debug.Log(name + " index: " + animate.index[this]);
            if (animate.index[this] < divisions && !overrideDivisions)
            {
                if (rotate)
                {
                    obj.transform.localEulerAngles = (((divisions - animate.index[this]) * animate.startEuler[this] + animate.index[this] * euler) / divisions);
                }
                if (translate)
                {
                    obj.transform.localPosition = ((divisions - animate.index[this]) * animate.startPos[this] + animate.index[this] * pos) / divisions;
                }
            } else
            {
                if (rotate)
                {
                    if (print)
                        Debug.Log(name + " to " + euler);
                    obj.transform.localEulerAngles = euler;
                }
                if (translate)
                {
                    obj.transform.localPosition = pos;
                }
            }
            if (!overrideDivisions)
                animate.index[this]++;

            /*
            Vector3 currRot = getCurrRot();
            if (rotate)
                obj.transform.localEulerAngles -= applyInverts(AngleDiff(FixAngle(euler), FixAngle(currRot)).normalized * speed);
            if (translate)
                obj.transform.localPosition += (pos - obj.transform.localPosition).normalized * speed;
                */
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    [System.NonSerialized]
    public bool initted = false;

    public void EditorInit()
    {
        index.Clear();
        timeStamp.Clear();
        startPos.Clear();
        startEuler.Clear();
        completed.Clear();

        for (int i = 0; i < keyframes.Length; i++)
        {
            //Debug.Log("Adding " + keyframes[i].name + " for " + keyframes[i].obj.name);
            if (keyframes[i].name.Length == 0)
                continue;
            keyframes[i] = new KeyFrame(keyframes[i].name, keyframes[i].obj, keyframes[i].rotate, keyframes[i].translate, keyframes[i].euler, keyframes[i].pos, keyframes[i].divisions, overridePriority == -1 ? keyframes[i].priority : overridePriority, this, keyframes[i].invert_x, keyframes[i].invert_y, keyframes[i].invert_z, keyframes[i].invert_check_x, keyframes[i].invert_check_y, keyframes[i].invert_check_z, keyframes[i].print);
            index.Add(keyframes[i], 0);
            timeStamp.Add(keyframes[i], 0);
            startPos.Add(keyframes[i], Vector3.zero);
            startEuler.Add(keyframes[i], Vector3.zero);
        }
        if (playerAnimate)
        {
            player = GetComponent<Player>();
            if (player == null)
                player = GetComponentInParent<Player>();
        }
    }

    public void Init()
    {
        index.Clear();
        timeStamp.Clear();
        startPos.Clear();
        startEuler.Clear();
        completed.Clear();

        for (int i = 0; i < keyframes.Length; i++)
        {
            //Debug.Log("Adding " + keyframes[i].name + " for " + keyframes[i].obj.name);
            if (keyframes[i].name.Length == 0)
                continue;
            keyframes[i] = new KeyFrame(keyframes[i].name, keyframes[i].obj, keyframes[i].rotate, keyframes[i].translate, keyframes[i].euler, keyframes[i].pos, Mathf.Max(1, Mathf.RoundToInt(keyframes[i].divisions * divisionMult)), keyframes[i].priority, this, keyframes[i].invert_x, keyframes[i].invert_y, keyframes[i].invert_z, keyframes[i].invert_check_x, keyframes[i].invert_check_y, keyframes[i].invert_check_z, keyframes[i].print);
            index.Add(keyframes[i], 0);
            timeStamp.Add(keyframes[i], 0);
            startPos.Add(keyframes[i], Vector3.zero);
            startEuler.Add(keyframes[i], Vector3.zero);
            completed.Add(keyframes[i], false);
        }
        if (playerAnimate)
        {
            player = GetComponent<Player>();
            if (player == null)
                player = GetComponentInParent<Player>();
        }
        initted = true;
    }

    public void ResetAll()
    {
        foreach (KeyFrame kf in index.Keys.ToList())
            index[kf] = 0;
        foreach (KeyFrame kf in completed.Keys.ToList())
            completed[kf] = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [System.NonSerialized]
    public bool enableAnim = false;

    int c = 0;
    private void FixedUpdate()
    {

        if (c % 2 == 0 && movements.Count > 0)
        {
            foreach (KeyFrame kf in keyframes)
            {
                if (movements.ContainsValue(kf))
                {
                    if (kf.isComplete())
                    {
                        movements.Remove(kf.obj);
                    } else
                    {
                        kf.Step();
                    }
                }
            }
        }

        c++;
        c %= 10000;
    }

    public static Vector3 FixAngle(Vector3 eulers)
    {
        while (eulers.x > 180)
            eulers.x -= 360;
        while (eulers.x < -180)
            eulers.x += 360;
        while (eulers.y > 180)
            eulers.y -= 360;
        while (eulers.y < -180)
            eulers.y += 360;
        while (eulers.z > 180)
            eulers.z -= 360;
        while (eulers.z < -180)
            eulers.z += 360;

        if (Mathf.Abs(eulers.y) == 180 && Mathf.Abs(eulers.z) == 180)
            eulers = new Vector3(eulers.x + 180, 0, 0);
        if (Mathf.Abs(eulers.x) == 180 && Mathf.Abs(eulers.z) == 180)
            eulers = new Vector3(0, eulers.x + 180, 0);
        if (Mathf.Abs(eulers.y) == 180 && Mathf.Abs(eulers.x) == 180)
            eulers = new Vector3(0, 0, eulers.x + 180);

        return eulers;
    }

    public static Vector3 AngleDiff(Vector3 a, Vector3 b)
    {
        float x;
        float y;
        float z;

        x = b.x - a.x;
        if (Mathf.Abs(x) > Mathf.Abs(b.x - a.x + 360))
            x = b.x - a.x + 360;
        if (Mathf.Abs(x) > Mathf.Abs(b.x - a.x - 360))
            x = b.x - a.x - 360;

        y = b.y - a.y;
        if (Mathf.Abs(y) > Mathf.Abs(b.y - a.y + 360))
            y = b.y - a.y + 360;
        if (Mathf.Abs(y) > Mathf.Abs(b.y - a.y - 360))
            y = b.y - a.y - 360;

        z = b.z - a.z;
        if (Mathf.Abs(z) > Mathf.Abs(b.z - a.z + 360))
            z = b.z - a.z + 360;
        if (Mathf.Abs(z) > Mathf.Abs(b.z - a.z - 360))
            z = b.z - a.z - 360;

        return new Vector3(x, y, z);
    }

    public static Vector3 FixAngle2(Vector3 eulers)
    {
        return Quaternion.Euler(eulers).eulerAngles;
    }

    public static Vector3 AngleDiff2(Vector3 a, Vector3 b)
    {
        return (Quaternion.Inverse(Quaternion.Euler(a)) * Quaternion.Euler(b)).eulerAngles;
    }
}
