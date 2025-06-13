using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(ARPlaneManager))]
public class PlaneVisibilityManager : MonoBehaviour
{
    private ARPlaneManager planeManager;
    private PlaceObjectOnPlane placeObjectScript;
    private Dictionary<ARPlane, Coroutine> planeTimers = new Dictionary<ARPlane, Coroutine>();
    private float hideDelay = 10f; // Time in seconds before hiding a plane

    void Start()
    {
        planeManager = GetComponent<ARPlaneManager>();
        // Find PlaceObjectOnPlane in the scene instead of on this GameObject
        placeObjectScript = FindObjectOfType<PlaceObjectOnPlane>();

        Debug.Log("PlaneVisibilityManager: Started");
        Debug.Log($"PlaneVisibilityManager: Found planeManager: {planeManager != null}");
        Debug.Log($"PlaneVisibilityManager: Found placeObjectScript: {placeObjectScript != null}");

        // Subscribe to plane events
        planeManager.planesChanged += OnPlanesChanged;

        // Subscribe to object placement event
        if (placeObjectScript != null)
        {
            // We'll track object placement through the PlaceObjectOnPlane script
            StartCoroutine(MonitorObjectPlacement());
        }
    }

    void OnDisable()
    {
        if (planeManager != null)
        {
            planeManager.planesChanged -= OnPlanesChanged;
        }
    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        Debug.Log($"PlaneVisibilityManager: Planes changed - Added: {args.added.Count}, Removed: {args.removed.Count}");

        // Handle newly detected planes
        foreach (ARPlane plane in args.added)
        {
            Debug.Log($"PlaneVisibilityManager: New plane detected - Starting timer");
            // Start timer for the new plane
            StartPlaneTimer(plane);
        }

        // Clean up removed planes
        foreach (ARPlane plane in args.removed)
        {
            if (planeTimers.ContainsKey(plane))
            {
                if (planeTimers[plane] != null)
                {
                    StopCoroutine(planeTimers[plane]);
                }
                planeTimers.Remove(plane);
            }
        }
    }

    private void StartPlaneTimer(ARPlane plane)
    {
        // If there's already a timer running for this plane, stop it
        if (planeTimers.ContainsKey(plane) && planeTimers[plane] != null)
        {
            StopCoroutine(planeTimers[plane]);
        }

        // Start a new timer
        Coroutine timer = StartCoroutine(HidePlaneAfterDelay(plane));
        planeTimers[plane] = timer;
        Debug.Log($"PlaneVisibilityManager: Started timer for plane - Will hide in {hideDelay} seconds");
    }

    private IEnumerator HidePlaneAfterDelay(ARPlane plane)
    {
        yield return new WaitForSeconds(hideDelay);

        if (plane != null && plane.gameObject != null)
        {
            Debug.Log("PlaneVisibilityManager: Timer completed - Hiding plane");
            // Get the mesh renderer and disable it (keeping the plane active for detection)
            MeshRenderer meshRenderer = plane.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.enabled = false;
                Debug.Log("PlaneVisibilityManager: Disabled mesh renderer");
            }

            // Also disable the line renderer if it exists
            LineRenderer lineRenderer = plane.GetComponent<LineRenderer>();
            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
                Debug.Log("PlaneVisibilityManager: Disabled line renderer");
            }
        }
    }

    private IEnumerator MonitorObjectPlacement()
    {
        float lastPlacementTime = 0f;

        while (true)
        {
            // Check if an object was recently placed
            if (Time.time - lastPlacementTime > 0.1f && // Small threshold to avoid multiple checks
                placeObjectScript != null &&
                !placeObjectScript.allowPlacement) // allowPlacement is false during placement
            {
                lastPlacementTime = Time.time;
                ShowAllPlanes();
                Debug.Log("PlaneVisibilityManager: Object placed - Showing all planes");
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private void ShowAllPlanes()
    {
        foreach (ARPlane plane in planeManager.trackables)
        {
            // Show the plane
            MeshRenderer meshRenderer = plane.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.enabled = true;
            }

            LineRenderer lineRenderer = plane.GetComponent<LineRenderer>();
            if (lineRenderer != null)
            {
                lineRenderer.enabled = true;
            }

            // Restart the timer for this plane
            StartPlaneTimer(plane);
        }
        Debug.Log("PlaneVisibilityManager: Showed all planes and restarted timers");
    }
}