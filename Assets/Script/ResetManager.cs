using UnityEngine;
using UnityEngine.UI;

public class ResetManager : MonoBehaviour
{
    private PlaceObjectOnPlane placeObjectScript;

    void Start()
    {
        // Find the PlaceObjectOnPlane script
        placeObjectScript = FindObjectOfType<PlaceObjectOnPlane>();

        // Get the Button component and add listener
        Button resetButton = GetComponent<Button>();
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ResetScene);
        }
    }

    public void ResetScene()
    {
        // Temporarily disable object placement
        if (placeObjectScript != null)
        {
            placeObjectScript.allowPlacement = false;
        }

        // Find all objects with the PlacedObject tag and destroy them
        GameObject[] placedObjects = GameObject.FindGameObjectsWithTag("PlacedObject");
        foreach (GameObject obj in placedObjects)
        {
            Destroy(obj);
        }

        // Re-enable object placement after a short delay
        if (placeObjectScript != null)
        {
            Invoke("EnablePlacement", 0.2f);
        }
    }

    private void EnablePlacement()
    {
        if (placeObjectScript != null)
        {
            placeObjectScript.allowPlacement = true;
        }
    }
}