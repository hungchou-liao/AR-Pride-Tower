using UnityEngine;
using UnityEngine.UI;
using TMPro; // Add TextMeshPro namespace
using System;
using System.IO;
using System.Collections;

public class ScreenshotManager : MonoBehaviour
{
    private Button downloadButton;
    [SerializeField] private TMP_Text successMessageText; // Changed from Text to TMP_Text
    private float messageDisplayTime = 2f; // How long to show the message

    void Start()
    {
        // Get the Button component and add listener
        downloadButton = GetComponent<Button>();
        if (downloadButton != null)
        {
            downloadButton.onClick.AddListener(CaptureAndSaveScreenshot);
        }

        // Initialize success message text if assigned
        if (successMessageText != null)
        {
            successMessageText.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Success message TMP_Text not assigned in ScreenshotManager!");
        }
    }

    private void ShowSuccessMessage(string message)
    {
        if (successMessageText != null)
        {
            successMessageText.text = message;
            successMessageText.gameObject.SetActive(true);
            StartCoroutine(HideMessageAfterDelay());
        }
    }

    private IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDisplayTime);
        if (successMessageText != null)
        {
            successMessageText.gameObject.SetActive(false);
        }
    }

    public void CaptureAndSaveScreenshot()
    {
        StartCoroutine(CaptureScreenshot());
    }

    private IEnumerator CaptureScreenshot()
    {
        // Wait for the end of frame so we can capture everything that was rendered
        yield return new WaitForEndOfFrame();

        // Create a unique filename using timestamp
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filename = $"ARScreenshot_{timestamp}.png";

        // Get the path for saving
        string folderPath = Path.Combine(Application.persistentDataPath, "Screenshots");
        string fullPath = Path.Combine(folderPath, filename);

        // Create the directory if it doesn't exist
        Directory.CreateDirectory(folderPath);

        try
        {
            // Take the screenshot
            Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            screenshot.Apply();

            // Convert to bytes and save
            byte[] bytes = screenshot.EncodeToPNG();
            File.WriteAllBytes(fullPath, bytes);

            // Clean up
            Destroy(screenshot);

            // Show success message
            Debug.Log($"Screenshot saved to: {fullPath}");
            ShowSuccessMessage("Screenshot saved successfully!");

            // On iOS, make the image available in the photo gallery
#if UNITY_IOS
            UnityEngine.iOS.Device.SetNoBackupFlag(fullPath);
#endif

            // Show the screenshot in the gallery on Android
#if UNITY_ANDROID
            AndroidJavaClass classPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject objActivity = classPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass classUri = new AndroidJavaClass("android.net.Uri");
            AndroidJavaObject objIntent = new AndroidJavaObject("android.content.Intent", new object[2]
            { "android.intent.action.MEDIA_SCANNER_SCAN_FILE", classUri.CallStatic<AndroidJavaObject>("parse", "file://" + fullPath) });
            objActivity.Call("sendBroadcast", objIntent);
#endif
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save screenshot: {e.Message}");
            ShowSuccessMessage("Failed to save screenshot!");
        }
    }
}