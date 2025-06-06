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

    ARRaycastManager raycaster;
    List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private float lastPlaceTime = 0f;
    public float placementCooldown = 1f;

    private void Start()
    {
        raycaster = GetComponent<ARRaycastManager>();
    }

    public void OnPlaceObject(InputValue value)
    {
        if (!allowPlacement) return;

        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (Time.time - lastPlaceTime < placementCooldown)
        {
            Debug.Log("Placement cooldown active. Please wait.");
            return;
        }

        Vector2 touchPosition = value.Get<Vector2>();
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