using UnityEngine;

public class Oar : MonoBehaviour
{
    [SerializeField] private Transform trackedObject;
    [SerializeField] private Transform pivotPoint;

    private Vector3 offset;
    
    private PlayerController player;

    private void Start()
    {
        offset = pivotPoint.position - trackedObject.position;
        player = gameObject.GetComponentInParent<PlayerController>();
    }

    private void FixedUpdate()
    {
        transform.LookAt(trackedObject);
        
        transform.Rotate(new Vector3(0, 0, trackedObject.eulerAngles.x));
        transform.position = pivotPoint.position;

        PlayAudio();
    }

    bool play = true;

    private void PlayAudio()
    {

#if UNITY_EDITOR

        if (player == null) return;

        switch (player.GetStrokeState())

#else

        switch (StatsManager.Instance.GetStrokeState())

#endif
        {
            case (int) PlayerController.StrokeState.Driving:

                if (play) FindObjectOfType<AudioManager>().Play("Rowing" + Random.Range(1, 3));

                play = false;
                break;

            case (int) PlayerController.StrokeState.Recovery:

                play = true;
                break;

            case (int)PlayerController.StrokeState.WaitingForWheelToAccelerate:

                play = true;
                break;
        }
    }
}