using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human : Destroyable
{
    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
    }
    
    public bool Alive()
    {
        return HP > 0;
    }

    new protected void FixedUpdate()
    {
        base.FixedUpdate();
    }
}
