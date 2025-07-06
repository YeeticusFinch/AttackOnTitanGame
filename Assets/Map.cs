using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public QPathFinder.PathFinder pathfinder;
    public FancyPF fancyPF;
    public static Map instance;

    public static float metersToUnits = 0.1642f;

    public static float MetersToUnits(float meters)
    {
        return meters * metersToUnits;
    }

    public static float UnitsToMeters(float units)
    {
        return units / metersToUnits;
    }

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
