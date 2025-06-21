using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;


public class ARObjectShadow : MonoBehaviour
{
    [SerializeField] private float shadowOffset = 0.01f; // How far above the plane the shadow appears
    [SerializeField] private float baseShadowScale = 0.2f; // Base scale of the shadow
    [SerializeField] private Color shadowColor = new Color(0, 0, 0, 0.5f); // Semi-transparent black
    [SerializeField] private float maxShadowScale = 1.0f; // Maximum scale the shadow can reach
    [SerializeField] private float minShadowScale = 0.1f; // Minimum scale the shadow can reach

    private GameObject shadowQuad;
    private ARRaycastManager raycastManager;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private Renderer shadowRenderer;
    private Material shadowMaterial;

    void Start()
    {
        // Create shadow quad
        shadowQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        shadowQuad.name = "ShadowQuad";
        shadowQuad.transform.parent = transform;

        // Remove the collider from the shadow
        Destroy(shadowQuad.GetComponent<Collider>());

        // Create and assign material
        shadowMaterial = new Material(Shader.Find("Unlit/Transparent"));
        shadowMaterial.color = shadowColor;
        shadowRenderer = shadowQuad.GetComponent<Renderer>();
        shadowRenderer.material = shadowMaterial;

        // Get AR Raycast Manager
        raycastManager = FindObjectOfType<ARRaycastManager>();
        if (raycastManager == null)
        {
            Debug.LogError("ARRaycastManager not found in scene!");
            enabled = false;
            return;
        }
    }

    void Update()
    {
        if (shadowQuad == null || raycastManager == null) return;

        // Cast ray downward to find the AR plane
        if (raycastManager.Raycast(transform.position + Vector3.up * 0.1f, hits, TrackableType.PlaneWithinPolygon))
        {
            // Get the hit point
            Vector3 hitPoint = hits[0].pose.position;

            // Calculate distance from object to plane
            float distanceToPlane = Vector3.Distance(transform.position, hitPoint);

            // Position the shadow slightly above the plane
            shadowQuad.transform.position = hitPoint + Vector3.up * shadowOffset;

            // Make shadow face up
            shadowQuad.transform.rotation = Quaternion.Euler(90, 0, 0);

            // Scale shadow based on distance to plane
            // The further the object is from the plane, the larger the shadow
            float scaleMultiplier = 1f + (distanceToPlane * 2f); // Adjust multiplier as needed
            float scale = Mathf.Clamp(baseShadowScale * scaleMultiplier, minShadowScale, maxShadowScale);
            shadowQuad.transform.localScale = new Vector3(scale, scale, 1f);

            // Adjust shadow opacity based on distance to plane
            // The further the object is from the plane, the more transparent the shadow
            Color currentColor = shadowColor;
            currentColor.a = Mathf.Clamp01(1f - (distanceToPlane * 0.5f)); // Adjust fade rate as needed
            shadowMaterial.color = currentColor;

            shadowQuad.SetActive(true);
        }
        else
        {
            // Hide shadow if no plane is detected
            shadowQuad.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (shadowQuad != null)
        {
            Destroy(shadowQuad);
        }
        if (shadowMaterial != null)
        {
            Destroy(shadowMaterial);
        }
    }
}