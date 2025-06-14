using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDetector : MonoBehaviour
{
    public float detectionDistance = 1.0f;
    public Material highlightMaterial;
    private Material originalMaterial;
    private Renderer objRenderer;
    public static GameObject closestObject;
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
        else
        {
            Debug.LogError($"ObjectDetector on {gameObject.name}: No Renderer component found!");
        }

        if (highlightMaterial == null)
        {
            Debug.LogError($"ObjectDetector on {gameObject.name}: Highlight Material is not assigned!");
        }

        if (!gameObject.CompareTag("PlacedObject"))
        {
            Debug.LogError($"ObjectDetector on {gameObject.name}: Object does not have 'PlacedObject' tag!");
        }

        deadZoneHeight = Screen.height / 6f;
    }

    void Update()
    {
        // Reset static variables at the start of each frame IF THIS IS THE CLOSEST OBJECT
        if (gameObject == closestObject)
        {
            closestObject = null;
            minDistance = float.MaxValue;
            isObjectInView = false;
        }

        Vector3 directionToCamera = arCamera.position - transform.position;
        float distance = Vector3.Distance(arCamera.position, transform.position);

        // Simplified detection - only check distance and dead zone
        bool inDeadZone = false;
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(transform.position);
        inDeadZone = screenPoint.y <= deadZoneHeight;

        if (distance <= detectionDistance && !inDeadZone)
        {
            if (distance < minDistance)
            {
                if (closestObject != null && closestObject != gameObject)
                {
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
                Debug.Log($"Detected object: {gameObject.name} at distance: {distance}");
            }
        }

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
        if (objRenderer != null)
        {
            objRenderer.material = originalMaterial;
        }

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

    public static bool IsObjectDetected(GameObject obj)
    {
        return obj == closestObject && isObjectInView;
    }
}