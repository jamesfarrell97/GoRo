using UnityEngine;
using TMPro;

public class EventNotification : MonoBehaviour
{
    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text routeText;
    [SerializeField] TMP_Text distanceText;
    [SerializeField] TMP_Text participantText;


    public enum EventCategory
    {
        Race,
        Trial
    }

    private EventCategory eventCategory;

    public enum NotificationState
    {
        Active,
        Hidden
    }

    private NotificationState state;

    public void Setup(EventCategory category, string title, string route, string distance, string participants)
    {
        eventCategory = category;

        titleText.text = title;
        routeText.text = route;
        distanceText.text = distance;
        participantText.text = participants;

        state = NotificationState.Active;
    }

    public void JoinEvent()
    {
        switch(eventCategory)
        {
            case EventCategory.Race:
                GameManager.Instance.StartRace(routeText.text);
                break;

            case EventCategory.Trial:
                GameManager.Instance.StartTrial(routeText.text);
                break;
        }

        Reset();
    }

    public void DeclineEvent()
    {
        Reset();
    }

    public void ClosePanel()
    {
        Reset();
    }

    public void Reset()
    {
        state = NotificationState.Hidden;
        GameManager.Instance.HideEventNotificationPanel();
    }
}
