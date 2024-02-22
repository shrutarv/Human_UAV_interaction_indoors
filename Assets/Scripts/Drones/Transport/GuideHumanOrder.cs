using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideHumanOrder : MonoBehaviour
{
    public int id;
    //public GameObject source;
    public GameObject station;
    //public GameObject load;

    // Start is called before the first frame update
    void Start()
    {
        //source = transform.Find($"Source{id}").gameObject;
        station = transform.Find("Station").gameObject;
        //load = transform.Find($"Load{id}").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Finished()
    {
        Destroy(gameObject);
    }
}
