using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation.Samples;

[RequireComponent(typeof(ARPlaneManager))]
public class PlaneVisibilityManager : MonoBehaviour
{
    private ARPlaneManager planeManager;

    void Start()
    {
        planeManager = GetComponent<ARPlaneManager>();

        Debug.Log("PlaneVisibilityManager: Started");
        Debug.Log($"PlaneVisibilityManager: Found planeManager: {planeManager != null}");

        // Subscribe to plane events to get newly added planes
        planeManager.planesChanged += OnPlanesChanged;

        // Subscribe to object placement event
        PlaceObjectOnPlane.OnObjectPlaced += CheckAndHidePlanes;
    }

    void OnDisable()
    {
        if (planeManager != null)
        {
            planeManager.planesChanged -= OnPlanesChanged;
        }
        PlaceObjectOnPlane.OnObjectPlaced -= CheckAndHidePlanes;
    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        Debug.Log($"PlaneVisibilityManager: Planes changed - Added: {args.added.Count}, Removed: {args.removed.Count}");

        // Set newly detected planes to visible (they will be hidden later if count >= 5)
        foreach (ARPlane plane in args.added)
        {
            SetPlaneVisibility(plane, true);
        }
    }

    // Method called when an object is placed
    private void CheckAndHidePlanes()
    {
        Debug.Log($"PlaneVisibilityManager: Object placed. Total objects: {PlaceObjectOnPlane.placedObjectCount}");
        if (PlaceObjectOnPlane.placedObjectCount >= 5)
        {
            HideAllPlanes();
        }
    }

    private void HideAllPlanes()
    {
        Debug.Log("PlaneVisibilityManager: Hiding all planes.");
        foreach (ARPlane plane in planeManager.trackables)
        {
            SetPlaneVisibility(plane, false);
        }
    }

    private void SetPlaneVisibility(ARPlane plane, bool visible)
    {
        // Disable MeshRenderer
        MeshRenderer meshRenderer = plane.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = visible;
        }

        // Disable LineRenderer
        LineRenderer lineRenderer = plane.GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.enabled = visible;
        }

        // Disable primary visualizer components
        ARPlaneMeshVisualizer meshVisualizer = plane.GetComponent<ARPlaneMeshVisualizer>();
        if (meshVisualizer != null)
        {
            meshVisualizer.enabled = visible;
        }

        ARFeatheredPlaneMeshVisualizer featheredVisualizer = plane.GetComponent<ARFeatheredPlaneMeshVisualizer>();
        if (featheredVisualizer != null)
        {
            featheredVisualizer.enabled = visible;
        }

        // Optionally, disable any other relevant visualizers/colliders if planes are still visible
        // Collider planeCollider = plane.GetComponent<Collider>();
        // if (planeCollider != null)
        // {
        //     planeCollider.enabled = visible;
        // }
    }
}