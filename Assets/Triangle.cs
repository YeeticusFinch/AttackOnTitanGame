using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class Triangle : MonoBehaviour
{
    public float angle = 45;
    public float length = 1;
    public float thickness = 0.1f;
    public int resolution = 0;
    public float shifter = 0;

    float oldAngle;
    float oldLength;
    float oldthickness;
    int oldResolution;
    float oldShifter = 0;

    // Update is called once per frame
    void Update()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            if (oldAngle != angle || oldLength != length || oldthickness != thickness || oldResolution != resolution || oldShifter != shifter)
            {
                oldAngle = angle;
                oldLength = length;
                oldthickness = thickness;
                oldResolution = resolution;
                oldShifter = shifter;

                foreach (BoxCollider c in GetComponentsInChildren<BoxCollider>().ToList())
                {
                    if (c.gameObject.name.Contains("triangle"))
                    {
                        DestroyImmediate(c.gameObject);
                    }
                }

                if (resolution > 0)
                {
                    Debug.DrawRay(transform.position, (-transform.up * Mathf.Sin(Mathf.Deg2Rad * angle) + transform.right * Mathf.Cos(Mathf.Deg2Rad * angle)) * length + transform.right * shifter, Color.green);
                    Debug.DrawRay(transform.position, (-transform.up * Mathf.Sin(Mathf.Deg2Rad * angle) - transform.right * Mathf.Cos(Mathf.Deg2Rad * angle)) * length + transform.right * shifter, Color.green);
                    //float zOffset = thickness / Mathf.Cos(Mathf.Deg2Rad * angle);
                    float increment = ((length) * Mathf.Sin(Mathf.Deg2Rad * angle)) * 1.0f / (resolution);

                    for (int i = 1; i < resolution; i++)
                    {
                        float capheight = -increment * (i) - increment / 2;
                        float capwidth = Mathf.Abs(2 * (capheight + increment / 2) / Mathf.Tan(Mathf.Deg2Rad * angle));
                        {
                            GameObject cap = new GameObject();
                            cap.name = "triangle-" + i;
                            cap.transform.parent = transform;
                            cap.transform.localPosition = Vector3.zero;
                            cap.transform.localEulerAngles = Vector3.zero;

                            BoxCollider capCol = cap.AddComponent<BoxCollider>();

                            capCol.center = new Vector3(shifter * i / resolution, capheight, 0);
                            capCol.size = new Vector3(capwidth, increment, thickness);
                        }
                    }
                }
            }
        }
    }
}