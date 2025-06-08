using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDetector : MonoBehaviour
{
    public float detectionDistance = 0.08f; // Much smaller detection range
    public Material highlightMaterial;
    private Material originalMaterial;
    private Renderer objRenderer;
    private static GameObject closestObject;
    private static float minDistance = float.MaxValue;
    private static Transform arCamera;
    private static bool isObjectInView = false;
    private float deadZoneHeight;

    void Start()
    {
        if (arCamera == null)
        {
            arCamera = Camera.main.transform;
        }

        objRenderer = GetComponent<Renderer>();
        if (objRenderer != null)
        {
            originalMaterial = objRenderer.material;
        }

        // Calculate the dead zone height (1/6 of screen height)
        deadZoneHeight = Screen.height / 6f;
    }

    void Update()
    {
        // Reset static variables at the start of each frame
        if (gameObject == closestObject)
        {
            closestObject = null;
            minDistance = float.MaxValue;
            isObjectInView = false;
        }

        // Check if object is in view
        Vector3 directionToCamera = arCamera.position - transform.position;
        float angleToCamera = Vector3.Angle(directionToCamera, -transform.forward);
        bool inFieldOfView = angleToCamera < 90f;

        // Check if object is in screen dead zone
        bool inDeadZone = false;
        if (inFieldOfView)
        {
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(transform.position);
            inDeadZone = screenPoint.y <= deadZoneHeight;
        }

        // Calculate distance to camera
        float distance = Vector3.Distance(arCamera.position, transform.position);

        // Only consider objects within detection range, in view, and not in dead zone
        if (distance <= detectionDistance && inFieldOfView && !inDeadZone)
        {
            // Update closest object if this one is closer
            if (distance < minDistance)
            {
                if (closestObject != null && closestObject != gameObject)
                {
                    // Reset previous closest object's material
                    var prevRenderer = closestObject.GetComponent<Renderer>();
                    var prevDetector = closestObject.GetComponent<ObjectDetector>();
                    if (prevRenderer != null && prevDetector != null)
                    {
                        prevRenderer.material = prevDetector.originalMaterial;
                    }
                }

                minDistance = distance;
                closestObject = gameObject;
                isObjectInView = true;
            }
        }

        // Update material based on whether this is the closest object
        if (objRenderer != null)
        {
            if (gameObject == closestObject && isObjectInView)
            {
                objRenderer.material = highlightMaterial;
            }
            else
            {
                objRenderer.material = originalMaterial;
            }
        }
    }

    void OnDisable()
    {
        // Reset material when object is disabled
        if (objRenderer != null)
        {
            objRenderer.material = originalMaterial;
        }

        // Reset static variables if this was the closest object
        if (gameObject == closestObject)
        {
            closestObject = null;
            minDistance = float.MaxValue;
            isObjectInView = false;
        }
    }

    public bool IsDetecting()
    {
        return gameObject == closestObject && isObjectInView;
    }

    // Static method to check if an object is currently detected
    public static bool IsObjectDetected(GameObject obj)
    {
        return obj == closestObject && isObjectInView;
    }
}