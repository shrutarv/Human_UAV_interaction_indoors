using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderController : MonoBehaviour
{

    public DroneController droneController;
    public Transform drone;
    public float velocity = 1.4f; 

    public Vector3 targetPosition;

    public Vector3 nextRotationEuler;

    public Boid boid;
    public List<Boid> boids;
    public List<ObstacleBoid> obstacleBoids;
    public bool isWanderingActive = false;
    public Wandering3D wanderingBehavior = new Wandering3D();
    public bool isPursuitActive = false;

    public Pursuit pursuitBehavior = new Pursuit();
    public InferenceAgent pursuitInferenceAgent;

    public bool isEncircleHumanActive = false;
    public ObstacleBoid currentHuman = null;
    public float encircleAngle = 0f;
    public float encircleDistance = 0f;
    public Encircling encirclingBehavior = new Encircling();

    public Guiding guidingBehavior = new Guiding();
    public bool isGuidingHumanActive = false;
    public float approachDistance = 0f;
    public ObstacleBoid GuidetHuman = null;
    //public float encircleAngle = 0f;
    //public float encircleDistance = 0f;


    public bool isUpperFencingActive = true;
    public Fencing3D upperFencingBehavior = new Fencing3D();
    public Fencing3D lowerFencingBehavior = new Fencing3D();
    public Separation separationBehavior = new Separation();
    public bool isCohesionActive = false;
    public Cohesion cohesionBehavior = new Cohesion();
    public bool isAlignmentActive = false;
    public Alignment alignmentBehavior = new Alignment();

    public bool leadingDroneActive = false;
    internal float droneTime = 0;

    public Rigidbody rb;
    Transform test;
    //SwarmController swarmController;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        targetPosition = transform.position;
        wanderingBehavior.Init(transform);
        pursuitBehavior.Init(transform, droneController.transform);
        encirclingBehavior.Init(droneController.id, transform, obstacleBoids);
        guidingBehavior.Init(droneController.id, transform, obstacleBoids);
        //swarmController = GetComponent<SwarmController>();

        pursuitInferenceAgent = GetComponent<InferenceAgent>();
        
        upperFencingBehavior.Init(transform);
        lowerFencingBehavior.Init(transform);
        lowerFencingBehavior.yMax = 1.4f;
        lowerFencingBehavior.yMin = 0.1f;
        lowerFencingBehavior.zMax = 3.5f;
        separationBehavior.Init(droneController.id, transform);
        cohesionBehavior.Init(droneController.id, transform);
        alignmentBehavior.Init(droneController.id, transform);
    }

    // Update is called once per frame
    void Update()
    {   
        //test = swarmController.d;   
        //var drone = test.Find("Drone");
        //Debug.Log(swarmController.step);
            
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //if (Physics.Raycast(ray, out RaycastHit hit) && Input.GetMouseButtonDown(0))
        //{
        //    if (hit.transform == droneController.GetConnection().GetFloor().transform)
        //    {
        //        targetPosition = new Vector3(hit.point.x, hit.point.y + 0.5f, hit.point.z);
        //        pursuitBehavior.SetTarget(targetPosition);
                
        //    }
        //}
    }

    void FixedUpdate()
    {
        if (!isPursuitActive && !isWanderingActive)
        {
            transform.position = droneController.transform.position;
        }

        Quaternion nextRotation = transform.rotation;
        

        if (isPursuitActive && !pursuitBehavior.HasLeaderReachedTarget())
        {
            //if (isEncircleHumanActive)
            //{
                
            //    pursuitBehavior.target = encirclingBehavior.GetTargetPosition(currentHuman?.GetBoid(), encircleAngle, encircleDistance);
            //    targetPosition = pursuitBehavior.target != null ? pursuitBehavior.target.Value : Vector3.zero;
            //}

            if (pursuitBehavior.target != null)
            {
                //Debug.Log(pursuitBehavior.target);
                if (pursuitInferenceAgent == null)
                {
                    nextRotation = Quaternion.LookRotation(pursuitBehavior.GetPursuitVector().Value, Vector3.up);
                } else
                {
                    Debug.Log(pursuitInferenceAgent);
                    pursuitInferenceAgent.position = transform.position;
                    pursuitInferenceAgent.target = pursuitBehavior.target.Value;
                    pursuitInferenceAgent.RequestDecision();
                    var vector = pursuitInferenceAgent.output;
                    vector.y = pursuitBehavior.GetPursuitVector().Value.y;
                    nextRotation = Quaternion.LookRotation(vector, Vector3.up);
                }
            }
        }

        if (isWanderingActive)
        {
            wanderingBehavior.Update(Time.deltaTime);
            nextRotation = Quaternion.Slerp(transform.rotation, wanderingBehavior.targetRotation, Time.deltaTime * wanderingBehavior.directionChangeInterval);
        }

        var encirclingVector = encirclingBehavior.GetEncirclingVector(currentHuman?.boid);
        if (isEncircleHumanActive && encirclingVector != Vector3.zero)
        {
            guidingBehavior.Init(droneController.id, transform, obstacleBoids);
            nextRotation = Quaternion.Slerp(nextRotation, Quaternion.LookRotation(encirclingVector), 0.05f); //0.1f
        }

        // Make the drone fly towards human
        var guidingVector = guidingBehavior.GetGuidingingVector(GuidetHuman?.boid);
        if (isGuidingHumanActive && guidingVector != Vector3.zero)
        {
            //nextRotation = Quaternion.Slerp(nextRotation, Quaternion.LookRotation(guidingVector), 0.05f); //0.1f
        }

        var cohesionVector = cohesionBehavior.GetCohesionVector(boids);
        if (isCohesionActive && cohesionVector != Vector3.zero)
        {
            nextRotation = Quaternion.Slerp(nextRotation, Quaternion.LookRotation(cohesionVector), 0.015f); //0.1f
        }

        var alignmentVector = alignmentBehavior.GetAlignmentVector(boids);
        if (isAlignmentActive && alignmentVector != Vector3.zero)
        {
            nextRotation = Quaternion.Slerp(nextRotation, Quaternion.LookRotation(alignmentVector), 0.1f); //0.2f
        }

        //separation comes after fencing, so the fence can be ignored in certain cases
        var separationVector = separationBehavior.GetSeparationVector(boids);
        if (separationVector != Vector3.zero)
        {
            nextRotation = Quaternion.Slerp(nextRotation, Quaternion.LookRotation(separationVector), 1f);
        }

        nextRotationEuler = nextRotation.eulerAngles;
        rb.MoveRotation(nextRotation);

        //always use fencing after wandering
        if (isUpperFencingActive)
        {
            var upperFencingVector = upperFencingBehavior.GetCenterVectorWhenAtFence();
            if (upperFencingVector.HasValue)
            {
                rb.MoveRotation(Quaternion.LookRotation(upperFencingVector.Value));
            }
        }
        else
        {
            var lowerFencingVector = lowerFencingBehavior.GetCenterVectorWhenAtFence();
            if (lowerFencingVector.HasValue)
            {
                rb.MoveRotation(Quaternion.LookRotation(lowerFencingVector.Value));
            }
        }

        var distanceBetweenLeaderandBody = Vector3.Distance(drone.position, transform.position);

        if (isWanderingActive || (isPursuitActive && !pursuitBehavior.HasDroneReachedTarget()))
        {
            if (distanceBetweenLeaderandBody < 0.8f) // stick of the carrot
            {
                var forward = transform.TransformDirection(Vector3.forward);
                rb.MovePosition(transform.position + forward * velocity * Time.deltaTime);
            }
        }

        if (leadingDroneActive)
        {
            droneTime += Time.deltaTime;
            if (droneTime > 0.4f)
            {
                if (Vector3.Distance(drone.position, transform.position) > 0.001f)
                {
                    droneController.targetPosition = transform.position;
                    droneController.GetConnection().MoveTo(droneController.id, 0, ComputeDuration(drone.position, transform.position), transform.position.x, transform.position.z, transform.position.y, 0);
                }
                droneTime = 0;
            }
        }
    }

    internal float ComputeDuration(Vector3 from, Vector3 to)
    {
        float distance = Vector3.Distance(from, to);
        return Math.Max((distance / velocity), 1.2f); //1.4f
    }


    void OnDisable()
    {
        boids.Remove(boid);
    }
}
