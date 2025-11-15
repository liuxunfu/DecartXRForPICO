using UnityEngine;
using System.Collections;

// Execute within Unity Editor
public class WebCamTextureManager : MonoBehaviour
{
    public Vector2Int videoResolution = new Vector2Int(1280, 720); // Resolution of video stream captured by camera

    public WebCamTexture WebCamTexture { get; private set; }

    private WebCamDevice webCamDevice;

#if UNITY_EDITOR
    private void Awake()
    {
        // Request camera usage permission
        Application.RequestUserAuthorization(UserAuthorization.WebCam);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam) || WebCamTexture.devices.Length <= 0)
        {
            return;
        }

        foreach (WebCamDevice device in WebCamTexture.devices)
        {
            if (device.isFrontFacing)
            {
                // Get the front camera
                webCamDevice = device;
                break;
            }
        }

        StartCoroutine(StartCamera());
    }
#endif

    IEnumerator StartCamera()
    {
        yield return new WaitForSeconds(1);

        // Create a WebCamTexture
        WebCamTexture = new WebCamTexture(webCamDevice.name, videoResolution.x, videoResolution.y);

        // Real time acquisition of camera video streams
        WebCamTexture.Play();
    }
}
