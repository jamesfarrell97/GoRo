﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class RaceManager : MonoBehaviour
{

    public void TakePartInARace(Boat player)
    {
        //Will initiate Menu Screen to appear where player will be prompted to select race track they wish to take part in
        //From this interaction the following methods starting with "AddPlayerToXXX" represent the button pressed actions relevant to race selected
    }

    #region Race Initiation Button Responses
    public void AddPlayerToHeroBeachRace()
    {
        Race heroBeachRace = GameObject.Find("HeroBeachRace").GetComponent<Race>();
        if(heroBeachRace.raceInitiated == false)
        {
            //BringUpRaceSetupMenu
            //SetAllRaceParametersBased on the feedback given by player
            heroBeachRace.raceInitiated = true;
            //heroBeachRace.numberOfLaps = ?;
            //heroBeachRace.raceCapacity = ?;
            //Add player to participants list
        }
        else
        {
            if(heroBeachRace.participants.Count < heroBeachRace.raceCapacity)
            {
                //Add player to the partcipants list
            }
        }
    }
    #endregion Race Initiation Button Responses

    //HardCoded Method to reach simple solution for Base Version of the 
    //waypoint/ race/ time trial mechanics until the menu and more race routes are implemented 
    public void AddPlayerToRace(Boat player)
    {
        Race heroBeachRace = GameObject.Find("HeroBeachRace").GetComponent<Race>();
        heroBeachRace.raceInitiated = true;
        heroBeachRace.timeRaceInitiated = Time.timeSinceLevelLoad;
        heroBeachRace.numberOfLaps = 1;
        heroBeachRace.raceCapacity = 1;
        player.GetComponent<Boat>().GetComponent<DictatePlayerMovement>().currentRace = heroBeachRace;
        heroBeachRace.AddParticipantIntoRace(player);
    }
    

}
