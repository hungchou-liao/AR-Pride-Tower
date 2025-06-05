using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaceObjectOnPlane : MonoBehaviour
{
    [SerializeField] GameObject placedPrefab;
    GameObject spawnedObject;

    ARRaycastManager raycaster;
    List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private void Start()
    {
        raycaster = GetComponent<ARRaycastManager>();
    }

    public void OnPlaceObject(InputValue value)
    {
        Vector2 touchPosition = value.Get<Vector2>();
        Debug.Log("Tapped screen at: " + touchPosition);

        if (raycaster.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            Debug.Log("Plane hit at: " + hitPose.position);

            GameObject newObject = Instantiate(placedPrefab, hitPose.position, hitPose.rotation);
            Debug.Log("New object spawned at: " + hitPose.position);

        }
        else
        {
            Debug.Log("No plane hit.");
        }
    }
}