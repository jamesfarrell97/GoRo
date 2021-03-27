using UnityEngine;

public class Oar : MonoBehaviour
{
    [SerializeField] Transform trackedObject;
    [SerializeField] Transform pivotPoint;

    public bool rowing;

    private Vector3 offset;
    private bool playCollisionSound;

    private void Start()
    {
        offset = pivotPoint.position - trackedObject.position;
    }

    private void FixedUpdate()
    {
        transform.LookAt(trackedObject);
        
        transform.Rotate(new Vector3(0, 0, trackedObject.eulerAngles.x));
        transform.position = pivotPoint.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!rowing) return;

        if (other.tag.Equals("Water") && playCollisionSound)
        {
            FindObjectOfType<AudioManager>().Play("Rowing" + Random.Range(1, 5));
            playCollisionSound = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("Water"))
        {
            playCollisionSound = true;
        }
    }
}