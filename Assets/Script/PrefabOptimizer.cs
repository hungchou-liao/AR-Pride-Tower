using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class PrefabOptimizer : MonoBehaviour
{
    [Header("Prefab Settings")]
    [SerializeField] private float targetScale = 0.2f; // Default scale for AR objects
    [SerializeField] private bool useMeshCollider = true;
    [SerializeField] private bool optimizeMeshes = true;
    [SerializeField] private float colliderMargin = 0.01f;

    [Header("Physics Settings")]
    [SerializeField] private float mass = 1f;
    [SerializeField] private float drag = 0.5f;
    [SerializeField] private float angularDrag = 0.05f;
    [SerializeField] private bool useGravity = true;

    private void Start()
    {
        Debug.Log("PrefabOptimizer: Starting optimization...");
        OptimizeAllPrefabs();
    }

    public void OptimizeAllPrefabs()
    {
        // Find all prefabs in the Resources/Prefabs folder
        GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs");

        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogError("No prefabs found in Resources/Prefabs folder!");
            return;
        }

        Debug.Log($"Found {prefabs.Length} prefabs to optimize");

        foreach (GameObject prefab in prefabs)
        {
            if (prefab == null) continue;

            // Create a temporary instance to modify
            GameObject instance = Instantiate(prefab);

            // Optimize the instance
            OptimizePrefab(instance);

            // Save the optimized prefab
            SaveOptimizedPrefab(instance, prefab.name);

            // Clean up
            DestroyImmediate(instance);
        }

        Debug.Log("Prefab optimization complete!");
    }

    private void OptimizePrefab(GameObject prefab)
    {
        // Set the scale
        prefab.transform.localScale = Vector3.one * targetScale;

        // Get or add required components
        MeshFilter meshFilter = prefab.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = prefab.AddComponent<MeshFilter>();
        }

        // Handle colliders
        Collider existingCollider = prefab.GetComponent<Collider>();
        if (existingCollider != null)
        {
            DestroyImmediate(existingCollider);
        }

        if (useMeshCollider && meshFilter != null && meshFilter.sharedMesh != null)
        {
            MeshCollider meshCollider = prefab.AddComponent<MeshCollider>();
            meshCollider.convex = true; // Required for Rigidbody
            meshCollider.skinWidth = colliderMargin;
        }
        else
        {
            BoxCollider boxCollider = prefab.AddComponent<BoxCollider>();
            // Adjust box collider size to match mesh bounds
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                Bounds bounds = meshFilter.sharedMesh.bounds;
                boxCollider.size = bounds.size;
                boxCollider.center = bounds.center;
            }
        }

        // Add or configure Rigidbody
        Rigidbody rb = prefab.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = prefab.AddComponent<Rigidbody>();
        }

        // Configure Rigidbody
        rb.mass = mass;
        rb.drag = drag;
        rb.angularDrag = angularDrag;
        rb.useGravity = useGravity;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Add AR interaction components if they don't exist
        if (prefab.GetComponent<ObjectGrabber>() == null)
        {
            prefab.AddComponent<ObjectGrabber>();
        }

        if (prefab.GetComponent<StackableObject>() == null)
        {
            prefab.AddComponent<StackableObject>();
        }
    }

    private void SaveOptimizedPrefab(GameObject instance, string originalName)
    {
#if UNITY_EDITOR
        string prefabPath = $"Assets/Resources/Prefabs/AR_Objects/{originalName}_Optimized.prefab";
        
        // Create directory if it doesn't exist
        Directory.CreateDirectory(Path.GetDirectoryName(prefabPath));

        // Create the prefab
        PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        Debug.Log($"Saved optimized prefab: {prefabPath}");
#endif
    }
}