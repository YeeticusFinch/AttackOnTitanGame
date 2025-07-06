using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[ExecuteInEditMode]
public class RoofCollider : MonoBehaviour
{
    public float angle;
    public float length = 1;
    public float width = 1;
    public float thickness = 0.1f;
    public float offset = 0;
    public float endOffset = 0;
    public int resolution = 0;
    public bool leftCaps = false;
    public bool rightCaps = false;

    float oldAngle;
    float oldLength;
    float oldWidth;
    float oldthickness;
    float oldOffset;
    float oldEndOffset;
    int oldResolution;
    bool oldLeftCaps;
    bool oldRightCaps;

    GameObject leftBox;
    GameObject rightBox;

    List<GameObject> endCaps = new List<GameObject>();

    // Start is called before the first frame update
    void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            if (leftBox == null || rightBox == null)
            {
                while (GetComponentInChildren<BoxCollider>() != null)
                {
                    DestroyImmediate(GetComponentInChildren<BoxCollider>().gameObject);
                }
                leftBox = new GameObject();
                rightBox = new GameObject();
                leftBox.transform.parent = transform;
                rightBox.transform.parent = transform;
                leftBox.transform.localPosition = Vector3.zero;
                rightBox.transform.localPosition = Vector3.zero;

                
                BoxCollider leftCollider = leftBox.AddComponent<BoxCollider>();
                BoxCollider rightCollider = rightBox.AddComponent<BoxCollider>();

                fixBoxes();
                fixCaps();
            }
            if (oldAngle != angle || oldLength != length || oldthickness != thickness || oldWidth != width || oldOffset != offset)
            {
                fixBoxes();
                fixCaps();
            }
            if (oldResolution != resolution || oldLeftCaps != leftCaps || oldRightCaps != rightCaps || oldEndOffset != endOffset)
            {
                fixCaps();
            }
        }
    }

    void fixCaps()
    {
        oldResolution = resolution;
        oldLeftCaps = leftCaps;
        oldRightCaps = rightCaps;
        oldEndOffset = endOffset;

        foreach (BoxCollider c in GetComponentsInChildren<BoxCollider>().ToList())
        {
            if (c.gameObject.name.Contains("end"))
            {
                DestroyImmediate(c.gameObject);
            }
        }
        endCaps.Clear();

        if (resolution > 0)
        {
            float offsetScalar = 1.8f;
            float zOffset = thickness / Mathf.Cos(Mathf.Deg2Rad * angle);
            if (offset < zOffset)
                offsetScalar = 0;
            float increment = ((length - offset * offsetScalar) * Mathf.Sin(Mathf.Deg2Rad * angle) - zOffset) * 1.0f / (resolution);

            for (int i = 0; i < resolution; i++)
            {
                float capheight = -offsetScalar * offset * Mathf.Sin(Mathf.Deg2Rad * angle) - increment * (i) - increment/2 - zOffset;
                float capwidth = Mathf.Abs(2 * (capheight + increment/2) / Mathf.Tan(Mathf.Deg2Rad * angle));
                if (leftCaps)
                {
                    GameObject cap = new GameObject();
                    cap.name = "left-end-" + i;
                    cap.transform.parent = transform;
                    cap.transform.localPosition = Vector3.zero;
                    cap.transform.localEulerAngles = Vector3.zero;

                    BoxCollider capCol = cap.AddComponent<BoxCollider>();

                    capCol.center = new Vector3(0, capheight, width/2-thickness/2-endOffset);
                    capCol.size = new Vector3(capwidth, increment, thickness);
                }
                if (rightCaps)
                {
                    GameObject cap = new GameObject();
                    cap.name = "right-end-" + i;
                    cap.transform.parent = transform;
                    cap.transform.localPosition = Vector3.zero;
                    cap.transform.localEulerAngles = Vector3.zero;

                    BoxCollider capCol = cap.AddComponent<BoxCollider>();

                    capCol.center = new Vector3(0, capheight, -width / 2 + thickness/2 + endOffset);
                    capCol.size = new Vector3(capwidth, increment, thickness);
                }
            }
        }
    }

    void fixBoxes()
    {
        oldAngle = angle;
        oldLength = length;
        oldthickness = thickness;
        oldWidth = width;
        oldOffset = offset;

        leftBox.tag = tag;
        rightBox.tag = tag;

        BoxCollider leftCollider = leftBox.GetComponent<BoxCollider>();
        BoxCollider rightCollider = rightBox.GetComponent<BoxCollider>();
        leftBox.transform.localEulerAngles = new Vector3(0, 0, -angle);
        rightBox.transform.localEulerAngles = new Vector3(0, 0, angle);

        leftCollider.size = new Vector3(length-offset*2, thickness, width);
        rightCollider.size = new Vector3(length-offset*2, thickness, width);

        leftCollider.center = new Vector3(offset + length/2, -thickness / 2, 0);
        rightCollider.center = new Vector3(-offset - length/2, -thickness / 2, 0);

        leftBox.transform.localPosition = Vector3.zero;
        rightBox.transform.localPosition = Vector3.zero;
        
        //leftBox.transform.position = transform.position + leftBox.transform.right * (length / 2);
        //rightBox.transform.position = transform.position - rightBox.transform.right * (length / 2);
    }
}
