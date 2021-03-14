using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Route : MonoBehaviour
{
    [SerializeField] public string routeName;
    [SerializeField] public RouteStatus routeStatus;
    [SerializeField] public RouteType routeType;
    [SerializeField] public float routeLength = 1; //Temporarily set to 1 for now

    public enum RouteType
    {
        LinearTrack,
        LoopedTrack
    }

    public enum RouteStatus
    {
        Available,
        InitiatedRace, //States that it is available, if one is looking to join an initiated race on this track
        Occupied, //If it was chosen when creating a new race (Prevent others from trying to setup any race on it too)
        Busy
    }
}
