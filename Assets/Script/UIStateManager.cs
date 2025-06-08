using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStateManager : MonoBehaviour
{
    public ObjectGrabber grabber;
    public GameObject grabButton;
    public GameObject dropButton;

    public void OnGrabPressed()
    {
        if (grabber != null)
        {
            grabber.Grab();
            grabButton.SetActive(false);
            dropButton.SetActive(true);
        }
    }

    public void OnDropPressed()
    {
        if (grabber != null)
        {
            grabber.Drop();
            grabButton.SetActive(true);
            dropButton.SetActive(false);
        }
    }
}