using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ForceAnimate : MonoBehaviour
{
    public bool forceFrame = false;
    public Animate anim;
    public string animName;
    public int frameNum = 1;

    int oldFrameNum;
    Animate oldAnim;
    bool oldForceFrame = false;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (anim.name != animName)
        {
            animName = anim.name;
        }
        if (!Application.isPlaying && forceFrame)
        {
            if (anim != oldAnim || oldFrameNum != frameNum || oldForceFrame != forceFrame)
            {
                //Debug.Log("Forcing frame " + frameNum + " of anim " + anim.name);
                oldForceFrame = forceFrame;
                oldAnim = anim;
                oldFrameNum = frameNum;

                anim.Init();

                ApplyAnim();
            }
        }
    }

    void ApplyAnim()
    {
        foreach (Animate.KeyFrame kf in anim.keyframes)
        {
            if (ContainsNumber(kf.name))
            {
                if (kf.name.Contains(frameNum.ToString()))
                    kf.Teleport();
            } else
            {
                kf.Teleport();
            }
        }
    }

    bool ContainsNumber(string txt)
    {
        char[] numbers = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        foreach (char c in numbers)
        {
            if (txt.Contains(c))
                return true;
        }

        return false;
    }
}
