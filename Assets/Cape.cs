using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cape : MonoBehaviour
{
    Rigidbody rb;
    public GameObject tip;
    public GameObject pole;
    public GameObject actualCapeTip;
    public GameObject attachPoint;

    Vector3 attachOffset;

    Vector3 tipPrevPos;
    Vector3 polePrevPos;

    public float capeLength;
    public float randomness;
    public float lerpSpeed;

    public Vector3 tipSettlePos;
    public Vector3 poleSettlePos;

    Vector3 tipSettlePosOffset;

    Vector3 tipRand;
    Vector3 poleRand;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        tip.transform.SetParent(null);
        pole.transform.SetParent(null);

        attachOffset = attachPoint.transform.InverseTransformPoint(transform.position);
        tipSettlePosOffset = attachPoint.transform.InverseTransformPoint(transform.TransformPoint(tipSettlePos));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        transform.position = attachPoint.transform.TransformPoint(attachOffset);
        
        tip.transform.rotation = transform.rotation;
        float tipDist = Vector3.Distance(tip.transform.position, transform.TransformPoint(tipSettlePos));
        float poleDist = Vector3.Distance(pole.transform.position, transform.TransformPoint(poleSettlePos));
        Vector3 localTip = transform.InverseTransformPoint(tip.transform.position);
        if (transform.InverseTransformPoint(tip.transform.position).y < tipSettlePos.y)
        {
            // Prevent cape from clipping into character
            tip.transform.position = transform.TransformPoint(new Vector3(localTip.x, tipSettlePos.y, localTip.z));
        }
        if (tipDist > 0.01f)
        {
            if (tipDist > capeLength)
                tip.transform.position = transform.TransformPoint(tipSettlePos) + (tip.transform.position - transform.TransformPoint(tipSettlePos)).normalized * capeLength;
            tip.transform.position = LerpPos(tip.transform.position, transform.TransformPoint(tipSettlePos) + rb.velocity.magnitude * randomness * (new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), Random.Range(-10, 10))));
        }
        if (poleDist > 0.01f)
        {
            if (poleDist > capeLength/2)
                pole.transform.position = transform.TransformPoint(poleSettlePos) + (pole.transform.position - transform.TransformPoint(poleSettlePos)).normalized * capeLength/2;
            pole.transform.position = LerpPos(pole.transform.position, transform.TransformPoint(poleSettlePos) + rb.velocity.magnitude * randomness * (new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), Random.Range(-10, 10))));
        }
    }

    Vector3 LerpPos(Vector3 pos, Vector3 target)
    {
        if (Vector3.Distance(pos, target) < lerpSpeed)
            return target;
        else {
            Vector3 diff = target - pos;
            return pos + diff.normalized * lerpSpeed;
        }
    }
}
