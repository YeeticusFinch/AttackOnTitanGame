using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QPathFinder;
public class PFCopy : MonoBehaviour
{
    public PathFinder from;
    public PathFinder to;

    // Start is called before the first frame update
    void Start()
    {
        /*
        foreach (Node n in from.graphData.nodes)
        {
            to.graphData.nodes.Add(n);
        }*/

        to.graphData = from.graphData;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
