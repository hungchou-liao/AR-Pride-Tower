using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectGrabber : MonoBehaviour
{
    public string targetTag = "PlacedObject";
    public float grabDistance = 0.15f; // Very short grab distance
    public float grabRadius = 0.05f; // Very small grab radius
    private Transform arCamera;
    private GameObject heldObject;
    private Vector3 originalScale;
    private Vector3 originalLocalPosition;
    private Quaternion originalRotation;

    private Transform grabAnchorTransform;
    private PlaceObjectOnPlane placeObjectScript;

    void Start()
    {
        arCamera = Camera.main.transform;

        // Create a holding point in the top 1/3 of the screen, slightly in front of the camera
        GameObject grabAnchor = new GameObject("GrabAnchor");
        grabAnchor.transform.SetParent(arCamera);
        // Position the anchor point higher up (y=0.3) and closer to the camera (z=0.5)
        grabAnchor.transform.localPosition = new Vector3(0, 0.3f, 0.5f);
        grabAnchor.transform.localRotation = Quaternion.identity;
        grabAnchorTransform = grabAnchor.transform;

        // Find the PlaceObjectOnPlane script
        placeObjectScript = FindObjectOfType<PlaceObjectOnPlane>();
    }

    public void Grab()
    {
        if (heldObject != null) return;

        // Disable object placement immediately when attempting to grab
        if (placeObjectScript != null)
        {
            placeObjectScript.allowPlacement = false;
        }

        // Raycast from the center of the screen
        Ray ray = new Ray(arCamera.position, arCamera.forward);
        RaycastHit[] hits = Physics.SphereCastAll(ray, grabRadius, grabDistance);

        GameObject closestObject = null;
        float closestDistance = float.MaxValue;

        // Find the closest detected object
        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag(targetTag))
            {
                GameObject hitObject = hit.collider.gameObject;
                float distance = Vector3.Distance(arCamera.position, hitObject.transform.position);

                // Only consider objects that are detected (green)
                if (ObjectDetector.IsObjectDetected(hitObject) && distance < closestDistance)
                {
                    closestDistance = distance;
                    closestObject = hitObject;
                }
            }
        }

        // If we found a valid object, grab it
        if (closestObject != null)
        {
            heldObject = closestObject;

            // Store original properties
            originalScale = heldObject.transform.localScale;
            originalLocalPosition = heldObject.transform.localPosition;
            originalRotation = heldObject.transform.rotation;

            // Parent to the grab anchor while maintaining world scale
            heldObject.transform.SetParent(grabAnchorTransform);

            // Maintain the object's original scale and rotation
            heldObject.transform.localScale = originalScale;
            heldObject.transform.rotation = originalRotation;

            var rb = heldObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                rb.isKinematic = true;
            }
        }

        // If no object was grabbed, re-enable placement
        if (heldObject == null && placeObjectScript != null)
        {
            placeObjectScript.allowPlacement = true;
        }
    }

    public void Drop()
    {
        if (heldObject == null) return;

        // Keep placement disabled during the drop action
        StartCoroutine(EnablePlacementAfterDelay());

        // Unparent the object
        heldObject.transform.SetParent(null);

        // Keep the current position but restore original rotation
        heldObject.transform.rotation = originalRotation;

        var rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
        }

        heldObject = null;
    }

    private IEnumerator EnablePlacementAfterDelay()
    {
        // Wait for a short time to ensure the drop action is complete
        yield return new WaitForSeconds(0.2f);

        // Re-enable object placement
        if (placeObjectScript != null)
        {
            placeObjectScript.allowPlacement = true;
        }
    }

    void Update()
    {
        // Add smooth movement to follow the grab anchor
        if (heldObject != null)
        {
            heldObject.transform.rotation = Quaternion.Slerp(
                heldObject.transform.rotation,
                grabAnchorTransform.rotation * Quaternion.Euler(0, 0, 0),
                Time.deltaTime * 10f
            );
        }
    }

    // Public method to check if currently holding an object
    public bool IsHolding()
    {
        return heldObject != null;
    }

    void OnDrawGizmos()
    {
        // Draw debug visualization in editor
        if (arCamera != null)
        {
            Gizmos.color = Color.blue;
            Vector3 sphereEnd = arCamera.position + arCamera.forward * grabDistance;
            Gizmos.DrawWireSphere(sphereEnd, grabRadius);
            Gizmos.DrawLine(arCamera.position, sphereEnd);
        }
    }
}