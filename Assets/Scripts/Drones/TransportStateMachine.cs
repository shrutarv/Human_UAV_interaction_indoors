using Stateless;
using Stateless.Graph;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TransportStateMachine : MonoBehaviour, IDroneControllerListener
{

    public enum Trigger
    {
        Activate,
        Deactivate,
        Start,
        StartWithCountdown,
        StartWithRandomCountdown,
        DoneStarting,
        Land,
        DoneLanding,
        WanderAlone,
        WanderWithSwarm,
        EncircleHuman,
        SwitchToUpperFence,
        SwitchToLowerFence,
        GoHome,
        DoneGoingHome,
        GotoSource,
        ArriveAtSource,
        GotoSink,
        ArriveAtSink,
        GoBackToWandering,
        Reposition,
        GoToHuman,
        ArriveAtHuman,
        GoToStation,
        ArriveAtStation,
        GuideHuman
        
    }

    public enum State
    {
        Deactivated,
        Landing,
        Landed,
        CountdownToStart,
        Starting,
        Started,
        Wandering,
        WanderingAlone,
        WanderingWithSwarm,
        EncirclingHuman,
        GoingHome,
        RepositioningWhileGoingHome,
        Transporting,
        GoingToSource,
        ArrivedAtSource,
        GoingToSink,
        ArrivedAtSink,
        RepositioningWhileGoingToSink,        
        RepositioningWhileGoingToStation,
        GoingToHuman,
        ArrivedAtHuman,
        GoingToStation,
        ArrivedAtStation,
        GuidingHuman
        
    }

    private State _state = State.Deactivated;
    private StateMachine<State, Trigger> _machine;
    public string state;

    private void SetupStateMachine()
    {
        _machine = new StateMachine<State, Trigger>(() => _state, s => { _state = s; state = s.ToString(); });
        _machine.OnUnhandledTrigger((s, t) => { });

        _machine.Configure(State.Deactivated)
            .Permit(Trigger.Activate, State.Landed);

        _machine.Configure(State.Landing)
            .OnEntry(t => { HandleLanding(); })
            .Permit(Trigger.DoneLanding, State.Landed);

        _machine.Configure(State.Landed)
            .OnEntry(t => { if(isAutonomous) HandleAutonomousLandedBehavior(t); })
            .Permit(Trigger.Deactivate, State.Deactivated)
            .Permit(Trigger.Start, State.Starting)
            .Permit(Trigger.StartWithCountdown, State.CountdownToStart)
            .Permit(Trigger.StartWithRandomCountdown, State.CountdownToStart);

        _machine.Configure(State.CountdownToStart)
            .OnEntry(HandleCountdown)
            .Permit(Trigger.Start, State.Starting);

        _machine.Configure(State.Starting)
            .OnEntry(t => { HandleStarting(); })
            .Permit(Trigger.DoneStarting, State.Started);

        _machine.Configure(State.Started)
            .OnEntry(HandleStarted)
            .Permit(Trigger.WanderAlone, State.WanderingAlone)
            .Permit(Trigger.WanderWithSwarm, State.WanderingWithSwarm)
            .Permit(Trigger.EncircleHuman, State.EncirclingHuman)
            .Permit(Trigger.GoHome, State.GoingHome)
            .Permit(Trigger.Land, State.Landing)
            .Permit(Trigger.GuideHuman, State.GuidingHuman);

        _machine.Configure(State.GoingHome)
            .SubstateOf(State.Started)
            .OnEntry(HandleGoingHome)
            .Ignore(Trigger.GoHome)
            .Permit(Trigger.Reposition, State.RepositioningWhileGoingHome)
            .Permit(Trigger.DoneGoingHome, State.Landing);

        _machine.Configure(State.RepositioningWhileGoingHome)
            .SubstateOf(State.Started)
            .OnEntry(HandleRepositioningWhileGoingHome)
            .Permit(Trigger.GoHome, State.GoingHome);

        _machine.Configure(State.Wandering)
            .SubstateOf(State.Started)
            .Permit(Trigger.GotoSource, State.GoingToSource)
            .Permit(Trigger.GoToHuman, State.GoingToHuman);

        _machine.Configure(State.WanderingAlone)
            .SubstateOf(State.Wandering)
            .OnEntry(HandleWanderingAlone)
            .PermitReentry(Trigger.SwitchToUpperFence)
            .PermitReentry(Trigger.SwitchToLowerFence)
            .Permit(Trigger.EncircleHuman, State.EncirclingHuman)
            .Permit(Trigger.WanderWithSwarm, State.WanderingWithSwarm)
            .Permit(Trigger.GuideHuman, State.GuidingHuman);

        _machine.Configure(State.WanderingWithSwarm)
            .SubstateOf(State.Wandering)
            .OnEntry(HandleWanderingWithSwarm)
            .PermitReentry(Trigger.SwitchToUpperFence)
            .PermitReentry(Trigger.SwitchToLowerFence)
            .Permit(Trigger.EncircleHuman, State.EncirclingHuman)
            .Permit(Trigger.WanderAlone, State.WanderingAlone)
            .Permit(Trigger.GuideHuman, State.GuidingHuman);

        _machine.Configure(State.EncirclingHuman)
            .SubstateOf(State.Wandering)
            .OnEntry(HandleEncirclingHuman)
            .PermitReentry(Trigger.Reposition)
            .Permit(Trigger.WanderWithSwarm, State.WanderingWithSwarm)
            .Permit(Trigger.WanderAlone, State.WanderingAlone)            
            .Permit(Trigger.GuideHuman, State.GuidingHuman);
                    
         /*&]
         _machine.Configure(State.GuidingHuman)
            .SubstateOf(State.Wandering)
            .OnEntry(HandleGuidingHuman)
            .PermitReentry(Trigger.Reposition)
            .Permit(Trigger.WanderWithSwarm, State.WanderingWithSwarm)
            .Permit(Trigger.WanderAlone, State.WanderingAlone)            
            .Permit(Trigger.EncircleHuman, State.EncirclingHuman);
*/
        _machine.Configure(State.Transporting)
            .SubstateOf(State.Started);

        State originWanderingState = State.WanderingWithSwarm;
        _machine.Configure(State.GoingToSource)
            .SubstateOf(State.Transporting)
            .OnEntry(t => { HandleGoingToSource(t); originWanderingState = t.Source; })
            .Permit(Trigger.ArriveAtSource, State.ArrivedAtSource);

        _machine.Configure(State.ArrivedAtSource)
            .SubstateOf(State.Transporting)
            .OnEntry(HandleArrivedAtSource)
            .Permit(Trigger.GotoSink, State.GoingToSink);

        _machine.Configure(State.GoingToSink)
            .SubstateOf(State.Transporting)
            .OnEntry(HandleGoingToSink)
            .Permit(Trigger.Reposition, State.RepositioningWhileGoingToSink)
            .Permit(Trigger.ArriveAtSink, State.ArrivedAtSink);

        _machine.Configure(State.RepositioningWhileGoingToSink)
            .SubstateOf(State.Transporting)
            .OnEntry(HandleRepositioningWhileGoingToSink)
            .Permit(Trigger.GotoSink, State.GoingToSink);

        _machine.Configure(State.ArrivedAtSink)
            .SubstateOf(State.Transporting)
            .OnEntry(HandleArrivedAtSink)
            .PermitDynamic(Trigger.GoBackToWandering, () => { return originWanderingState; });

        
         _machine.Configure(State.GuidingHuman)
            .SubstateOf(State.Wandering)
            .Permit(Trigger.GoToHuman, State.GoingToHuman);
        
        // SHRUTARV : Added code to include guide human to rack behavior
        State originWanderingState2 = State.WanderingWithSwarm;
        _machine.Configure(State.GoingToHuman)
            .SubstateOf(State.GuidingHuman)
            .OnEntry(t => { HandleGoingToHuman(t); originWanderingState2 = t.Source; })
            .Permit(Trigger.ArriveAtHuman, State.ArrivedAtHuman)
            .Permit(Trigger.GoToStation, State.GoingToStation);
 
        _machine.Configure(State.ArrivedAtHuman)
            .SubstateOf(State.GuidingHuman)
            .OnEntry(HandleArrivedAtHuman)
            .Permit(Trigger.GoToStation, State.GoingToStation);

        _machine.Configure(State.GoingToStation)
            .SubstateOf(State.GuidingHuman)
            .OnEntry(HandleGoingToStation)
            .Permit(Trigger.Reposition, State.RepositioningWhileGoingToStation)   //Check reposition
            .Permit(Trigger.ArriveAtStation, State.ArrivedAtStation);

        _machine.Configure(State.RepositioningWhileGoingToStation)
            .SubstateOf(State.GuidingHuman)
            .OnEntry(HandleRepositioningWhileGoingToStation)
            .Permit(Trigger.GoToStation, State.GoingToStation);

        _machine.Configure(State.ArrivedAtStation)
            .SubstateOf(State.GuidingHuman)
            .OnEntry(HandleArrivedAtStation)
            .PermitDynamic(Trigger.GoBackToWandering, () => { return originWanderingState; });
        
       

    }

    public void Fire(Trigger trigger)
    {
        _machine.Fire(trigger);
    }

    LeaderController leaderController;
    public bool isAutonomous = true;
    public float maxCountdown = 1f;
    public float Countdown { get; set; } = 0;
    private bool isCountdownActive = false;

    public float lastDistanceToHome = 0;
    public float timeDistanceToHomeDidNotDecrease = 0;
    public float repositionWhileGoingHomeTime = 0;

    public float lastDistanceToSink = 0;
    public float timeDistanceToSinkDidNotDecrease = 0;
    public float repositionWhileGoingToSinkTime = 0;

    public float lastDistanceToStation = 0;
    public float timeDistanceToStationDidNotDecrease = 0;
    public float repositionWhileGoingToStationTime = 0;

    public float lastDistanceToEncircleTarget = 0;
    public float timeDistanceToEncircleTargetDidNotDecrease = 0;
    public float repositionWhileEncirclingTime = 0;

    private TransportOrder TransportOrder;
    private GuideHumanOrder Guide;
    public bool numberPressed = false;
    private bool enterPressed = false;
    // Start is called before the first frame update
    void Start()
    {
        SetupStateMachine();
        leaderController = GetComponent<LeaderController>();
        leaderController.droneController.AddListener(this);
    }

    // Update is called once per frame
    void Update()
    {
        state = _machine.State.ToString();

        if (isCountdownActive)
        {
            RunCountdown();
        }

        if (_machine.IsInState(State.GoingHome))
        {
            MonitorGoingHome();
        }

        if (_machine.IsInState(State.RepositioningWhileGoingHome))
        {
            MonitorRepositioningWhileGoingHome();
        }

        if (_machine.IsInState(State.GoingToSource))
        {
            MonitorGoingToSource();
        }

        if (_machine.IsInState(State.GoingToSink))
        {
            MonitorGoingToSink();
        }

        if (_machine.IsInState(State.RepositioningWhileGoingToSink))
        {
            MonitorRepositioningWhileGoingToSink();
        }

        if (_machine.IsInState(State.EncirclingHuman))
        {
            MonitorEncirclingHuman();
        }

        if (_machine.IsInState(State.GoingToHuman))
        {
            // Debug.Log("In state Going to human");
            MonitorGoingToHuman();
        }

        if (_machine.IsInState(State.GuidingHuman))
        {
            //if (CanGuideHuman())
            //{
                //Debug.Log("Guiding Human");
           // }
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (CanGuideHuman() && !enterPressed)
            {
                Debug.Log("Triggering GoToHuman");
                Fire(Trigger.GoToHuman);
                enterPressed = true;
            }
        }
        // When 1 is pressed, fire trigger.
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (CanGuideHuman())
            {
                numberPressed = true;
                Debug.Log("Arrived. Now go to Station");
                Fire(Trigger.ArriveAtHuman);
                Fire(Trigger.GoToStation);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            if (CanGuideHuman() && numberPressed)
            {
                Debug.Log("Arrived. Now go back to Station");
                Fire(Trigger.ArriveAtStation);
                Fire(Trigger.GoBackToWandering);
            }
        }

/*&]
        //SHRUTARV: Added code below for guiding humans
         if (_machine.IsInState(State.GoingToHuman))
        {
            MonitorGoingToHuman();
        }

        if (_machine.IsInState(State.GoingToStation))
        {
            MonitorGoingToStation();
        }

       // if (_machine.IsInState(State.HandleRepositioningWhileGoingToStation))
        //{
        //    MonitorRepositioningWhileGoingToStation();
        //}
*/
    }

    public bool CanTransport()
    {
        return _machine.IsInState(State.Wandering);
    }

    public bool CanGuideHuman()
    {
        //Debug.Log("Checking if can guide human");
        return _machine.IsInState(State.GuidingHuman);
    }
/*&]
    public void GuideHuman(GuideHumanOrder GHOrder)
    {
        if (CanGuideHuman())
        {
            GuideHumanOrder = GHOrder;
            Fire(Trigger.GotoHuman);
        }
    }
*/
    public void Transport(TransportOrder order)
    {
        if (CanTransport())
        {
            TransportOrder = order;
            Fire(Trigger.GotoSource);
        }
    }

    private void HandleCountdown(StateMachine<State, Trigger>.Transition t)
    {
        if (t.Trigger == Trigger.StartWithRandomCountdown)
        {
            Countdown = UnityEngine.Random.Range(0, maxCountdown);
        }
        isCountdownActive = true;
    }

    private void RunCountdown()
    {
        if (Countdown > 0)
        {
            Countdown -= Time.deltaTime;
        }
        else
        {
            isCountdownActive = false;
            _machine.Fire(Trigger.Start);
        }
    }

    private void HandleLanding()
    {
        leaderController.targetPosition = Vector3.zero;
        leaderController.pursuitBehavior.SetTarget(null);
        leaderController.leadingDroneActive = false;
        leaderController.isPursuitActive = false;
        leaderController.isEncircleHumanActive = false;
        leaderController.isAlignmentActive = false;
        leaderController.isCohesionActive = false;
        leaderController.isWanderingActive = false;
        leaderController.isGuidingHumanActive = false;
        //leaderController.droneController.DroneLand();
    }

    private void HandleAutonomousLandedBehavior(StateMachine<State, Trigger>.Transition t)
    {
        //Automatically start on activation, otherwise stay landed
        if(t.Source == State.Deactivated)
        {
            _machine.Fire(Trigger.StartWithCountdown);
        } else
        {
            Fire(Trigger.Deactivate);
        }
    }

    private void HandleStarting()
    {
        leaderController.boid.SeparationDistance = 0.6f;
        leaderController.targetPosition = Vector3.zero;
        leaderController.pursuitBehavior.SetTarget(null);
        leaderController.leadingDroneActive = false;
        leaderController.isPursuitActive = false;
        leaderController.isEncircleHumanActive = false;
        leaderController.isAlignmentActive = false;
        leaderController.isCohesionActive = false;
        leaderController.isWanderingActive = false;
        leaderController.isGuidingHumanActive = false;
        leaderController.droneController.DroneStart();
    }

    private void HandleStarted(StateMachine<State, Trigger>.Transition t)
    {
        if(t.Source == State.Starting)
        {
            _machine.Fire(Trigger.WanderAlone);
        }
    }

    private void HandleGoingHome(StateMachine<State, Trigger>.Transition t)
    {
        leaderController.isUpperFencingActive = true;

        var homePos = leaderController.droneController.homeHoverPosition;
        leaderController.targetPosition = homePos;
        leaderController.pursuitBehavior.SetTarget(homePos);

        leaderController.leadingDroneActive = true;
        leaderController.isPursuitActive = true;
        leaderController.isEncircleHumanActive = false;
        leaderController.isAlignmentActive = false;
        leaderController.isCohesionActive = false;
        leaderController.isWanderingActive = false;
        leaderController.isGuidingHumanActive = false;
    }

    private void MonitorGoingHome()
    {
        var distanceToHome = leaderController.pursuitBehavior.GetDistanceFromLeaderToTarget();
        var distanceChange = Math.Abs(lastDistanceToHome - distanceToHome);

        if (distanceChange < 0.05f)
        {
            timeDistanceToHomeDidNotDecrease += Time.deltaTime;
        }
        else
        {
            timeDistanceToHomeDidNotDecrease = 0;
        }

        if (timeDistanceToHomeDidNotDecrease > 5)
        {
            leaderController.boid.SeparationDistance = 0.6f;
            Fire(Trigger.Reposition);
        }

        if (leaderController.pursuitBehavior.GetDistanceFromDroneToTarget() < 0.2f)
        {
            leaderController.boid.SeparationDistance = 0.3f;
            leaderController.leadingDroneActive = false;
			leaderController.droneController.DroneMoveHomeAndLand(); //SHRUTARV floor-based landing
			//leaderController.droneController.DroneMoveHome(); //NILS charging station landing
            Fire(Trigger.DoneGoingHome);
        }
        lastDistanceToHome = distanceToHome;
    }

    private void HandleRepositioningWhileGoingHome(StateMachine<State, Trigger>.Transition t)
    {
        repositionWhileGoingHomeTime = UnityEngine.Random.Range(0.5f, 2f);

        leaderController.isUpperFencingActive = true;
        leaderController.targetPosition = Vector3.zero;
        leaderController.pursuitBehavior.SetTarget(null);
        leaderController.leadingDroneActive = true;
        leaderController.isPursuitActive = false;
        leaderController.isEncircleHumanActive = false;
        leaderController.isAlignmentActive = false;
        leaderController.isCohesionActive = false;
        leaderController.isWanderingActive = true;
        leaderController.isGuidingHumanActive = false;
    }

    private void MonitorRepositioningWhileGoingHome()
    {
        if (repositionWhileGoingHomeTime < 0)
        {
            repositionWhileGoingHomeTime = 0;
            Fire(Trigger.GoHome);
        }
        else
        {
            repositionWhileGoingHomeTime -= Time.deltaTime;
        }
    }


    private void HandleWanderingAlone(StateMachine<State, Trigger>.Transition t)
    {
        if (t.Trigger == Trigger.SwitchToUpperFence) leaderController.isUpperFencingActive = true; //i should not have done this
        if (t.Trigger == Trigger.SwitchToLowerFence) leaderController.isUpperFencingActive = false;
        leaderController.targetPosition = Vector3.zero;
        leaderController.pursuitBehavior.SetTarget(null);
        leaderController.leadingDroneActive = true;
        leaderController.isPursuitActive = false;
        leaderController.isEncircleHumanActive = false;
        leaderController.isAlignmentActive = false;
        leaderController.isCohesionActive = false;
        leaderController.isWanderingActive = true;
        leaderController.isGuidingHumanActive = false;
    }

    private void HandleWanderingWithSwarm(StateMachine<State, Trigger>.Transition t)
    {
        if (t.Trigger == Trigger.SwitchToUpperFence) leaderController.isUpperFencingActive = true; //i should not have done this
        if (t.Trigger == Trigger.SwitchToLowerFence) leaderController.isUpperFencingActive = false;
        leaderController.targetPosition = Vector3.zero;
        leaderController.pursuitBehavior.SetTarget(null);
        leaderController.leadingDroneActive = true;
        leaderController.isPursuitActive = false;
        leaderController.isEncircleHumanActive = false;
        leaderController.isAlignmentActive = true;
        leaderController.isCohesionActive = true;
        leaderController.isWanderingActive = true;
        leaderController.isGuidingHumanActive = false;
    }


    // public float lastDistanceToEncircleTarget = 0;
    //public float timeDistanceToEncircleTargetDidNotDecrease = 0;
    //public float repositionWhileEncirclingTime = 0;

    private void HandleEncirclingHuman(StateMachine<State, Trigger>.Transition t)
    {
        if (t.Trigger == Trigger.SwitchToUpperFence) leaderController.isUpperFencingActive = true; //i should not have done this
        if (t.Trigger == Trigger.SwitchToLowerFence) leaderController.isUpperFencingActive = false;
        leaderController.currentHuman = GameObject.Find("Human2").GetComponentInChildren<ObstacleBoid>();//leaderController.encirclingBehavior.GetRandomObstacleBoid();
        leaderController.encircleAngle = UnityEngine.Random.Range(0, 2 * Mathf.PI);
        leaderController.encircleDistance = leaderController.currentHuman.boid.SeparationDistance + UnityEngine.Random.Range(0, 1f);
        leaderController.targetPosition = leaderController.encirclingBehavior.GetEncirclingPosition(leaderController.currentHuman.boid, leaderController.encircleAngle, leaderController.encircleDistance);
        leaderController.pursuitBehavior.SetTarget(leaderController.targetPosition);
        leaderController.leadingDroneActive = true;
        leaderController.isPursuitActive = false;
        leaderController.isEncircleHumanActive = true;
        leaderController.isAlignmentActive = false;
        leaderController.isCohesionActive = false;
        leaderController.isWanderingActive = true;
    }

    private void HandleGuidingHuman(StateMachine<State, Trigger>.Transition t)
    {
        if (t.Trigger == Trigger.SwitchToUpperFence) leaderController.isUpperFencingActive = true; //i should not have done this
        if (t.Trigger == Trigger.SwitchToLowerFence) leaderController.isUpperFencingActive = false;
        leaderController.currentHuman = GameObject.Find("Human").GetComponentInChildren<ObstacleBoid>();//leaderController.encirclingBehavior.GetRandomObstacleBoid();
        //leaderController.encircleAngle = UnityEngine.Random.Range(0, 2 * Mathf.PI);
        leaderController.approachDistance = leaderController.currentHuman.boid.SeparationDistance + UnityEngine.Random.Range(0, 1f);
        leaderController.targetPosition = GameObject.Find("Human").transform.position;
        //leaderController.targetPosition = leaderController.encirclingBehavior.GetEncirclingPosition(leaderController.currentHuman.boid, leaderController.encircleAngle, leaderController.encircleDistance);
        leaderController.pursuitBehavior.SetTarget(leaderController.targetPosition);
        leaderController.leadingDroneActive = true;
        leaderController.isPursuitActive = false;
        leaderController.isGuidingHumanActive = true;
        leaderController.isAlignmentActive = false;
        leaderController.isCohesionActive = false;
        leaderController.isWanderingActive = true;
    }

    private void MonitorEncirclingHuman()
    {

        //var distanceToEncircleTarget = leaderController.pursuitBehavior.GetDistanceFromLeaderToTarget();
        //var distanceChange = Math.Abs(lastDistanceToEncircleTarget - distanceToEncircleTarget);

        //if (distanceChange < 0.05f)
        //{
        //    timeDistanceToEncircleTargetDidNotDecrease += Time.deltaTime;
        //}
        //else
        //{
        //    timeDistanceToEncircleTargetDidNotDecrease = 0;
        //}

        //if (timeDistanceToEncircleTargetDidNotDecrease > 5)
        //{
        //    Fire(Trigger.Reposition);
        //}

        //if (leaderController.pursuitBehavior.HasDroneReachedTarget())
        //{
        //    Fire(Trigger.Reposition);
        //}
        //lastDistanceToEncircleTarget = distanceToEncircleTarget;
    }

    private void HandleGoingToSource(StateMachine<State, Trigger>.Transition t)
    {
        leaderController.isUpperFencingActive = false;

        leaderController.pursuitBehavior.SetTarget(TransportOrder.source.transform.position);

        leaderController.leadingDroneActive = true;
        leaderController.isPursuitActive = true;
        leaderController.isEncircleHumanActive = false;
        leaderController.isAlignmentActive = false;
        leaderController.isCohesionActive = false;
        leaderController.isWanderingActive = false;
        leaderController.isGuidingHumanActive = false;
    }

    private void HandleGoingToHuman(StateMachine<State, Trigger>.Transition t)
    {
         //Debug.Log(drone.position);
        //Debug.Log("Inside Handle Going to human");
        leaderController.isUpperFencingActive = false;
        //leaderController.GuideHuman = GameObject.Find("Human2").GetComponentInChildren<ObstacleBoid>();
        leaderController.targetPosition = GameObject.Find("Human").transform.position;
        leaderController.targetPosition.y = 3.0f;
        leaderController.pursuitBehavior.SetTarget(leaderController.targetPosition);
       
        leaderController.leadingDroneActive = true;
        leaderController.isPursuitActive = true;
        leaderController.isEncircleHumanActive = false;
        leaderController.isAlignmentActive = false;
        leaderController.isCohesionActive = false;
        leaderController.isWanderingActive = false;
    }

    private void MonitorGoingToHuman()
    {
        if (leaderController.pursuitBehavior.HasDroneReachedTarget())
        {
            Fire(Trigger.ArriveAtHuman);
        }
    }

    private void HandleArrivedAtHuman(StateMachine<State, Trigger>.Transition t)
    {
        Debug.Log("Arrived at human");
        Fire(Trigger.GoToStation);
    }

    private void HandleGoingToStation(StateMachine<State, Trigger>.Transition t)
    {
        leaderController.isUpperFencingActive = false;

        leaderController.targetPosition = GameObject.Find("Station").transform.position;
        leaderController.targetPosition.y = 3.0f;
        //Debug.Log("leaderController.targetPosition" + leaderController.targetPosition);
        leaderController.pursuitBehavior.SetTarget(leaderController.targetPosition);

        leaderController.leadingDroneActive = true;
        leaderController.isPursuitActive = true;
        leaderController.isEncircleHumanActive = false;
        leaderController.isAlignmentActive = false;
        leaderController.isCohesionActive = false;
        leaderController.isWanderingActive = false;
    }

    private void MonitorGoingToStation()
    {
        //GuideHumanOrder.load.transform.position = leaderController.droneController.transform.position;

        var distanceToStation = leaderController.pursuitBehavior.GetDistanceFromLeaderToTarget();
        var distanceChange = Math.Abs(lastDistanceToStation - distanceToStation);

        if(distanceChange < 0.5f)
        {
            timeDistanceToStationDidNotDecrease += Time.deltaTime;
        } else
        {
            timeDistanceToStationDidNotDecrease = 0;
        }

        if(timeDistanceToStationDidNotDecrease > 5)
        {
            Fire(Trigger.Reposition);
        }

        if (leaderController.pursuitBehavior.HasDroneReachedTarget())
        {
            Fire(Trigger.ArriveAtStation);
        }
        lastDistanceToStation = distanceToStation;
    }
    
    private void HandleRepositioningWhileGoingToStation(StateMachine<State, Trigger>.Transition t)
    {
        repositionWhileGoingToStationTime = UnityEngine.Random.Range(1, 5);

        leaderController.isUpperFencingActive = false;
        leaderController.targetPosition = Vector3.zero;
        leaderController.pursuitBehavior.SetTarget(null);
        leaderController.leadingDroneActive = true;
        leaderController.isPursuitActive = false;
        leaderController.isEncircleHumanActive = false;
        leaderController.isAlignmentActive = false;
        leaderController.isCohesionActive = false;
        leaderController.isWanderingActive = true;
    }
/*&]
    private void MonitorRepositioningWhileGoingToStation()
    {
        TransportOrder.load.transform.position = leaderController.droneController.transform.position;
        if (repositionWhileGoingToStationTime < 0)
        {
            repositionWhileGoingToStationTime = 0;
            Fire(Trigger.GoToStation);
        } else
        {
            repositionWhileGoingToStationTime -= Time.deltaTime;
        }
    }
*/
    private void MonitorGoingToSource()
    {
        if (leaderController.pursuitBehavior.HasDroneReachedTarget())
        {
            Fire(Trigger.ArriveAtSource);
        }
    }

    private void HandleArrivedAtSource(StateMachine<State, Trigger>.Transition t)
    {
        Fire(Trigger.GotoSink);
    }

    private void HandleArrivedAtStation(StateMachine<State, Trigger>.Transition t)
    {
        leaderController.isUpperFencingActive = true;
        Fire(Trigger.GoBackToWandering);
    }

    private void HandleGoingToSink(StateMachine<State, Trigger>.Transition t)
    {
        leaderController.isUpperFencingActive = false;

        leaderController.pursuitBehavior.SetTarget(TransportOrder.sink.transform.position);

        leaderController.leadingDroneActive = true;
        leaderController.isPursuitActive = true;
        leaderController.isEncircleHumanActive = false;
        leaderController.isAlignmentActive = false;
        leaderController.isCohesionActive = false;
        leaderController.isWanderingActive = false;
        leaderController.isGuidingHumanActive = false;
    }

    private void MonitorGoingToSink()
    {
        TransportOrder.load.transform.position = leaderController.droneController.transform.position;

        var distanceToSink = leaderController.pursuitBehavior.GetDistanceFromLeaderToTarget();
        var distanceChange = Math.Abs(lastDistanceToSink - distanceToSink);

        if(distanceChange < 0.05f)
        {
            timeDistanceToSinkDidNotDecrease += Time.deltaTime;
        } else
        {
            timeDistanceToSinkDidNotDecrease = 0;
        }

        if(timeDistanceToSinkDidNotDecrease > 5)
        {
            Fire(Trigger.Reposition);
        }

        if (leaderController.pursuitBehavior.HasDroneReachedTarget())
        {
            Fire(Trigger.ArriveAtSink);
        }
        lastDistanceToSink = distanceToSink;
    }

    private void HandleRepositioningWhileGoingToSink(StateMachine<State, Trigger>.Transition t)
    {
        repositionWhileGoingToSinkTime = UnityEngine.Random.Range(1, 5);

        leaderController.isUpperFencingActive = false;
        leaderController.targetPosition = Vector3.zero;
        leaderController.pursuitBehavior.SetTarget(null);
        leaderController.leadingDroneActive = true;
        leaderController.isPursuitActive = false;
        leaderController.isEncircleHumanActive = false;
        leaderController.isAlignmentActive = false;
        leaderController.isCohesionActive = false;
        leaderController.isWanderingActive = true;
        leaderController.isGuidingHumanActive = false;
    }

    private void MonitorRepositioningWhileGoingToSink()
    {
        TransportOrder.load.transform.position = leaderController.droneController.transform.position;
        if (repositionWhileGoingToSinkTime < 0)
        {
            repositionWhileGoingToSinkTime = 0;
            Fire(Trigger.GotoSink);
        } else
        {
            repositionWhileGoingToSinkTime -= Time.deltaTime;
        }
    }

    private void HandleArrivedAtSink(StateMachine<State, Trigger>.Transition t)
    {
        TransportOrder.Finished();
        TransportOrder = null;
        leaderController.isUpperFencingActive = true;
        Fire(Trigger.GoBackToWandering);
    }

    public void DroneStarted()
    {
        _machine.Fire(Trigger.DoneStarting);
    }

    public void DroneLanded()
    {
        _machine.Fire(Trigger.DoneLanding);
    }

    [CustomEditor(typeof(TransportStateMachine))]
    public class TransportMetaBehaviorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            TransportStateMachine t = (TransportStateMachine)target;

            if (GUILayout.Button("Activate"))
            {
                t._machine.Fire(Trigger.Activate);
            }

            if (GUILayout.Button("Deactivate"))
            {
                t._machine.Fire(Trigger.Deactivate);
            }

            if (GUILayout.Button("Start"))
            {
                t._machine.Fire(Trigger.Start);
            }

            if (GUILayout.Button("Start with Random Countdown"))
            {
                t._machine.Fire(Trigger.StartWithRandomCountdown);
            }

            if (GUILayout.Button("Land"))
            {
                t._machine.Fire(Trigger.Land);
            }
            
            if (GUILayout.Button("Guide Human"))
            {
                t._machine.Fire(Trigger.GuideHuman);
            }
            
            if (GUILayout.Button("Encircle Human"))
            {
                t._machine.Fire(Trigger.EncircleHuman);
            }



        }
    }

}
