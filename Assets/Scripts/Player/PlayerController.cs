using UnityEngine;
using Photon.Pun;

// Code referenced: https://www.youtube.com/watch?v=7bevpWbHKe4&t=315s
//
//
//
public class PlayerController : MonoBehaviour
{
    [SerializeField] public float boatSpeed = 1f;
    [SerializeField] [Range(0, 1f)] float speedIncreaseFactor = 0.1f;
    [SerializeField] [Range(0, 1f)] float speedDecayFactor = 0.1f;

    [SerializeField] private Animator[] rowingAnimators;
    [SerializeField] private Material otherPlayerMaterial;

    [HideInInspector] public Transform[] route;

    [HideInInspector] public bool participatingInRace = false;
    [HideInInspector] public bool participatingInTimeTrial = false;

    private AchievementTracker achievementTracker;
    private BoxCollider boxCollider;
    private PhotonView photonView;
    private StatsManager stats;

    private bool allowedMove = true;
    private bool move = false;

    private void Awake()
    {
        stats = GameManager.Instance.GetComponent<StatsManager>();

        achievementTracker = GetComponent<AchievementTracker>();
        boxCollider = GetComponent<BoxCollider>();
        photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            AssignMenuCamera();
        }
        else
        {
            DestroyCameras();
            DestroyCanvas();

            UpdateAppearance();
        }
    }

    private void AssignMenuCamera()
    {
        // Retrieve player camera
        Camera playerCamera = transform.Find("Cameras/Main Camera").GetComponent<Camera>();

        // Update menu camera
        MenuManager.Instance.GetComponentInParent<Canvas>().worldCamera = playerCamera;

        // Display HUD
        MenuManager.Instance.OpenMenu("HUD");
    }

    private void DestroyCameras()
    {
        Camera[] cameras = GetComponentsInChildren<Camera>();
        foreach (Camera c in cameras)
        {
            c.gameObject.SetActive(false);
        }
    }

    private void DestroyCanvas()
    {
        Canvas[] canvas = GetComponentsInParent<Canvas>();
        foreach (Canvas c in canvas)
        {
            Destroy(c);
        }
    }

    private void UpdateAppearance()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;

            for (int i = 0; i < renderer.materials.Length; i++)
            {
                // Change minimap icon color
                if (renderer.gameObject.name == "Boat Front" || renderer.gameObject.name == "Boat Rear")
                {
                    materials[i].color = Color.red;
                }

                // Destroy water mask
                else if (renderer.gameObject.name.Contains("Mask"))
                {
                    Destroy(renderer.transform.parent.gameObject.GetComponent<SetRenderQueue>());
                    Destroy(renderer.gameObject);
                }

                // Update material
                else if (!materials[i].color.Equals(null))
                {
                    materials[i] = otherPlayerMaterial;
                }
            }

            renderer.materials = materials;
        }
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        achievementTracker.TrackAchievements(photonView);

        if (!allowedMove)
        {
            velocity = 0;
            return;
        }

        //if (Input.GetKey(KeyCode.W))
        //{
        //    moveForward = true;
        //}
        //else
        //{
        //    moveForward = false;
        //}

        UpdateSpeed();
        CalculateVelocity();
        Animate();
    }

    private float currSpeed = 0;
    private float prevSpeed = 0;
    private float deltSpeed = 0;

    private float velocity = 0;
    private bool change;

    private float currTime = 0;
    private void UpdateSpeed()
    {
        if (currTime < 1f)
        {
            currTime += Time.fixedDeltaTime;
            return;
        }

        currTime = 0;
        //#if UNITY_EDITOR
        //        if (moveForward)
        //        {
        //            currDist += 3 * Time.fixedDeltaTime; //     3 meters
        //            currTime += 1 * Time.fixedDeltaTime; // per 1 second
        //        }
        //#else
        //        currDist = stats.GetMetersRowed();
        //        currTime = stats.GetSecondsRowing();
        //#endif

        //        deltDist = currDist - prevDist;
        //        deltTime = currTime - prevTime;

        //        if (deltTime <= 0) deltTime = Time.fixedDeltaTime;

        //        speed = ((deltDist) / (deltTime) * Time.fixedDeltaTime);

        //        prevDist = currDist;
        //        prevTime = currTime;

        //currDist += 2 * Time.fixedDeltaTime; //     3 meters
        //currTime += 1 * Time.fixedDeltaTime; // per 1 second

        currSpeed = stats.GetSpeed();

        if (move)
        {
            currSpeed = (Random.Range(1.5f, 3f) / 35);
        }

        deltSpeed = currSpeed - prevSpeed;

        change = (deltSpeed == 0) 
            ? false 
            : true;

        currSpeed = (deltSpeed != 0)
            ? currSpeed
            : 0;

        prevSpeed = currSpeed;
    }

    private void CalculateVelocity()
    {
        //if (velocity < 0)
        //{
        //    velocity = 0;
        //}
        //else if (speed == 0)
        //{
        //    velocity -= speedDecayFactor;
        //}
        //else
        //{
        //    velocity = speed;
        //}

        if (velocity < currSpeed)
        {
            velocity += speedIncreaseFactor * Time.fixedDeltaTime;
        }
        else
        {
            velocity -= speedDecayFactor * Time.fixedDeltaTime;
        }

        if (velocity < 0)
        {
            velocity = 0;
        }
        //// Increase velocity over time if speed applied
        //if (change)
        //{
        //    velocity += speedIncreaseFactor * Time.fixedDeltaTime;
        //}
        //else
        //{
        //    velocity -= speedDecayFactor * Time.fixedDeltaTime;
        //}

        //// Min speed
        //if (velocity < 0)
        //{
        //    velocity = 0;
        //}

        //// Max speed
        //else if (currSpeed != 0 && velocity > currSpeed)
        //{
        //    velocity = currSpeed;
        //}

        // speed is measured in meters per second and this function is ran 
        // fixedDeltaTime's per second - so, we can figure out how far to 
        // move this update, if we evenly spread the speed out across the 
        // second by multiplying it by fixedDeltaTime

        //velocity = speed * Time.deltaTime;
    }

    private void Animate()
    {
        foreach (Animator animator in rowingAnimators)
        {
            if (velocity > 0)
            {
                animator.SetBool("Play", true);
            }
            else
            {
                animator.SetBool("Play", false);
            }
        }
    }

    public void Go()
    {
        move = true;
    }

    public void Stop()
    {
        move = false;
    }

    public void PauseMovement()
    {
        this.allowedMove = false;
    }

    public void ResumeMovement()
    {
        this.allowedMove = true;
    }

    public float GetVelocity()
    {
        return velocity;
    }
}
