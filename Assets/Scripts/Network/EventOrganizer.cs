using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

namespace UnityStandardAssets.Utility
{
    public class EventOrganizer : MonoBehaviour
    {
        public static EventOrganizer Instance;
        [HideInInspector] public PlayerController eventInitiator;
        [HideInInspector] public EventType eventType = EventType.None;
        [HideInInspector] private Route route;

        [SerializeField] public TMP_Text raceRouteNameText;
        [SerializeField] public TMP_Text trialRouteNameText;
        [SerializeField] public TMP_Text raceRouteLengthText;
        [SerializeField] public TMP_Text trialRouteLengthText;

        [SerializeField] public Dropdown routeTypeFilterDD;
        [SerializeField] public Dropdown routeLengthFilterDD;
        [SerializeField] public Dropdown raceLapAmountDD;
        [SerializeField] public Dropdown trialLapAmountDD;
        [SerializeField] public Dropdown raceParticipantsAmountDD;
        [SerializeField] public Dropdown waitTimeDD;
        [SerializeField] public Slider raceWaitingTimeSlider;

        private int lapAmount;
        private int participantsAmount;
        private float waitingTime;

        public enum EventType
        {
            None,
            Race,
            Trial,
            JoinARace
        }

        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            Instance = this;

            DisposeResources();
        }

        public void OpenSetupMenu()
        {
            if (eventType == EventType.Race)
            {
                OpenRaceSetup();
            }
            else if (eventType == EventType.Trial)
            {
                OpenTrialSetup();
            }
            else if (eventType == EventType.JoinARace)
            {
                JoinInitiatedRace();
            }

            ResetRouteSelection();
        }

        public void SetRoute(Route chosenRoute)
        {
            route = chosenRoute;
            route.routeStatus = Route.RouteStatus.Occupied;
        }

        //Release the temporary "hold" on the route, executed when user backs out of event setup on the route
        public void ReleaseRoute()
        {
            route.routeStatus = Route.RouteStatus.Available;
        }

        //Reset all variables, ready for next event organization session
        public void DisposeResources()
        {
            eventInitiator = null;
            eventType = EventType.None;
            route = null;

            lapAmount = 1;
            participantsAmount = 1;
            waitingTime = 1;
        }

        public void UpdateLapAmount(Dropdown lapAmountDD)
        {
            int index = lapAmountDD.value;
            lapAmount = int.Parse(lapAmountDD.options[index].text);

            float distance = route.routeLength * float.Parse($"{lapAmount}");
            UpdateDistanceText(distance);
        }

        public void UpdateParticipantAmount(Dropdown participantsAmountDD)
        {
            int index = participantsAmountDD.value;
            participantsAmount = int.Parse(participantsAmountDD.options[index].text);
        }

        public void UpdateWaitingTime(Dropdown waitTimeDD)
        {
            int index = waitTimeDD.value;
            waitingTime = float.Parse(waitTimeDD.options[index].text);
        }

        private void ExtractAllValues(bool valuesForRace)
        {
            if (valuesForRace == true)
            {
                UpdateLapAmount(raceLapAmountDD);
                UpdateParticipantAmount(raceParticipantsAmountDD);
                UpdateWaitingTime(waitTimeDD);
            }
            else
            {
                UpdateLapAmount(trialLapAmountDD);
            }
        }

        private void JoinInitiatedRace()
        {
            RaceManager.Instance.AddPlayerToRace(eventInitiator, route);
        }

        //Open Race Setup Menu
        //Set the Relevant Route Details within this Menu
        private void OpenRaceSetup()
        {
            MenuManager.Instance.OpenMenu("Race Setup");

            raceRouteNameText.text = route.name;
            raceRouteLengthText.text = $"{route.routeLength} metres";
        }

        //Open Trial Setup Menu
        //Set the Relevant Route Details within this Menu
        private void OpenTrialSetup()
        {
            MenuManager.Instance.OpenMenu("Trial Setup");
            trialRouteNameText.text = route.name;
            trialRouteLengthText.text = $"{route.routeLength} metres";
        }

        private void UpdateDistanceText(float distance)
        {
            string distanceText = $"{distance} metres";

            if (eventType == EventType.Race)
            {
                raceRouteLengthText.text = distanceText;
            }
            else
            {
                trialRouteLengthText.text = distanceText;
            }
        }

        //Create organized race and add creator to the race
        public void StartRace()
        {
            ExtractAllValues(true);
            RaceManager.Instance.CreateRace(eventInitiator, route.gameObject.name, waitingTime, lapAmount, participantsAmount);
            MenuManager.Instance.OpenMenu("HUD");

            //DisposeResources();
            ResetRaceSetup();
        }

        public void StartTrial()
        {
            ExtractAllValues(false);
            TrialManager.Instance.CreateTrialAndAddPlayer(eventInitiator, route, lapAmount);
            MenuManager.Instance.OpenMenu("HUD");

            //DisposeResources();
            ResetTrialSetup();
        }

        //Set all values within the setup menu back to default after this window is closed
        public void ResetRaceSetup()
        {
            //Set index back to 0
            raceLapAmountDD.value = 0;
            raceParticipantsAmountDD.value = 0;
            waitTimeDD.value = 0;
        }

        //Set all values within the setup menu back to default after this window is closed
        public void ResetTrialSetup()
        {
            trialLapAmountDD.value = 0; //Set index back to 0
        }

        //Set filtering options back to default
        public void ResetRouteSelection()
        {
            routeTypeFilterDD.value = 0;
            routeLengthFilterDD.value = 0;
        }


    }
}