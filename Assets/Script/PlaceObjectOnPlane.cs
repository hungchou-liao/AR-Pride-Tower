using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;

public class PlaceObjectOnPlane : MonoBehaviour
{
    [SerializeField] GameObject[] prefabs; // Array of prefabs to choose from
    GameObject spawnedObject;
    public bool allowPlacement = true;

    [SerializeField] ObjectGrabber objectGrabber; // Reference to the ObjectGrabber
    [SerializeField] Canvas uiCanvas; // Reference to the UI Canvas
    [SerializeField] AudioClip jumpSound;

    ARRaycastManager raycaster;
    List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private float lastPlaceTime = 0f;
    public float placementCooldown = 1f;
    private bool isInteractingWithUI = false;

    // Define the bottom screen dead zone (1/6 of screen height)
    private float deadZoneHeight;

    // Track the current index in the sequence
    private int currentPrefabIndex = 0;

    private AudioSource audioSource;

    // Static members to track placed objects and notify listeners
    public static int placedObjectCount = 0;
    public static event System.Action OnObjectPlaced;

    private Camera mainCamera;
    public float launchSpeed;

    private void Start()
    {
        mainCamera = Camera.main;
        raycaster = GetComponent<ARRaycastManager>();

        // Reset placed object count at the start of the scene
        placedObjectCount = 0;

        // Find ObjectGrabber if not assigned
        if (objectGrabber == null)
        {
            objectGrabber = FindObjectOfType<ObjectGrabber>();
        }

        // Find UI Canvas if not assigned
        if (uiCanvas == null)
        {
            uiCanvas = FindObjectOfType<Canvas>();
        }

        // Ensure UI is rendered on top
        if (uiCanvas != null)
        {
            uiCanvas.sortingOrder = 100; // Set a high sorting order for UI
        }

        // Calculate the dead zone height (1/6 of screen height)
        deadZoneHeight = Screen.height / 6f;

        // Load all prefabs from the Prefab folder
        LoadPrefabs();

        // Initialize audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("Added AudioSource component to " + gameObject.name);
        }

        // Check audio system
        if (AudioListener.pause)
        {
            Debug.LogWarning("Audio system is paused! Check AudioListener settings.");
        }

        // Check audio source settings
        if (audioSource != null)
        {
            Debug.Log($"AudioSource settings on {gameObject.name}:");
            Debug.Log($"- Mute: {audioSource.mute}");
            Debug.Log($"- Volume: {audioSource.volume}");
            Debug.Log($"- Play On Awake: {audioSource.playOnAwake}");
            Debug.Log($"- Spatial Blend: {audioSource.spatialBlend}");
        }

        if (jumpSound == null)
        {
            Debug.LogError("Place sound effect is not assigned in the inspector!");
        }
        else
        {
            Debug.Log($"Place sound effect is properly assigned: {jumpSound.name}");
            Debug.Log($"- Load Type: {jumpSound.loadType}");
            Debug.Log($"- Load State: {jumpSound.loadState}");
            Debug.Log($"- Length: {jumpSound.length} seconds");
        }


    }

    private void LoadPrefabs()
    {
        // Load all prefabs from the Prefab folder
        prefabs = Resources.LoadAll<GameObject>("Prefabs");

        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogError("No prefabs found in the Prefab folder!");
            return;
        }

        // Sort prefabs in the desired order: P > R > I > D > E > 25 > Heart
        System.Array.Sort(prefabs, (a, b) =>
        {
            string[] order = { "P", "R", "I", "D", "E", "25", "Heart" };
            int indexA = System.Array.IndexOf(order, a.name);
            int indexB = System.Array.IndexOf(order, b.name);
            return indexA.CompareTo(indexB);
        });

        Debug.Log($"Loaded {prefabs.Length} prefabs in order: {string.Join(", ", System.Array.ConvertAll(prefabs, p => p.name))}");
    }

    void Update()
    {
        // Check if touching UI
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            isInteractingWithUI = EventSystem.current.IsPointerOverGameObject(Touchscreen.current.primaryTouch.touchId.ReadValue());
        }
        else
        {
            isInteractingWithUI = false;
        }
    }

    public void OnPlaceObject(InputValue value)
    {
        // Don't place objects if interacting with UI
        if (isInteractingWithUI)
        {
            Debug.Log("UI interaction detected, ignoring placement.");
            return;
        }

        // Don't allow placement if we're holding an object
        if (!allowPlacement || (objectGrabber != null && objectGrabber.IsHolding))
        {
            Debug.Log("Cannot place object while holding one.");
            return;
        }

        if (Time.time - lastPlaceTime < placementCooldown)
        {
            Debug.Log("Placement cooldown active. Please wait.");
            return;
        }

        Vector2 touchPosition = value.Get<Vector2>();

        // Check if touch is in the bottom dead zone
        if (touchPosition.y <= deadZoneHeight)
        {
            Debug.Log("Touch in bottom dead zone, ignoring placement.");
            return;
        }

        Debug.Log("Tapped screen at: " + touchPosition);

        int layerMask = 1 << LayerMask.NameToLayer("PlacedObject");
        Ray ray = mainCamera.ScreenPointToRay(touchPosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))//found placedobject to tap
        {
            //Destroy(hit.collider.gameObject);
            Rigidbody rb = hit.collider.gameObject.GetComponent<Rigidbody>();
            rb.velocity = Vector3.up * launchSpeed;

            // Play sound effect
            if (jumpSound != null && audioSource != null)
            {
                Debug.Log($"Attempting to play sound: {jumpSound.name}");
                Debug.Log($"- AudioSource enabled: {audioSource.enabled}");
                Debug.Log($"- AudioSource playing: {audioSource.isPlaying}");
                Debug.Log($"- AudioListener enabled: {AudioListener.pause == false}");

                audioSource.PlayOneShot(jumpSound);
                Debug.Log("PlayOneShot called");
            }
            else
            {
                Debug.LogWarning("Could not play sound effect - AudioSource or PlaceSound is null");
            }
        }

        else if (raycaster.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            Debug.Log("Plane hit at: " + hitPose.position);

            Vector3 stackPos = StackableObject.GetNextStackPosition(hitPose.position);

            // Place the next object in sequence
            if (prefabs != null && prefabs.Length > 0)
            {
                GameObject selectedPrefab = prefabs[currentPrefabIndex];

                // Get camera position on the same y as the object
                Vector3 cameraPosition = mainCamera.transform.position;
                cameraPosition.y = hitPose.position.y;
                Quaternion lookRotation = Quaternion.LookRotation(cameraPosition - hitPose.position);

                GameObject newObject = Instantiate(selectedPrefab, stackPos, lookRotation);
                lastPlaceTime = Time.time;
                Debug.Log($"Placed prefab: {selectedPrefab.name}");

                // Increment placed object count and invoke event
                placedObjectCount++;
                OnObjectPlaced?.Invoke();


                // Move to next prefab in sequence
                currentPrefabIndex = (currentPrefabIndex + 1) % prefabs.Length;
            }
            else
            {
                Debug.LogError("No prefabs available to place!");
            }
        }
        else
        {
            Debug.Log("No plane hit.");
        }
    }
}