using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIStateManager : MonoBehaviour
{
    public ObjectGrabber grabber;
    public GameObject grabButton;
    public GameObject dropButton;
    private PlaceObjectOnPlane placeObjectScript;

    void Start()
    {
        if (placeObjectScript == null)
        {
            placeObjectScript = FindObjectOfType<PlaceObjectOnPlane>();
        }
    }

    public void OnGrabPressed()
    {
        if (grabber != null)
        {
            // Disable object placement before grab
            if (placeObjectScript != null)
            {
                placeObjectScript.allowPlacement = false;
            }

            grabber.Grab();
            grabButton.SetActive(false);
            dropButton.SetActive(true);

            // Small delay before re-enabling placement if no object was grabbed
            StartCoroutine(CheckGrabResult());
        }
    }

    private IEnumerator CheckGrabResult()
    {
        yield return new WaitForSeconds(0.1f);
        if (grabber != null && !grabber.IsHolding() && placeObjectScript != null)
        {
            placeObjectScript.allowPlacement = true;
        }
    }

    public void OnDropPressed()
    {
        if (grabber != null)
        {
            // Disable object placement during drop
            if (placeObjectScript != null)
            {
                placeObjectScript.allowPlacement = false;
            }

            grabber.Drop();
            grabButton.SetActive(true);
            dropButton.SetActive(false);

            // Re-enable placement after a short delay
            StartCoroutine(EnablePlacementAfterDrop());
        }
    }

    private IEnumerator EnablePlacementAfterDrop()
    {
        yield return new WaitForSeconds(0.2f);
        if (placeObjectScript != null)
        {
            placeObjectScript.allowPlacement = true;
        }
    }
}