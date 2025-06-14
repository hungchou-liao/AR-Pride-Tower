using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;

// This script should be attached to your AR Session Origin or a similar manager GameObject.
public class ObjectGrabber : MonoBehaviour // Class name changed back to ObjectGrabber
{
    [SerializeField] string targetTag = "PlacedObject"; // Tag for grabbable objects
    [SerializeField] PlaceObjectOnPlane placeObjectScript; // Reference to your PlaceObjectOnPlane script

    private GameObject heldObject;
    private Transform arCameraTransform;
    private Vector3 offsetFromCameraPosition;
    private Quaternion offsetFromCameraRotation;
    private Vector3 originalScale;

    private ARRaycastManager raycastManager;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Start()
    {
        arCameraTransform = Camera.main.transform;
        raycastManager = GetComponent<ARRaycastManager>();

        if (raycastManager == null)
        {
            Debug.LogError("ObjectGrabber: ARRaycastManager not found on this GameObject.");
            enabled = false;
            return;
        }

        // Find PlaceObjectOnPlane if not assigned in Inspector
        if (placeObjectScript == null)
        {
            placeObjectScript = FindObjectOfType<PlaceObjectOnPlane>();
            if (placeObjectScript == null)
            {
                Debug.LogWarning("ObjectGrabber: PlaceObjectOnPlane script not found in scene. Object placement control might be limited.");
            }
        }
    }

    void Update()
    {
        // Continuously update position and rotation of held object relative to camera
        if (heldObject != null)
        {
            heldObject.transform.position = arCameraTransform.position + arCameraTransform.TransformVector(offsetFromCameraPosition);
            heldObject.transform.rotation = arCameraTransform.rotation * offsetFromCameraRotation;
        }
    }

    // Public property to check if an object is currently held
    public bool IsHolding => heldObject != null;

    public void OnGrabButtonPressed()
    {
        if (heldObject != null)
        {
            Debug.Log("ObjectGrabber: Already holding an object. Cannot grab.");
            return; // Already holding an object
        }

        // Check if an object is currently detected and highlighted (green)
        if (ObjectDetector.closestObject != null && ObjectDetector.IsObjectDetected(ObjectDetector.closestObject))
        {
            GameObject objectToGrab = ObjectDetector.closestObject;

            // Ensure it has the target tag (though ObjectDetector typically filters this)
            if (!objectToGrab.CompareTag(targetTag))
            {
                Debug.LogWarning($"ObjectGrabber: Detected object {objectToGrab.name} does not have the required tag '{targetTag}'.");
                return;
            }

            heldObject = objectToGrab;

            // Store original scale to prevent shrinking
            originalScale = heldObject.transform.localScale;

            // Disable physics on the grabbed object
            Rigidbody rb = heldObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                rb.isKinematic = true;
            }

            // Parent to the camera to keep it fixed relative to the screen
            heldObject.transform.SetParent(arCameraTransform);

            // Calculate and store the offset from the camera's local space
            offsetFromCameraPosition = arCameraTransform.InverseTransformVector(heldObject.transform.position - arCameraTransform.position);
            offsetFromCameraRotation = Quaternion.Inverse(arCameraTransform.rotation) * heldObject.transform.rotation;

            // Explicitly apply the original scale after parenting
            heldObject.transform.localScale = originalScale;

            // Disable placement while holding an object
            if (placeObjectScript != null)
            {
                placeObjectScript.allowPlacement = false;
            }

            Debug.Log($"ObjectGrabber: Grabbed object: {heldObject.name}");
        }
        else
        {
            Debug.Log("ObjectGrabber: No detected (green) object to grab.");
        }
    }

    public void OnDropButtonPressed()
    {
        if (heldObject == null)
        {
            Debug.Log("ObjectGrabber: Not holding an object to drop.");
            return; // Not holding an object
        }

        // Unparent the object
        heldObject.transform.SetParent(null);

        // Re-enable physics
        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
            // You might want to add a small upward force or clear velocity here if it sinks slightly
            // rb.velocity = Vector3.zero;
            // rb.angularVelocity = Vector3.zero;
        }

        // Re-enable placement after dropping
        if (placeObjectScript != null)
        {
            placeObjectScript.allowPlacement = true;
        }

        Debug.Log($"ObjectGrabber: Dropped object: {heldObject.name}");
        heldObject = null;
    }
}