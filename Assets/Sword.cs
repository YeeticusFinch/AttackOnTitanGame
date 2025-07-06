using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    public GameObject bladeBase;
    public GameObject bladeTip;
    GameObject swordTrail;
    LineRenderer lr;

    [System.NonSerialized]
    public bool swinging = false;
    Vector3[] prevPos = new Vector3[30];

    Player player;

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponentInParent<Player>();
        swordTrail = Instantiate(Resources.Load("SwordTrail"), transform) as GameObject;
        lr = swordTrail.GetComponent<LineRenderer>();
        swordTrail.transform.localPosition = bladeBase.transform.localPosition * 0.5f + bladeTip.transform.localPosition * 0.5f;
        swordTrail.transform.localPosition += new Vector3(0, 0, bladeTip.transform.localPosition.z * 0.5f);
        swordTrail.transform.localEulerAngles = new Vector3(0, -90, 0);
        swordTrail.transform.localScale = new Vector3(2, 2, 2);

        lr.positionCount = prevPos.Length;
    }

    // Update is called once per frame
    void Update()
    {
        if (swinging)
        {
            if (player != null && player.HP < 0.0001f)
            {
                swinging = false;
                player.attacker.Enable(false);
                player.spinAttacker.Enable(false);
            }
            else
            {
                SetPositions();
                swordTrail.SetActive(true);
                lr.SetPositions(prevPos);
                //lr.SetPositions(new Vector3[] { swordTrail.transform.position, swordTrail.transform.position - swordTrail.transform.right * 0.05f, prevPos, prevPos2, prevPos3 });
            }
           
        } else
        {
            swordTrail.SetActive(false);
        }

    }

    void SetPositions()
    {
        for (int i = prevPos.Length-1; i >= 2; i--)
        {
            if (prevPos[i - 1].magnitude < 0.001f)
            {
                prevPos[i] = swordTrail.transform.position - swordTrail.transform.right * 0.05f;
            }
            else
                prevPos[i] = prevPos[i - 1];
        }
        prevPos[0] = swordTrail.transform.position;
        prevPos[1] = swordTrail.transform.position - swordTrail.transform.right * 0.05f;
    }

    int c = 0;
    private void FixedUpdate()
    {
        c++;
        c %= 10000;
        if (c % 2 == 0 && !swinging)
        {
            SetPositions();
            /*
            prevPos3 = prevPos2;
            prevPos2 = prevPos;
            prevPos = swordTrail.transform.position - swordTrail.transform.right * 0.05f;
            */

        }
    }

}
