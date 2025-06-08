using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectGrabber : MonoBehaviour
{
    public string targetTag = "PlacedObject";
    public float grabDistance = 0.3f;
    private Transform arCamera;
    private GameObject heldObject;
    private Vector3 originalScale;

    void Start()
    {
        arCamera = Camera.main.transform;
    }

    public void Grab()
    {
        if (heldObject != null) return;

        Collider[] colliders = Physics.OverlapSphere(arCamera.position, grabDistance);
        foreach (var col in colliders)
        {
            if (col.CompareTag(targetTag))
            {
                heldObject = col.gameObject;
                originalScale = heldObject.transform.localScale;

                heldObject.transform.SetParent(arCamera);
                heldObject.transform.localPosition = heldObject.transform.localPosition; // keep current world position
                heldObject.transform.localRotation = Quaternion.identity;

                var rb = heldObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.useGravity = false;
                    rb.isKinematic = true;
                }
                break;
            }
        }
    }

    public void Drop()
    {
        if (heldObject == null) return;

        heldObject.transform.SetParent(null);
        heldObject.transform.localScale = originalScale;

        var rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
        }

        heldObject = null;
    }
}