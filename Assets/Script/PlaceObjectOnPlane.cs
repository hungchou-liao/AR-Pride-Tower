using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;

public class PlaceObjectOnPlane : MonoBehaviour
{
    [SerializeField] GameObject placedPrefab;
    GameObject spawnedObject;
    public bool allowPlacement = true;

    [SerializeField] ObjectGrabber objectGrabber; // Reference to the ObjectGrabber
    [SerializeField] Canvas uiCanvas; // Reference to the UI Canvas

    ARRaycastManager raycaster;
    List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private float lastPlaceTime = 0f;
    public float placementCooldown = 1f;
    private bool isInteractingWithUI = false;

    // Define the bottom screen dead zone (1/6 of screen height)
    private float deadZoneHeight;

    private void Start()
    {
        raycaster = GetComponent<ARRaycastManager>();

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
        if (!allowPlacement || (objectGrabber != null && objectGrabber.IsHolding()))
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

        if (raycaster.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            Debug.Log("Plane hit at: " + hitPose.position);

            Vector3 stackPos = StackableObject.GetNextStackPosition(hitPose.position);
            GameObject newObject = Instantiate(placedPrefab, stackPos, hitPose.rotation);
            lastPlaceTime = Time.time;
        }
        else
        {
            Debug.Log("No plane hit.");
        }
    }
}