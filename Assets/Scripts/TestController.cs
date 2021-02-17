using Photon.Pun;
using UnityEngine;

public class TestController : MonoBehaviour
{
    [SerializeField] [Range(1, 5)] float speed = 1f;

    private PhotonView photonView;
    private Rigidbody rigidbody;
    private Camera camera;

    private bool paused;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        photonView = GetComponent<PhotonView>();
        camera = GetComponentInChildren<Camera>();

        if (!photonView.IsMine)
        {
            camera.gameObject.SetActive(false);
            Destroy(rigidbody);
        }
        else
        {
            AssignMenuCamera();
        }
    }

    private void AssignMenuCamera()
    {
        // Retrieve player camera
        Camera playerCamera = transform.Find("Capsule/Main Camera").GetComponent<Camera>();

        // Update menu camera
        MenuManager.Instance.GetComponentInParent<Canvas>().worldCamera = playerCamera;

        // Display HUD
        MenuManager.Instance.OpenMenu("HUD");
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        if (paused) return;

        // Movement
        if (Input.GetKey(KeyCode.W))
        {
            rigidbody.AddForce(transform.forward * speed);
        }
        else if (Input.GetKey(KeyCode.A))
        {
            rigidbody.AddForce(-transform.right * speed);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            rigidbody.AddForce(transform.right * speed);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            rigidbody.AddForce(-transform.forward * speed);
        }

        // Rotation
        if (Input.GetKey(KeyCode.Q))
        {
            rigidbody.AddTorque(-transform.up * speed);
        }
        else if (Input.GetKey(KeyCode.E))
        {
            rigidbody.AddTorque(transform.up * speed);
        }
    }

    public void Pause()
    {
        paused = true;
    }

    public void Resume()
    {
        paused = false;
    }
}
