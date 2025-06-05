using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDetector : MonoBehaviour
{
    public float detectionDistance = 0.3f; // how close the camera needs to be
    public Material highlightMaterial;
    private Material originalMaterial;
    private Renderer objRenderer;
    private Transform arCamera;

    void Start()
    {
        arCamera = Camera.main.transform;
        objRenderer = GetComponent<Renderer>();
        originalMaterial = objRenderer.material;
    }

    void Update()
    {
        float distance = Vector3.Distance(arCamera.position, transform.position);

        if (distance <= detectionDistance)
        {
            objRenderer.material = highlightMaterial;
        }
        else
        {
            objRenderer.material = originalMaterial;
        }
    }
}
