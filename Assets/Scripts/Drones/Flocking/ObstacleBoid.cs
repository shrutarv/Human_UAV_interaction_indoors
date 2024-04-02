using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleBoid : MonoBehaviour
{

    public Boid boid;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public Boid GetBoid()
    {
        if(boid == null)
        {
            boid = new Boid();
            boid.Body = transform;
            boid.SeparationDistance = 1.0f; //original 1.2f
        }
        return boid;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
