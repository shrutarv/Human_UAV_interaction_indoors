using Stateless;
using Stateless.Graph;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class TransportStateMachine : MonoBehaviour, IDroneControllerListener
{
    public float startTime = 0;
    public bool arrivedAtHuman = false;
    public bool arrivedAtStation = false;
    public bool goingToStationTemp = false;
    public bool goingToStationTemp2 = false;
    public bool goingToStation2 = false;
    public bool goingToStation = true;
    public bool arrivedAtStationTemp = false;
    public bool arrivedAtStation2 = false;
    public bool waiting = false;
    public bool round1 = true;
    public bool round2 = false;
    public bool arrivedAtStationTemp2 = false;
    public bool waiting2 = false;
    public bool waiting3 = false;
    public bool goingToStation3 = false;
    public bool arrivedAtStation3 = false;
    public bool goingToStation4 = false;
    public bool arrivedAtStation4 = false;
    public bool flagFinal = false;
    public bool goingToStationCorner1 = false;
    public bool goingToStationCorner2 = false;
    public bool arrivedAtStationCorner1 = false;
    public bool arrivedAtStationCorner2 = false;
    public bool stopButtonPressed = false;


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
        GoToStationTemp,
        GoToStationTemp2,
        GoToStationCorner1,
        GoToStationCorner2,
        GoToStartAisle,
        ArriveAtStartAisle,
        GoToStation2,
        ArriveAtStation2,
        GoToStation3,
        ArriveAtStation3,
        GoToStation4,
        ArriveAtStation4,

        GoWait,
        GoWait2,
        LandOneDrone,
        StartOneDrone,
        GuideHuman,
        GoWait3,
        GoWait4,
        WaitForHuman
        
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
        GoingToStationTemp,
        GoingToStationTemp2,
        GoingToStationCorner1,
        GoingToStationCorner2,
        GoingToStation2,
        GuidingHuman,
        ArrivedAtStartAisle,
        GoingToStartAisle,
        ArrivedAtStation2,
        GoingToStation3,
        ArrivedAtStation3,
        GoingToStation4,
        ArrivedAtStation4,

        Waiting,
        Waiting2,
        Waiting3,
        Waiting4,
        LandedOneDrone,
        StartedOneDrone,
        WaitingForHuman  

        
    }

    private State _state = State.Deactivated;
    private StateMachine<State, Trigger> _machine;
    public string state;
    public bool flagOnce = false;

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

        _machine.Configure(State.GoingToStation2)
            .SubstateOf(State.Wandering)
            .Permit(Trigger.GoToStation3, State.GoingToStation3);
        
        // SHRUTARV : Added code to include guide human to rack behavior
        State originWanderingState2 = State.WanderingWithSwarm;
        _machine.Configure(State.GoingToHuman)
            .SubstateOf(State.GuidingHuman)
            .OnEntry(t => { HandleGoingToHuman(t); originWanderingState2 = t.Source; })
            .Permit(Trigger.ArriveAtHuman, State.ArrivedAtHuman)
            .Permit(Trigger.GoToStationTemp, State.GoingToStationTemp)
            .Permit(Trigger.GoToStation2, State.GoingToStation2)
            .Permit(Trigger.GoToStation, State.GoingToStation);
 
        _machine.Configure(State.ArrivedAtHuman)
            .SubstateOf(State.GuidingHuman)
            .OnEntry(HandleArrivedAtHuman)
            .Permit(Trigger.GoToStation, State.GoingToStation)
            .Permit(Trigger.GoToStationTemp, State.GoingToStationTemp)
            .Permit(Trigger.GoToStationTemp2, State.GoingToStationTemp2)
            .Permit(Trigger.LandOneDrone, State.LandedOneDrone)
            .Permit(Trigger.GoToStation2, State.GoingToStation2)
            .Permit(Trigger.GoToStation3, State.GoingToStation3)
            .Permit(Trigger.GoToStation4, State.GoingToStation4)
            .Permit(Trigger.GoToStationCorner1, State.GoingToStationCorner1)
            .Permit(Trigger.GoToStationCorner2, State.GoingToStationCorner2)
            .Permit(Trigger.GoWait, State.Waiting);

        _machine.Configure(State.WaitingForHuman)
            .SubstateOf(State.GuidingHuman)
            .OnEntry(HandleWaitingForHuman)
            .Permit(Trigger.GoToStation, State.GoingToStation)
            .Permit(Trigger.GoToStationTemp, State.GoingToStationTemp)
            .Permit(Trigger.GoToStationTemp2, State.GoingToStationTemp2)
            .Permit(Trigger.LandOneDrone, State.LandedOneDrone)
            .Permit(Trigger.GoToStation2, State.GoingToStation2)
            .Permit(Trigger.GoToStation3, State.GoingToStation3)
            .Permit(Trigger.GoToStation4, State.GoingToStation4)
            .Permit(Trigger.GoToStationCorner1, State.GoingToStationCorner1)
            .Permit(Trigger.GoToStationCorner2, State.GoingToStationCorner2)
            .Permit(Trigger.ArriveAtHuman, State.ArrivedAtHuman)
            .Permit(Trigger.GoWait, State.Waiting);


        _machine.Configure(State.GoingToStation)
            .SubstateOf(State.GuidingHuman)
            .OnEntry(HandleGoingToStation)
            .Permit(Trigger.Reposition, State.RepositioningWhileGoingToStation)   //Check reposition
            .Permit(Trigger.LandOneDrone, State.LandedOneDrone)
            .Permit(Trigger.ArriveAtStation, State.ArrivedAtStation)
            .Permit(Trigger.WaitForHuman, State.WaitingForHuman);

        _machine.Configure(State.RepositioningWhileGoingToStation)
            .SubstateOf(State.GuidingHuman)
            .OnEntry(HandleRepositioningWhileGoingToStation)
            .Permit(Trigger.GoToStation, State.GoingToStation);
       
        _machine.Configure(State.ArrivedAtStation)
            .SubstateOf(State.GuidingHuman)
            .OnEntry(HandleArrivedAtStation)
            .Permit(Trigger.GoWait, State.Waiting)
            .Permit(Trigger.GoToStationTemp, State.GoingToStationTemp)
            .Permit(Trigger.GoToStation3, State.GoingToStation3);

        _machine.Configure(State.Waiting)
            .SubstateOf(State.GuidingHuman)
            .OnEntry(HandleWaiting)
            .Permit(Trigger.GoToStationTemp, State.GoingToStationTemp)
            .Permit(Trigger.GoToStationCorner1, State.GoingToStationCorner1)
            .Permit(Trigger.GoWait3, State.Waiting3);

        _machine.Configure(State.Waiting2)
            .SubstateOf(State.GuidingHuman)
            .OnEntry(HandleWaiting2)
            .Permit(Trigger.GoToStationTemp2, State.GoingToStationTemp2);

        _machine.Configure(State.Waiting3)
            .SubstateOf(State.GuidingHuman)
            .OnEntry(HandleWaiting3)
            .Permit(Trigger.GoToStationTemp2, State.GoingToStationTemp2)
            .Permit(Trigger.GoToStationTemp, State.GoingToStationTemp)
            .Permit(Trigger.GoToStation2, State.GoingToStation2);

         _machine.Configure(State.Waiting4)
            .SubstateOf(State.GuidingHuman)
            .OnEntry(HandleWaiting4)
            .Permit(Trigger.GoToStation4, State.GoingToStation4);


        _machine.Configure(State.GoingToStationTemp)
            .SubstateOf(State.GuidingHuman)
            .OnEntry(HandleGoingToStationTemp)
            .Permit(Trigger.GoToStation2, State.GoingToStation2)
            .Permit(Trigger.GoToStationTemp2, State.GoingToStationTemp2)
            .Permit(Trigger.GoToStationCorner2, State.GoingToStationCorner2)
            .Permit(Trigger.WaitForHuman, State.WaitingForHuman);

         _machine.Configure(State.GoingToStationTemp2)
            .SubstateOf(State.GuidingHuman)
            .OnEntry(HandleGoingToStationTemp2)
            .Permit(Trigger.GoToStation, State.GoingToStation)
            .Permit(Trigger.GoToStation3, State.GoingToStation3)
            .Permit(Trigger.GoToStation4, State.GoingToStation4)
            .Permit(Trigger.WaitForHuman, State.WaitingForHuman)
             .Permit(Trigger.GoWait4, State.Waiting4);

        _machine.Configure(State.GoingToStationCorner1)
            .SubstateOf(State.GuidingHuman)
            .OnEntry(HandleGoingToStationCorner1)
            .Permit(Trigger.GoToStation2, State.GoingToStation2)
            .Permit(Trigger.GoToStationTemp, State.GoingToStationTemp)
            .Permit(Trigger.GoToStationCorner2, State.GoingToStationCorner2);
            
         _machine.Configure(State.GoingToStationCorner2)
            .SubstateOf(State.GuidingHuman)
            .OnEntry(HandleGoingToStationCorner2)
            .Permit(Trigger.GoToStation2, State.GoingToStation2)
            .Permit(Trigger.GoToStation3, State.GoingToStation3)
            .Permit(Trigger.GoToStationTemp2, State.GoingToStationTemp2);

        _machine.Configure(State.GoingToStation3)
            .SubstateOf(State.GuidingHuman)
            .OnEntry(HandleGoingToStation3)
            .Permit(Trigger.LandOneDrone, State.LandedOneDrone)
            .Permit(Trigger.GoWait2, State.Waiting2)
            .Permit(Trigger.GoToStationTemp, State.GoingToStationTemp)
            .Permit(Trigger.GoWait3, State.Waiting3)
            .Permit(Trigger.WaitForHuman, State.WaitingForHuman);;

        _machine.Configure(State.GoingToStation2)
            .SubstateOf(State.GuidingHuman)
            .OnEntry(HandleGoingToStation2)
            .Permit(Trigger.GoToStation, State.GoingToStation)
            .Permit(Trigger.LandOneDrone, State.LandedOneDrone)
            .Permit(Trigger.GoWait2, State.Waiting2)
            .Permit(Trigger.WaitForHuman, State.WaitingForHuman)
            .Permit(Trigger.GoToStationTemp2, State.GoingToStationTemp2);

        _machine.Configure(State.GoingToStation4)
            .SubstateOf(State.GuidingHuman)
            .OnEntry(HandleGoingToStation4)
            .Permit(Trigger.LandOneDrone, State.LandedOneDrone)
            .Permit(Trigger.GoWait2, State.Waiting2)
            .Permit(Trigger.GoToStationTemp, State.GoingToStationTemp)
            .Permit(Trigger.GoHome, State.GoingHome)
            .Permit(Trigger.WaitForHuman, State.WaitingForHuman);

         _machine.Configure(State.StartedOneDrone)
            .OnEntry(HandleStartedOneDrone)
            .Permit(Trigger.WanderAlone, State.WanderingAlone)
            .Permit(Trigger.WanderWithSwarm, State.WanderingWithSwarm)
            .Permit(Trigger.EncircleHuman, State.EncirclingHuman)
            .Permit(Trigger.GoHome, State.GoingHome)
            .Permit(Trigger.Land, State.Landing)
            .Permit(Trigger.GuideHuman, State.GuidingHuman);

        _machine.Configure(State.LandedOneDrone)
            .SubstateOf(State.Started)
            .OnEntry(HandleLandedOneDrone)
            .Ignore(Trigger.GoHome)
            .Permit(Trigger.Reposition, State.RepositioningWhileGoingHome)
            .Permit(Trigger.DoneGoingHome, State.Landing);

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
    public static float globalDistanceBetweenDroneAndHuman = 1.6f;
    private TransportOrder TransportOrder;
    private GuideHumanOrder Guide;
    public bool numberPressed = false;
    private bool enterPressed = false;
    public bool waiting4 = false;
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

        if (_machine.IsInState(State.GoingToStation))
        {
            MonitorGoingToStation();
        }

        if (_machine.IsInState(State.GoingToStationTemp))
        {
            MonitorGoingToStationTemp();
        }

        if (_machine.IsInState(State.GoingToStationTemp2))
        {
            MonitorGoingToStationTemp2();
        }

        if (_machine.IsInState(State.GoingToStationCorner1))
        {
            MonitorGoingToStationCorner1();
        }

        if (_machine.IsInState(State.GoingToStationCorner2))
        {
            MonitorGoingToStationCorner2();
        }


         if (_machine.IsInState(State.GoingToStation2))
        {
            MonitorGoingToStation2();
        }

        if(_machine.IsInState(State.ArrivedAtHuman))
        {
            MonitorArrivedAtHuman();
        }

        if (_machine.IsInState(State.Waiting))
        {
            MonitorWaiting();
            //MonitorRepositioningWhileGoingToStation();
        }

        if (_machine.IsInState(State.Waiting2))
        {
            MonitorWaiting2();
            //MonitorRepositioningWhileGoingToStation();
        }

        if (_machine.IsInState(State.Waiting3))
        {
            MonitorWaiting3();
            //MonitorRepositioningWhileGoingToStation();
        }

        if (_machine.IsInState(State.GoingToStation3))
        {
            MonitorGoingToStation3();
        }

        if (_machine.IsInState(State.GoingToStation4))
        {
            MonitorGoingToStation4();
        }

        if(_machine.IsInState(State.WaitingForHuman))
        {
            MonitorWaitingForHuman();
        }

        if(_machine.IsInState(State.Waiting4))
        {
            MonitorWaiting4();
        }

        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEquals))
        {
            if (CanGuideHuman() && !enterPressed)
            {
                Debug.Log("StopPressed");
                //Fire(Trigger.GoToHuman);
                stopButtonPressed = true;
                flagOnce = true;
            }
        }
        // WHen q is pressed land all drones
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Land all drones");
            leaderController.droneController.DroneLand();
            Fire(Trigger.Land);
        }
        
        // When 1 is pressed, fire trigger.
        
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.P))
       {
            numberPressed = true;
            Debug.Log("Land drone 1");
            Fire(Trigger.LandOneDrone);
            //Fire(Trigger.GoToStation);
            
        }



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
        Debug.Log("HandleStarting");
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

      private void HandleStartedOneDrone()
    {
        Debug.Log("HandleStartingOneDrone");
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

    private void HandleLandedOneDrone()
    {
        Debug.Log("HandleLandedOneDrone");
        //leaderController.targetPosition = Vector3.zero;
        //leaderController.pursuitBehavior.SetTarget(null);
        leaderController.leadingDroneActive = false;
        leaderController.isPursuitActive = false;
        leaderController.isEncircleHumanActive = false;
        leaderController.isAlignmentActive = false;
        leaderController.isCohesionActive = false;
        leaderController.isWanderingActive = false;
        leaderController.isGuidingHumanActive = false;
        leaderController.droneController.DroneLand1(1);
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
        leaderController.targetPosition.y = 1.0f;
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
        var distanceToHuman = Vector3.Distance(GameObject.Find("Drone").transform.position, GameObject.Find("Human").transform.position);
        
        
         //Debug.Log("Distance to human: " + distanceToHuman);
        if(timeDistanceToStationDidNotDecrease > 5)
        {
            Fire(Trigger.Reposition);
        }

        if (leaderController.pursuitBehavior.HasDroneReachedTarget()|| distanceToHuman < globalDistanceBetweenDroneAndHuman || stopButtonPressed)
        {
            // wait for 2 secs
            if (stopButtonPressed)
            {
                globalDistanceBetweenDroneAndHuman = Vector3.Distance(GameObject.Find("Drone").transform.position, GameObject.Find("Human").transform.position);
                Debug.Log("Global Distance between drone and human: " + globalDistanceBetweenDroneAndHuman);
                stopButtonPressed = false;       
            }
            
            
            if (!arrivedAtHuman)
            {startTime = Time.time;
            arrivedAtHuman = true;
            }
            if ((Time.time - startTime > 2.0f) && arrivedAtHuman)
            {
            
            Fire(Trigger.ArriveAtHuman);
            arrivedAtHuman = false;
            }
        }
    }

    private void HandleArrivedAtHuman(StateMachine<State, Trigger>.Transition t)
    {
        Debug.Log("Arrived at human");
        if(round1)
        {
            if(goingToStationTemp)
            {
                //goingToStationTemp = false;
                Fire(Trigger.GoToStationTemp);
            }
            else if(goingToStation)
            {
                //goingToStation = false;
                Fire(Trigger.GoToStation);
            }
            else if(goingToStation2)
            {
                //goingToStation2 = false;
                Fire(Trigger.GoToStation2);
            }
             else if(goingToStationTemp2)
            {
                //goingToStation2 = false;
                Fire(Trigger.GoToStationTemp2);
            }
            else if(goingToStationCorner1)
            {
                //goingToStation2 = false;
                Fire(Trigger.GoToStationCorner1);
            }
            else if(goingToStationCorner2)
            {
                //goingToStation2 = false;
                Fire(Trigger.GoToStationCorner2);
            }
        }
        else if(round2)
        {
            if(goingToStationTemp)
            {
                //goingToStationTemp = false;
                Fire(Trigger.GoToStationTemp);
            }
            else if(goingToStation)
            {
                //goingToStation = false;
                Fire(Trigger.GoToStation);
            }
            else if(goingToStation2)
            {
                //goingToStation2 = false;
                Fire(Trigger.GoToStation2);
            }
            else if(goingToStationTemp2)
            {
                //goingToStation2 = false;
                Fire(Trigger.GoToStationTemp2);
            }
            else if(goingToStation3)
            {
                //goingToStation2 = false;
                Fire(Trigger.GoToStation3);
            }
            else if(goingToStation4)
            {
                //goingToStation2 = false;
                Fire(Trigger.GoToStation4);
            }
             else if(goingToStationCorner1)
            {
                //goingToStation2 = false;
                Fire(Trigger.GoToStationCorner1);
            }
            else if(goingToStationCorner2)
            {
                //goingToStation2 = false;
                Fire(Trigger.GoToStationCorner2);
            }
        }
        //Fire(Trigger.GoToStation);
    }

    private void HandleWaitingForHuman(StateMachine<State, Trigger>.Transition t)
    {
        Debug.Log("Waiting For Human");
        //get current location of drone
        leaderController.targetPosition = GameObject.Find("Drone").transform.position;
        leaderController.pursuitBehavior.SetTarget(leaderController.targetPosition);

        leaderController.leadingDroneActive = true;
        leaderController.isPursuitActive = true;
        leaderController.isEncircleHumanActive = false;
        leaderController.isAlignmentActive = false;
        leaderController.isCohesionActive = false;
        leaderController.isWanderingActive = false;

    }

    private void MonitorWaitingForHuman()
    {
        var distanceToHuman = Vector3.Distance(GameObject.Find("Drone").transform.position, GameObject.Find("Human").transform.position);
         //Debug.Log("Distance to human: " + distanceToHuman);
        if(timeDistanceToStationDidNotDecrease > 5)
        {
            Fire(Trigger.Reposition);
        }
        //Debug.Log("Distance to human: " + distanceToHuman);
        if (distanceToHuman < globalDistanceBetweenDroneAndHuman)
        {
            Debug.Log(globalDistanceBetweenDroneAndHuman);
            //Debug.Log(arrivedAtHuman);
            // wait for 2 secs
            if (!arrivedAtHuman)
            {startTime = Time.time;
            arrivedAtHuman = true;
            }
            if ((Time.time - startTime > 2.0f) && arrivedAtHuman)
            {
            
            Fire(Trigger.ArriveAtHuman);
            Debug.Log("Trigger Arrive at human");
            
            }
        }
    }

    private void MonitorArrivedAtHuman()
    {
        if (Time.time - startTime > 2.0f) //wait for 2 sec around the human
        {
            if(goingToStationTemp)
            {
                goingToStationTemp = false;
                Fire(Trigger.GoToStationTemp);
            }
            else if(goingToStation)
            {
                goingToStation = false;
                Fire(Trigger.GoToStation);
            }
            else if(goingToStation2)
            {
                goingToStation2 = false;
                Fire(Trigger.GoToStation2);
            }
            else if(goingToStationTemp2)
            {
                goingToStationTemp2 = false;
                Fire(Trigger.GoToStationTemp2);
            }
            else if(goingToStationCorner1)
            {
                goingToStationCorner1 = false;
                Fire(Trigger.GoToStationCorner1);
            }
            else if(goingToStationCorner2)
            {
                goingToStationCorner2 = false;
                Fire(Trigger.GoToStationCorner2);
            }
          
            //Fire(Trigger.GoToStation);
        }
    }

    private void HandleGoingToStation(StateMachine<State, Trigger>.Transition t)
    {
        goingToStation = true;
        Debug.Log("Going to station");
        leaderController.isUpperFencingActive = false;

        leaderController.targetPosition = GameObject.Find("Station").transform.position;
        leaderController.targetPosition.y = 1.0f;
        Debug.Log("leaderController.targetPosition" + leaderController.targetPosition);
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
        goingToStation = true;
        goingToStationTemp = false;
        goingToStation2 = false;
        //GuideHumanOrder.load.transform.position = leaderController.droneController.transform.position;

        // compute distance between drone and station
        var distanceDronetoStation = Vector3.Distance(GameObject.Find("Drone").transform.position, GameObject.Find("Station").transform.position);
        var distanceToStation = leaderController.pursuitBehavior.GetDistanceFromLeaderToTarget();
        var distanceChange = Math.Abs(lastDistanceToStation - distanceToStation);
        var distanceToHuman = Vector3.Distance(GameObject.Find("Drone").transform.position, GameObject.Find("Human").transform.position);
        var currentPosition = GameObject.Find("Drone").transform.position;
        //Debug.Log("Distance to human: " + distanceToHuman);

        // if distance between drone and human is greater than 2.0 then drone flies back to the human
        if (distanceToHuman > globalDistanceBetweenDroneAndHuman + 1.5f)
        {
            //Fire(Trigger.GoToHuman);
            Fire(Trigger.WaitForHuman);
        }

        if(distanceChange < 0.5f)
        {
            timeDistanceToStationDidNotDecrease += Time.deltaTime;
        } else
        {
            timeDistanceToStationDidNotDecrease = 0;
        }
        //Debug.Log("Distance to station: " + distanceDronetoStation);
        if (distanceDronetoStation < 2.5f)
        {
            // wait for 2 secs
            if (!arrivedAtStation)
            {startTime = Time.time;
            arrivedAtStation = true;
            }
            if ((Time.time - startTime > 2.0f) && arrivedAtStation)
            {
            Debug.Log("Arrived at station");
            Fire(Trigger.ArriveAtStation);
            }
        }
        lastDistanceToStation = distanceToStation;
    }
    
        private void HandleArrivedAtStation(StateMachine<State, Trigger>.Transition t)
    {
        leaderController.isUpperFencingActive = true;
        leaderController.targetPosition = GameObject.Find("Station").transform.position;
        leaderController.targetPosition.x = leaderController.targetPosition.x - 4.0f;
        leaderController.targetPosition.z = leaderController.targetPosition.z - 0.5f;
        leaderController.targetPosition.y = 1.0f;
        leaderController.pursuitBehavior.SetTarget(leaderController.targetPosition);
        if (round1)
            Fire(Trigger.GoWait);
        else if (round2)
            Fire(Trigger.GoToStation3);
    }

    private void HandleWaiting()
    {   
        Debug.Log("Waiting");
        
    }

     private void HandleWaiting2()
    {   
        leaderController.isUpperFencingActive = true;
        leaderController.targetPosition = GameObject.Find("Station2").transform.position;
        leaderController.targetPosition.x = leaderController.targetPosition.x + 3.0f;
        leaderController.targetPosition.y = 1.0f;
        leaderController.pursuitBehavior.SetTarget(leaderController.targetPosition);
       
        Debug.Log("Waiting2");
        
    }

    private void MonitorWaiting()
    {
        // Wait for 5 secs
        if (!waiting)
        {    startTime = Time.time;
            waiting = true;
        }
       
        var distanceToHuman = Vector3.Distance(GameObject.Find("Drone").transform.position, GameObject.Find("Human").transform.position);
        if ((Time.time - startTime > 5.0f) && (distanceToHuman < globalDistanceBetweenDroneAndHuman + 1.5f))
        {
            //Fire(Trigger.GoToStationTemp);
            //Fire(Trigger.GoToStationCorner1);
            Fire(Trigger.GoWait3);
            waiting = false;
        }

    }

    private void MonitorWaiting2()
    {
        // Wait for 5 secs
        if (!waiting)
        {    startTime = Time.time;
            waiting = true;
        }
       
        var distanceToHuman = Vector3.Distance(GameObject.Find("Drone").transform.position, GameObject.Find("Human").transform.position);
        if ((Time.time - startTime > 5.0f) && (distanceToHuman < 2.0f))
        {
            Fire(Trigger.GoToStationTemp2);
            waiting = false;
        }

    }

    private void HandleGoingToStationTemp(StateMachine<State, Trigger>.Transition t)
    {
        Debug.Log("Going to station temp");
        leaderController.isUpperFencingActive = false;

        leaderController.targetPosition = GameObject.Find("StationTemp").transform.position;
        leaderController.targetPosition.y = 1.0f;
        Debug.Log("leaderController.targetPosition" + leaderController.targetPosition);
        leaderController.pursuitBehavior.SetTarget(leaderController.targetPosition);

        leaderController.leadingDroneActive = true;
        leaderController.isPursuitActive = true;
        leaderController.isEncircleHumanActive = false;
        leaderController.isAlignmentActive = false;
        leaderController.isCohesionActive = false;
        //leaderController.isWanderingActive = false;
        leaderController.isWanderingActive = true;
    }


    private void MonitorGoingToStationTemp()
    {
        goingToStationTemp = true;
        goingToStation = false;
        goingToStation2 = false;
        goingToStationCorner1 = false;
        goingToStationCorner2 = false; 
        //GuideHumanOrder.load.transform.position = leaderController.droneController.transform.position;

        var distanceToStation = leaderController.pursuitBehavior.GetDistanceFromLeaderToTarget();
        var distanceChange = Math.Abs(lastDistanceToStation - distanceToStation);
        var distanceToHuman = Vector3.Distance(GameObject.Find("Drone").transform.position, GameObject.Find("Human").transform.position);
        //Debug.Log("Distance to human: " + distanceToHuman);
        if(distanceChange < 0.5f)
        {
            timeDistanceToStationDidNotDecrease += Time.deltaTime;
        } else
        {
            timeDistanceToStationDidNotDecrease = 0;
        }
         if (distanceToHuman > globalDistanceBetweenDroneAndHuman + 1.5f)
        {
            Fire(Trigger.GoToHuman);
        }

        if (leaderController.pursuitBehavior.HasDroneReachedTarget()|| distanceToStation < 1.6f)
        {
            // wait for 2 secs
            if (!arrivedAtStationTemp)
            {startTime = Time.time;
            arrivedAtStationTemp = true;
            }
            
            if ((Time.time - startTime > 2.0f) && arrivedAtStationTemp)
            {
            Debug.Log("Arrived at station temp");
            Fire(Trigger.GoToStation2);
            }
            
          
        }
    }

    private void HandleGoingToStationCorner1(StateMachine<State, Trigger>.Transition t)
    {
        Debug.Log("Going to station corner 1");
        leaderController.isUpperFencingActive = true;
        leaderController.isWanderingActive = true;
        leaderController.targetPosition = GameObject.Find("Station").transform.position;
        leaderController.targetPosition.x = leaderController.targetPosition.x - 13.0f;
        //leaderController.targetPosition.y = 1.0f;
        Debug.Log("leaderController.targetPosition" + leaderController.targetPosition);
        leaderController.pursuitBehavior.SetTarget(leaderController.targetPosition);

        
    }

    private void MonitorGoingToStationCorner1()
    {
        //Debug.Log("monitor going to station corner 1");
        goingToStationCorner1 = true;
        goingToStationTemp = false;
        goingToStation = false;
        goingToStation2 = false;
        goingToStation3 = false;
        goingToStation4 = false;
        goingToStationCorner2 = false;
        //GuideHumanOrder.load.transform.position = leaderController.droneController.transform.position;

        var distanceToStation = leaderController.pursuitBehavior.GetDistanceFromLeaderToTarget();
        var distanceChange = Math.Abs(lastDistanceToStation - distanceToStation);
        var distanceToHuman = Vector3.Distance(GameObject.Find("Drone").transform.position, GameObject.Find("Human").transform.position);
        //Debug.Log("Distance to human: " + distanceToHuman);
        
         if (distanceToHuman > 2.0f)
        {
            Fire(Trigger.GoToHuman);
        }
        if (leaderController.pursuitBehavior.HasDroneReachedTarget()|| distanceToStation < 1.6f)
        {
            // wait for 2 secs
            if (!arrivedAtStationCorner1)
            {startTime = Time.time;
            arrivedAtStationCorner1 = true;
            }
            
            if ((Time.time - startTime > 2.0f) && arrivedAtStationCorner1)
            {
            Debug.Log("Arrived at station corner 1");  
            Fire(Trigger.GoToStationTemp);
            }
            
          
        }
    }

    private void HandleGoingToStationCorner2(StateMachine<State, Trigger>.Transition t)
    {
        Debug.Log("Going to station corner 2");
        leaderController.isUpperFencingActive = false;

        leaderController.targetPosition = GameObject.Find("StationCorner2").transform.position;
        leaderController.targetPosition.y = 1.0f;
        Debug.Log("leaderController.targetPosition" + leaderController.targetPosition);
        leaderController.pursuitBehavior.SetTarget(leaderController.targetPosition);

        leaderController.leadingDroneActive = true;
        leaderController.isPursuitActive = true;
        leaderController.isEncircleHumanActive = false;
        leaderController.isAlignmentActive = false;
        leaderController.isCohesionActive = false;
        leaderController.isWanderingActive = false;
    }
    
    private void MonitorGoingToStationCorner2()
    {
        goingToStationCorner2 = true;
        goingToStationTemp = false;
        goingToStation = false;
        goingToStation2 = false;
        goingToStation3 = false;
        goingToStation4 = false;
        goingToStationCorner1 = false;
        //GuideHumanOrder.load.transform.position = leaderController.droneController.transform.position;

        var distanceToStation = leaderController.pursuitBehavior.GetDistanceFromLeaderToTarget();
        var distanceChange = Math.Abs(lastDistanceToStation - distanceToStation);
        var distanceToHuman = Vector3.Distance(GameObject.Find("Drone").transform.position, GameObject.Find("Human").transform.position);
        //Debug.Log("Distance to human: " + distanceToHuman);
        if(distanceChange < 0.5f)
        {
            timeDistanceToStationDidNotDecrease += Time.deltaTime;
        } else
        {
            timeDistanceToStationDidNotDecrease = 0;
        }
         if (distanceToHuman > 2.0f)
        {
            Fire(Trigger.GoToHuman);
        }
        if (leaderController.pursuitBehavior.HasDroneReachedTarget()|| distanceToStation < 1.6f)
        {
            // wait for 2 secs
            if (!arrivedAtStationCorner2)
            {startTime = Time.time;
            arrivedAtStationCorner2 = true;
            }
            
            if ((Time.time - startTime > 2.0f) && arrivedAtStationCorner2)
            {
            Debug.Log("Arrived at station corner 2");  
            Fire(Trigger.GoToStation2);
            }
            
          
        }
    }

    private void HandleGoingToStation2(StateMachine<State, Trigger>.Transition t)
    {
        Debug.Log("Going to station 2");
        leaderController.isUpperFencingActive = false;

        leaderController.targetPosition = GameObject.Find("Station2").transform.position;
        leaderController.targetPosition.y = 1.0f;
        Debug.Log("leaderController.targetPosition" + leaderController.targetPosition);
        leaderController.pursuitBehavior.SetTarget(leaderController.targetPosition);

        leaderController.leadingDroneActive = true;
        leaderController.isPursuitActive = true;
        leaderController.isEncircleHumanActive = false;
        leaderController.isAlignmentActive = false;
        leaderController.isCohesionActive = false;
        leaderController.isWanderingActive = false;
    }

    private void MonitorGoingToStation2()
    {
        goingToStation2 = true;
        goingToStation = false;
        goingToStationTemp = false;
        goingToStation3 = false;
        goingToStation4 = false;
        goingToStationCorner1 = false;
        goingToStationCorner2 = false;


        //GuideHumanOrder.load.transform.position = leaderController.droneController.transform.position;
        var distanceDronetoStation = Vector3.Distance(GameObject.Find("Drone").transform.position, GameObject.Find("Station2").transform.position);
        var distanceToStation = leaderController.pursuitBehavior.GetDistanceFromLeaderToTarget();
        var distanceChange = Math.Abs(lastDistanceToStation - distanceToStation);
        var distanceToHuman = Vector3.Distance(GameObject.Find("Drone").transform.position, GameObject.Find("Human").transform.position);
        //Debug.Log("Distance to human: " + distanceToHuman);
        if(distanceChange < 0.5f)
        {
            timeDistanceToStationDidNotDecrease += Time.deltaTime;
        } else
        {
            timeDistanceToStationDidNotDecrease = 0;
        }
         if (distanceToHuman > globalDistanceBetweenDroneAndHuman + 1.5f)
        {
            //Fire(Trigger.GoToHuman);
            Fire(Trigger.WaitForHuman);
        }

        if (distanceDronetoStation < 2.5f)
        {
            // wait for 2 secs
            if (!arrivedAtStation2)
            {startTime = Time.time;
            arrivedAtStation2 = true;
            }
            if(round1)
            {
                if ((Time.time - startTime > 2.0f) && arrivedAtStation2)
                {
                Debug.Log("Arrived at station 2");
                Fire(Trigger.GoWait2);
                }
            }
            else if(round2)
            {
                if ((Time.time - startTime > 2.0f) && arrivedAtStation2)
                {
                Debug.Log("Arrived at station 2 round2");
                Fire(Trigger.GoToStationTemp2);
                }
            }
        }
    }

   private void HandleGoingToStationTemp2(StateMachine<State, Trigger>.Transition t)
    {
        Debug.Log("Going to station temp 2");
        leaderController.isUpperFencingActive = false;

        leaderController.targetPosition = GameObject.Find("StationTemp2").transform.position;
        leaderController.targetPosition.y = 1.0f;
        Debug.Log("leaderController.targetPosition" + leaderController.targetPosition);
        leaderController.pursuitBehavior.SetTarget(leaderController.targetPosition);

        leaderController.leadingDroneActive = true;
        leaderController.isPursuitActive = true;
        leaderController.isEncircleHumanActive = false;
        leaderController.isAlignmentActive = false;
        leaderController.isCohesionActive = false;
        leaderController.isWanderingActive = false;
    }


    private void MonitorGoingToStationTemp2()
    {
        
        goingToStationTemp2 = true;
        goingToStationTemp = false;
        goingToStation = false;
        goingToStation2 = false;
        goingToStation3 = false;
        goingToStation4 = false;
        //GuideHumanOrder.load.transform.position = leaderController.droneController.transform.position;
        var distanceDroneToStation = Vector3.Distance(GameObject.Find("Drone").transform.position, GameObject.Find("StationTemp2").transform.position);
        var distanceToStation = leaderController.pursuitBehavior.GetDistanceFromLeaderToTarget();
        var distanceChange = Math.Abs(lastDistanceToStation - distanceToStation);
        var distanceToHuman = Vector3.Distance(GameObject.Find("Drone").transform.position, GameObject.Find("Human").transform.position);
        //Debug.Log("Distance to human: " + distanceToHuman);
        if(distanceChange < 0.5f)
        {
            timeDistanceToStationDidNotDecrease += Time.deltaTime;
        } else
        {
            timeDistanceToStationDidNotDecrease = 0;
        }
         if (distanceToHuman > globalDistanceBetweenDroneAndHuman + 1.5f)
        {
            //Fire(Trigger.GoToHuman);
            Fire(Trigger.WaitForHuman);
        }

        if (distanceDroneToStation < 2.5f)
        {
            // wait for 2 secs
            if (!arrivedAtStationTemp2)
            {startTime = Time.time;
            arrivedAtStationTemp2 = true;
            }
            
            if ((Time.time - startTime > 2.0f) && arrivedAtStationTemp2 && !flagFinal)
            {
                round1 = false;
                round2 = true;    
                Debug.Log("Arrived at station temp 2");
                Fire(Trigger.GoToStation);
            }
            
            if ((Time.time - startTime > 2.0f) && arrivedAtStationTemp2 && flagFinal)
            {   
                //flagFinal = false;
                Debug.Log("Arrived at station temp 2 round2");
                //Fire(Trigger.GoToStation4);
                Fire(Trigger.GoWait4);
            }
          
        }
    }

    private void HandleGoingToStation3(StateMachine<State, Trigger>.Transition t)
    {
        flagFinal = true;
        Debug.Log("Going to station 3");
        leaderController.isUpperFencingActive = false;

        leaderController.targetPosition = GameObject.Find("Station3").transform.position;
        leaderController.targetPosition.y = 1.0f;
        Debug.Log("leaderController.targetPosition" + leaderController.targetPosition);
        leaderController.pursuitBehavior.SetTarget(leaderController.targetPosition);

        leaderController.leadingDroneActive = true;
        leaderController.isPursuitActive = true;
        leaderController.isEncircleHumanActive = false;
        leaderController.isAlignmentActive = false;
        leaderController.isCohesionActive = false;
        leaderController.isWanderingActive = false;
    }

    private void MonitorGoingToStation3()
    {
        goingToStation3 = true;
        goingToStation = false;
        goingToStationTemp = false;
        goingToStationTemp2 = false;
        goingToStation2 = false;
        //GuideHumanOrder.load.transform.position = leaderController.droneController.transform.position;
        var distanceDroneToStation = Vector3.Distance(GameObject.Find("Drone").transform.position, GameObject.Find("Station3").transform.position);
        var distanceToStation = leaderController.pursuitBehavior.GetDistanceFromLeaderToTarget();
        var distanceChange = Math.Abs(lastDistanceToStation - distanceToStation);
        var distanceToHuman = Vector3.Distance(GameObject.Find("Drone").transform.position, GameObject.Find("Human").transform.position);
        //Debug.Log("Distance to human: " + distanceToHuman);
        if(distanceChange < 0.5f)
        {
            timeDistanceToStationDidNotDecrease += Time.deltaTime;
        } else
        {
            timeDistanceToStationDidNotDecrease = 0;
        }
         if (distanceToHuman > globalDistanceBetweenDroneAndHuman + 1.5f)
        {
            Fire(Trigger.GoToHuman);
        }
        if (distanceDroneToStation < 2.5f)
        {
            // wait for 2 secs
            if (!arrivedAtStation3)
            {startTime = Time.time;
            arrivedAtStation3 = true;
            }
            
            if ((Time.time - startTime > 2.0f) && arrivedAtStation3)
            {
                Debug.Log("Arrived at station 3");
                Fire(Trigger.GoWait3);
            }
            
        }
    }

    private void HandleWaiting3()
    {   
        leaderController.isUpperFencingActive = true;
        leaderController.targetPosition = GameObject.Find("Station3").transform.position;
        leaderController.targetPosition.x = leaderController.targetPosition.x - 7.5f;
        leaderController.targetPosition.z = leaderController.targetPosition.z -2.0f;
        leaderController.pursuitBehavior.SetTarget(leaderController.targetPosition);
       
        Debug.Log("Waiting3");
        
    }

    private void MonitorWaiting3()
    {
        // Wait for 5 secs
        if (!waiting)
        {   
            startTime = Time.time;
            //Debug.Log("start time" + startTime);
            waiting = true;
        }
       
        var distanceToHuman = Vector3.Distance(GameObject.Find("Drone").transform.position, GameObject.Find("Human").transform.position);
        if ((Time.time - startTime > 20.0f) && (distanceToHuman < globalDistanceBetweenDroneAndHuman + 1.5f))
        {
            //Debug.Log("Time" + Time.time);
            //Fire(Trigger.GoToStationTemp);
            Fire(Trigger.GoToStation2);
        }

    }

    public void HandleWaiting4()
    {   
        leaderController.isUpperFencingActive = true;
        leaderController.targetPosition = GameObject.Find("StationTemp2").transform.position;
        leaderController.targetPosition.x = leaderController.targetPosition.x + 2.0f;
        leaderController.targetPosition.z = leaderController.targetPosition.z + 2.0f;
        leaderController.pursuitBehavior.SetTarget(leaderController.targetPosition);
       
        Debug.Log("Waiting4");
        
    }

    public void MonitorWaiting4()
    {
        // Wait for 5 secs
        if (!waiting4)
        {   
            startTime = Time.time;
            //Debug.Log("start time" + startTime);
            waiting4 = true;
        }
       
        var distanceToHuman = Vector3.Distance(GameObject.Find("Drone").transform.position, GameObject.Find("Human").transform.position);
        if ((Time.time - startTime > 12.0f) && (distanceToHuman < globalDistanceBetweenDroneAndHuman + 1.5f))
        {
            //Debug.Log("Time" + Time.time);
            //Fire(Trigger.GoToStationTemp);
            Fire(Trigger.GoToStation4);
        }
    }

    private void HandleGoingToStation4(StateMachine<State, Trigger>.Transition t)
    {
        Debug.Log("Going to station 4");
        leaderController.isUpperFencingActive = false;

        leaderController.targetPosition = GameObject.Find("Station4").transform.position;
        leaderController.targetPosition.y = 1.0f;
        Debug.Log("leaderController.targetPosition" + leaderController.targetPosition);
        leaderController.pursuitBehavior.SetTarget(leaderController.targetPosition);

        leaderController.leadingDroneActive = true;
        leaderController.isPursuitActive = true;
        leaderController.isEncircleHumanActive = false;
        leaderController.isAlignmentActive = false;
        leaderController.isCohesionActive = false;
        leaderController.isWanderingActive = false;
    }

    private void MonitorGoingToStation4()
    {
        goingToStation4 = true;
        goingToStation = false;
        goingToStationTemp = false;
        goingToStationTemp2 = false;
        goingToStation2 = false;
        goingToStation3 = false;
        //GuideHumanOrder.load.transform.position = leaderController.droneController.transform.position;
        var distanceDroneToStation = Vector3.Distance(GameObject.Find("Drone").transform.position, GameObject.Find("Station4").transform.position);
        var distanceToStation = leaderController.pursuitBehavior.GetDistanceFromLeaderToTarget();
        var distanceChange = Math.Abs(lastDistanceToStation - distanceToStation);
        var distanceToHuman = Vector3.Distance(GameObject.Find("Drone").transform.position, GameObject.Find("Human").transform.position);
        //Debug.Log("Distance to human: " + distanceToHuman);
        if(distanceChange < 0.5f)
        {
            timeDistanceToStationDidNotDecrease += Time.deltaTime;
        } else
        {
            timeDistanceToStationDidNotDecrease = 0;
        }
         if (distanceToHuman > globalDistanceBetweenDroneAndHuman + 1.5f)
        {
            //Fire(Trigger.GoToHuman);
            Fire(Trigger.WaitForHuman);
        }
        if (leaderController.pursuitBehavior.HasDroneReachedTarget()|| distanceToStation < 1.6f)
        {
            // wait for 2 secs
            if (!arrivedAtStation4)
            {startTime = Time.time;
            arrivedAtStation4 = true;
            }
            
            if ((Time.time - startTime > 2.0f) && arrivedAtStation4)
            {
                Debug.Log("Arrived at station 4");
                Fire(Trigger.GoHome);
            }
            
        }
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
