using UnityEngine;
using System.Collections;
/// <summary>
/// Controls the cameras
/// Must be on an empty game object
/// Allows to switch between cameras
/// </summary>
public class CameraController : MonoBehaviour
{
    public Camera[] cameras;
    private int currentCam = 0;

    // Use this for initialization
    void Start()
    {
        // Default to first camera (disable others)
        for (int i = 1; i < cameras.Length; i++)
        {
            cameras[i].gameObject.SetActive(false);
        }

        // If any cameras were added to the controller, enable the first one
        if (cameras.Length > 0)
        {
            cameras[0].gameObject.SetActive(true);
            Debug.Log("Camera with name: " + cameras[0].GetComponent<Camera>().name + ", is now enabled");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // C to switch to next camera
        if (Input.GetKeyDown(KeyCode.C))
        {
            currentCam++;
            Debug.Log("Current Camera: " + currentCam);
            if (currentCam < cameras.Length)
            {
                //Set the camera at the current index to inactive, and set the next one in the array to active
                cameras[currentCam - 1].gameObject.SetActive(false);
                cameras[currentCam].gameObject.SetActive(true);
                Debug.Log(cameras[currentCam].GetComponent<Camera>().name + " enabled");
            }
            else
            {
                // When reach the end of the camera array, move back to the beginning or the array
                cameras[currentCam - 1].gameObject.SetActive(false);
                currentCam = 0;
                cameras[currentCam].gameObject.SetActive(true);
                Debug.Log(cameras[currentCam].GetComponent<Camera>().name + " enabled");
            }
        }
    }
}
