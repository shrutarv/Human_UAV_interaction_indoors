using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneLaserUIManager : MonoBehaviour
{

    SwarmController swarmController;
    GameObject human1;
    GameObject human2;
    

    public bool waitingForActivate = true;
    public bool waitingForWanderWithSwarm = false;
    public bool waitingForGoHome = false;
    public bool waitingForEncircling = false;
    public bool waitingForGuiding = false;

    // Start is called before the first frame update
    void Start()
    {
        swarmController = GameObject.Find("Drones").GetComponent<SwarmController>();
        human1 = GameObject.Find("Human");
        human2 = GameObject.Find("Human2");

        var lr = transform.Find("Activate").GetComponent<LaserRectangle>();
        lr.DoPulse();

        waitingForActivate = false;
        EnableActivate(false);
        waitingForWanderWithSwarm = false;
        EnableWanderWithSwarm(false);
        waitingForGoHome = false;
        EnableGoHome(false);
        waitingForEncircling = false;
        EnableEncircling(false);
        waitingForGuiding = false;
        EnableGuidingHuman(false);
    }

    public void EnableStateMachine(bool started)
    {
        waitingForActivate = started;
        EnableActivate(started);
        waitingForWanderWithSwarm = false;
        EnableWanderWithSwarm(false);
        waitingForGoHome = false;
        EnableGoHome(false);
        waitingForEncircling = false;
        EnableEncircling(false);
        waitingForGuiding = false;
        EnableGuidingHuman(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (waitingForActivate)
        {
            var dist1 = GetDistanceToHuman(transform.Find("Activate").position, human1);
            var dist2 = GetDistanceToHuman(transform.Find("Activate").position, human2);

            if (dist1 < 0.4f || dist2 < 0.4f)
            {
                
                waitingForActivate = false;
                EnableActivate(false);
                waitingForWanderWithSwarm = true;
                EnableWanderWithSwarm(true);
                waitingForGoHome = true;
                EnableGoHome(true);
                waitingForEncircling = true;
                EnableEncircling(true);
                waitingForGuiding = true;
                EnableGuidingHuman(true);
                swarmController.ActivateSwarm();
            }
        } else
        {
            waitingForActivate = false;
            EnableActivate(false);
        }

        if (waitingForWanderWithSwarm)
        {
            var dist1 = GetDistanceToHuman(transform.Find("WanderWithSwarm").position, human1);
            var dist2 = GetDistanceToHuman(transform.Find("WanderWithSwarm").position, human2);

            if (dist1 < 0.4f || dist2 < 0.4f)
            {
                waitingForActivate = false;
                EnableActivate(false);
                waitingForWanderWithSwarm = false;
                EnableWanderWithSwarm(false);
                waitingForGoHome = true;
                EnableGoHome(true);
                waitingForEncircling = true;
                EnableEncircling(true);
                waitingForGuiding = true;
                EnableGuidingHuman(true);
                swarmController.WanderWithSwarm();
            }
        } else
        {
            waitingForWanderWithSwarm = false;
            EnableWanderWithSwarm(false);
        }

        if (waitingForGoHome)
        {
            var dist1 = GetDistanceToHuman(transform.Find("GoHome").position, human1);
            var dist2 = GetDistanceToHuman(transform.Find("GoHome").position, human2);

            if (dist1 < 0.4f || dist2 < 0.4f)
            {
                waitingForActivate = true;
                EnableActivate(true);
                waitingForWanderWithSwarm = false;
                EnableWanderWithSwarm(false);
                waitingForGoHome = false;
                EnableGoHome(false);
                waitingForEncircling = false;
                EnableEncircling(false);
                waitingForGuiding = false;
                EnableGuidingHuman(false);
                swarmController.GoHome();
            }
        } else
        {
            waitingForGoHome = false;
            EnableGoHome(false);
        }

        if (waitingForEncircling)
        {
            var dist1 = GetDistanceToHuman(transform.Find("EncircleHuman").position, human1);
            var dist2 = GetDistanceToHuman(transform.Find("EncircleHuman").position, human2);

            if (dist1 < 0.4f || dist2 < 0.4f)
            {
                waitingForActivate = false;
                EnableActivate(false);
                waitingForWanderWithSwarm = true;
                EnableWanderWithSwarm(true);
                waitingForGoHome = true;
                EnableGoHome(true);
                waitingForEncircling = false;
                EnableEncircling(false);
                waitingForGuiding = true;
                EnableGuidingHuman(true);
                swarmController.EncircleHuman();
            }
        }
        else
        {
            waitingForEncircling = false;
            EnableEncircling(false);
        }
        
        if (waitingForGuiding)
        {
            var dist1 = GetDistanceToHuman(transform.Find("GuideHuman").position, human1);
            var dist2 = GetDistanceToHuman(transform.Find("GuideHuman").position, human2);

            if (dist1 < 0.4f || dist2 < 0.4f)
            {
                waitingForActivate = false;
                EnableActivate(false);
                waitingForWanderWithSwarm = true;
                EnableWanderWithSwarm(true);
                waitingForGoHome = true;
                EnableGoHome(true);
                waitingForEncircling = true;
                EnableEncircling(true);
                waitingForGuiding = false;
                EnableGuidingHuman(false);
                swarmController.GuideHuman();
            }
        }
        else
        {
            waitingForGuiding = false;
            EnableGuidingHuman(false);
        }
    }

    private void EnableActivate(bool enable)
    {
        var lb = transform.Find("Activate").GetComponent<LaserBehaviour>();
        var lr = transform.Find("Activate").GetComponent<LaserRectangle>();
        lb.visible = enable;
        lr.drawGizmos = enable;
    }

    private void EnableWanderWithSwarm(bool enable)
    {
        var lb = transform.Find("WanderWithSwarm").GetComponent<LaserBehaviour>();
        var lr = transform.Find("WanderWithSwarm").GetComponent<LaserRectangle>();
        lb.visible = enable;
        lr.drawGizmos = enable;
    }

    private void EnableGoHome(bool enable)
    {
        var lb = transform.Find("GoHome").GetComponent<LaserBehaviour>();
        var lr = transform.Find("GoHome").GetComponent<LaserRectangle>();
        lb.visible = enable;
        lr.drawGizmos = enable;
    }

    private void EnableEncircling(bool enable)
    {
        var lb = transform.Find("EncircleHuman").GetComponent<LaserBehaviour>();
        var lr = transform.Find("EncircleHuman").GetComponent<LaserRectangle>();
        lb.visible = enable;
        lr.drawGizmos = enable;
    }

    private void EnableGuidingHuman(bool enable)
    {
        var lb = transform.Find("GuideHuman").GetComponent<LaserBehaviour>();
        var lr = transform.Find("GuideHuman").GetComponent<LaserRectangle>();
        lb.visible = enable;
        lr.drawGizmos = enable;
    }

    private float GetDistanceToHuman(Vector3 position, GameObject human)
    {
        if(human != null)
        {
            return Vector3.Distance(position, human.transform.position);
        } else
        {
            return float.PositiveInfinity;
        }
    }

}
