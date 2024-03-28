using BezierSolution;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DroneController : MonoBehaviour
{
    public int id;
    public float starttime = 0;
    public float duration = 3;
    public float height = 1.6f;
    public float trajectoryTimescale = 1;
    public float trajectoryVMax = 1;
    public float trajectoryAMax = 1;
    public Vector3 landPosition;
    public Vector3 homeHoverPosition;
    public Vector3 targetPosition;

    public bool isFirstStart = true;

    public float waitForLandTime = 5;
    public bool waitingForLanding = false;

    public List<IDroneControllerListener> listeners = new List<IDroneControllerListener>();

    public BezierSpline spline;

    public IDroneConnection connection;
    public AutoPilot autoPilot;

    public bool directFlight = false;
    public float velocity = 1;

    // Start is called before the first frame update
    void Start()
    {
        //SetToCrazyflieConnection();
        var droneBezier = GameObject.Find("DroneBezier");
        autoPilot = transform.GetComponentInChildren<AutoPilot>();
        //spline = droneBezier.GetComponentInChildren<BezierSpline>();
    }

    public void SetToCrazyflieConnection()
    {
        connection = transform.parent.parent.gameObject.GetComponent<CrazyflieConnection>();
    }

    public void SetDroneConnection(IDroneConnection conn)
    {
        connection = conn;
    }

    public IDroneConnection GetConnection()
    {
        if (connection == null)
        {
            SetToCrazyflieConnection();
        }
        return connection;
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit) && Input.GetMouseButtonDown(0))
        {
            if (hit.transform == connection.GetFloor().transform)
            {
                targetPosition = new Vector3(hit.point.x, hit.point.y + 0.01f, hit.point.z);
                if (directFlight)
                {
                    connection.MoveTo(id, 0, ComputeDuration(), targetPosition.x, targetPosition.z, height, 0);
                }
            }
        }

        if (waitingForLanding)
        {
            if(waitForLandTime < 0)
            {
                waitForLandTime = 5;
                waitingForLanding = false;
                DroneLand();
            } else
            {
                waitForLandTime -= Time.deltaTime;
            }
        }
    }

    internal float ComputeDuration()
    {
        float distance = Vector3.Distance(transform.position, targetPosition);
        return Math.Max((distance / velocity), 2f);
    }

    public void DroneStart()
    {
        if (isFirstStart)
        {
            landPosition = transform.position;
            homeHoverPosition = transform.position;
            homeHoverPosition.y = height;  //NILS comment for charging station mode, SHRUTARV uncomment for floor-based landing
            isFirstStart = false;
        }
		
		//SHRUTARV floor based starting code
        Debug.Log(id);
		connection.Start(id, starttime, duration, height); //SHRUTARV
		StartCoroutine(ObserveStartAndNotifyListeners(duration)); //SHRUTARV
		
		//NILS charging station starting code
		//foreach (var listener in listeners)
		//{
		//	listener.DroneStarted();
		//}
	}

    IEnumerator ObserveStartAndNotifyListeners(float time)
    {
        yield return new WaitForSeconds(time);
        foreach(var listener in listeners)
        {
            listener.DroneStarted();
        }
    }

    public void DroneLand()
    {
        connection.LandWithHeight(id, starttime, duration, landPosition.y);
        StartCoroutine(ObserveLandingAndNotifyListeners(duration));
    }

    public void DroneLand1(int id)
    {
        // Land drone with id 1
        connection.Land(1, starttime, duration);
        StartCoroutine(ObserveLandingAndNotifyListeners(duration));
    }
    
    IEnumerator ObserveLandingAndNotifyListeners(float time)
    {
        yield return new WaitForSeconds(time);
        foreach (var listener in listeners)
        {
            listener.DroneLanded();
        }
    }

    public void DroneMoveHome()
    {
        connection.MoveTo(id, starttime, duration, homeHoverPosition.x, homeHoverPosition.z, homeHoverPosition.y, 0);
    }

    public void DroneMoveHome1(int id)
    {
        Debug.Log("DroneMoveHome1");
        connection.MoveTo(2, starttime, duration, homeHoverPosition.x, homeHoverPosition.z, homeHoverPosition.y, 0);
    }


    public void DroneMoveHomeAndLand()
    {
        DroneMoveHome();
        waitingForLanding = true;
    }

    public void DroneMoveToTarget()
    {
        connection.MoveTo(id, starttime, duration, targetPosition.x, targetPosition.z, height, 0);
    }

    public void AddListener(IDroneControllerListener listener)
    {
        listeners.Add(listener);
    }

    [CustomEditor(typeof(DroneController))]
    public class DroneControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            DroneController c = (DroneController)target;
            if (GUILayout.Button("Start"))
            {
                c.DroneStart();
            }
            if (GUILayout.Button("Land"))
            {
                c.DroneLand();
            }
            if (GUILayout.Button("Move Home"))
            {
                c.DroneMoveHome();
            }
            if (GUILayout.Button("Move to Target"))
            {
                c.DroneMoveToTarget();
            }
            
        }
    }

}
