using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

public class CrazyflieConnection : MonoBehaviour, IDroneConnection
{

    public GameObject MQTTManagerGO;
    public MQTTManager manager;
    public GameObject floor;

    CultureInfo CultureUS = new CultureInfo("en-US");

    // Start is called before the first frame update
    void Start()
    {
        MQTTManagerGO = GameObject.Find("MQTTManager");
        manager = MQTTManagerGO.GetComponent<MQTTManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject GetFloor()
    {
        return floor;
    }

    public void Start(int id, double starttime, double duration, double height)
    {
        string topic = "crazyflie/start";

        // {"id": 1, "data": [0.000, 1.000, 0.500]}
        //               starttime, duration, height
        string json = String.Format(CultureUS, "{{\"id\": {0}, \"data\": [{1:f3}, {2:f3}, {3:f3}]}}", id, starttime, duration, height);

        //Console.WriteLine("Sending Start Command to channel {0}:\n{1}", topic, json);

        manager.Publish(topic, json);

        //if (!SimulationMode) client.Publish(topic, Encoding.UTF8.GetBytes(json));
    }

    public void Land(int id, double starttime, double duration)
    {
        string topic = "crazyflie/land";

        //{"id": 1, "data": [0.000, 1.000]}
        //               starttime, duration
        string json = String.Format(CultureUS, "{{\"id\": {0}, \"data\": [{1:f3}, {2:f3}]}}", id, starttime, duration);

        //Console.WriteLine("Sending Land Command to channel {0}:\n{1}", topic, json);

        manager.Publish(topic, json);

        //if (!SimulationMode) client.Publish(topic, Encoding.UTF8.GetBytes(json));
    }

    public void LandWithHeight(int id, double starttime, double duration, double height)
    {
        string topic = "crazyflie/landwithheight";

        //{"id": 1, "data": [0.000, 1.000, 0.500]}
        //               starttime, duration, height
        string json = String.Format(CultureUS, "{{\"id\": {0}, \"data\": [{1:f3}, {2:f3}, {3:f3}]}}", id, starttime, duration, height);
        
        //Console.WriteLine("Sending Land Command to channel {0}:\n{1}", topic, json);

        manager.Publish(topic, json);

        //if (!SimulationMode) client.Publish(topic, Encoding.UTF8.GetBytes(json));
    }

    public void MoveRelative(int id, double starttime, double duration, double x, double y, double z, double yaw)
    {
        string topic = "crazyflie/move";

        //{"id": 1, "data": [0.000, 3.000, [0.500, 0.100, 0.000], 0.000]}
        //               starttime, duration,  x,    y,    z,       yaw

        string json = String.Format(CultureUS, "{{\"id\": {0}, \"data\": [{1:f3}, {2:f3}, [{3:f3}, {4:f3}, {5:f3}], {6:f3}]}}"
            , id, starttime, duration, x, y, z, yaw);

        //Console.WriteLine("Sending Move(Relative) Command to channel {0}:\n{1}", topic, json);

        manager.Publish(topic, json);

        //if (!SimulationMode) client.Publish(topic, Encoding.UTF8.GetBytes(json));
    }

    public void MoveTo(int id, double starttime, double duration, double x, double y, double z, double yaw)
    {
        string topic = "crazyflie/moveto";

        //{"id": 1, "data": [0.000, 2.000, [3.000, 1.500, 0.600], 0.000]}
        //               starttime, duration,  x,    y,    z,       yaw

        string json = String.Format(CultureUS, "{{\"id\": {0}, \"data\": [{1:f3}, {2:f3}, [{3:f3}, {4:f3}, {5:f3}], {6:f3}]}}"
            , id, starttime, duration, x, y, z, yaw);

        //Console.WriteLine("Sending MoveTo(Absolute) Command to channel {0}:\n{1}", topic, json);

        manager.Publish(topic, json);

        //if (!SimulationMode) client.Publish(topic, Encoding.UTF8.GetBytes(json));
    }

    public void MoveHome(int id, double starttime, double duration)
    {
        string topic = "crazyflie/movehome";

        //{"id": 1, "data": [0.000, 0.500]}
        //               starttime, duration
        string json = String.Format(CultureUS, "{{\"id\": {0}, \"data\": [{1:f3}, {2:f3}]}}", id, starttime, duration);

        //Console.WriteLine("Sending MoveHome Command to channel {0}:\n{1}", topic, json);

        manager.Publish(topic, json);

        //if (!SimulationMode) client.Publish(topic, Encoding.UTF8.GetBytes(json));
    }

    public void CreateTrajectory(int id, double vmax, double amax, string groupMask, List<Vector3> waypoints)
    {
        string topic = "crazyflie/createTrajectory";

        //{"id": 1, "data": [[x, y, z], [...], ...}
        //               starttime, duration

        var list = new List<string>();
        foreach(var v in waypoints)
        {
            list.Add(String.Format(CultureUS, "[{0:f3}, {2:f3}, {1:f3}]", v.x, v.y, v.z));
        }

        string json = String.Format(CultureUS, "{{\"id\": {0}, \"vmax\": {1:f3}, \"amax\": {2:f3}, \"groupmask\": \"{3:f3}\", \"data\": [{4}]}}", id, vmax, amax, groupMask, string.Join(", ", list));

        Debug.Log(json);

        manager.Publish(topic, json);
    }

    public void StartTrajectory(int id, double starttime, double timescale, string groupMask)
    {
        string topic = "crazyflie/startTrajectory";

        //{"id": 1, "data": [0.000, 0.500]}
        //               starttime, duration
        string json = String.Format(CultureUS, "{{\"id\": {0}, \"groupmask\": \"{1}\",  \"timescale\": {2:f3} }}", id, groupMask, timescale);
                    
        manager.Publish(topic, json);
    }


}
