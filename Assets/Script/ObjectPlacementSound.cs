using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ObjectPlacementSound : MonoBehaviour
{
    [SerializeField]
    private AudioClip placementSound;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        if (placementSound != null)
        {
            audioSource.PlayOneShot(placementSound);
        }
        else
        {
            Debug.LogWarning("Placement sound (AudioClip) not assigned in ObjectPlacementSound script on " + gameObject.name);
        }
    }
}