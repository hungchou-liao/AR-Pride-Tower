using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIStateManager : MonoBehaviour
{
    [Header("Object Manipulation")]
    public ObjectGrabber grabber;
    public GameObject grabButton;
    public GameObject dropButton;

    [Header("Additional UI Controls")]
    [SerializeField] private Button exitButton;
    [SerializeField] private Button downloadButton;
    [SerializeField] private Button resetButton;

    private PlaceObjectOnPlane placeObjectScript;
    private ScreenshotManager screenshotManager;
    private ResetManager resetManager;

    void Start()
    {
        // Find PlaceObjectOnPlane script
        if (placeObjectScript == null)
        {
            placeObjectScript = FindObjectOfType<PlaceObjectOnPlane>();
        }

        // Get or add required components
        screenshotManager = gameObject.GetComponent<ScreenshotManager>() ?? gameObject.AddComponent<ScreenshotManager>();
        resetManager = gameObject.GetComponent<ResetManager>() ?? gameObject.AddComponent<ResetManager>();

        // Set up additional button listeners
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitApplication);
        }

        if (downloadButton != null)
        {
            downloadButton.onClick.AddListener(() => screenshotManager.CaptureAndSaveScreenshot());
        }

        if (resetButton != null)
        {
            resetButton.onClick.AddListener(() => resetManager.ResetScene());
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

    private void ExitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    void OnDestroy()
    {
        // Clean up listeners
        if (exitButton != null)
            exitButton.onClick.RemoveListener(ExitApplication);

        if (downloadButton != null)
            downloadButton.onClick.RemoveAllListeners();

        if (resetButton != null)
            resetButton.onClick.RemoveAllListeners();
    }
}