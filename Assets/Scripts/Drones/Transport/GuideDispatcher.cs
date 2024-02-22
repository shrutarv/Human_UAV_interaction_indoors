using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideDispatcher : MonoBehaviour
{

    SwarmController swarmController;
    TransportManager transportManager;
    //Queue<GuideOrder> orders = new Queue<GuideOrder>();

    public int numberOfWaitingOrders = 0;
    private bool enterPressed = false;

    // Start is called before the first frame update
    void Start()
    {

        //transportManager = GameObject.Find("Transports").GetComponent<TransportManager>();
        //transportManager.AddListener(this);
        swarmController = GetComponent<SwarmController>();

    }

    // Update is called once per frame
    void Update()
    {   
        Debug.Log("Update");
        if (!enterPressed)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Debug.Log("Triggering GoToHuman");
                // Fire(Trigger.GoToHuman);
                enterPressed = true;            
            

                var drone = swarmController.GetNearestTransportDroneToPosition(GameObject.Find("Human").transform.position);
                //Debug.Log(drone.position);
                if(drone == null)
                {
                    Debug.Log("Drone is null");
                } else
                {
                    var transportStateMachine = drone.GetComponentInChildren<TransportStateMachine>();
                    //transportStateMachine.GuideHuman();
                }
            }
        }
    }


}
