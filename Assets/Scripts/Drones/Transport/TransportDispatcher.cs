using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransportDispatcher : MonoBehaviour, ITransportManagerListener
{

    SwarmController swarmController;
    TransportManager transportManager;
    Queue<TransportOrder> orders = new Queue<TransportOrder>();

    public int numberOfWaitingOrders = 0;

    // Start is called before the first frame update
    void Start()
    {

        transportManager = GameObject.Find("Transports").GetComponent<TransportManager>();
        transportManager.AddListener(this);
        swarmController = GetComponent<SwarmController>();

    }

    // Update is called once per frame
    void Update()
    {   
          if(orders.Count > 0)
        {
            numberOfWaitingOrders = orders.Count;
            var order = orders.Dequeue();
            var drone = swarmController.GetNearestTransportDroneToPosition(order.source.transform.position);
            //Debug.Log(drone.position);
            if(drone == null)
            {
                orders.Enqueue(order);
            } else
            {
                var transportStateMachine = drone.GetComponentInChildren<TransportStateMachine>();
                transportStateMachine.Transport(order);
            }

        }
    }

    public void TransportOrderCreated(TransportOrder order)
    {
        var sourcePos = order.source.transform.position;
        orders.Enqueue(order);
    }

}
