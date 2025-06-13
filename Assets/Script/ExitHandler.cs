using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ExitHandler : MonoBehaviour
{
    private void Start()
    {
        // Get the Button component and add listener
        Button exitButton = GetComponent<Button>();
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(QuitApplication);
        }
    }

    private void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}