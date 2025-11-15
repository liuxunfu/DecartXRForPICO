using UnityEngine;
using System;
using System.Runtime.InteropServices;
using Unity.XR.PICO.TOBSupport;
using Unity.XR.PXR;
using System.Collections;

// PICO PassThrough Camera Manager
public class PassThroughCameraManager : MonoBehaviour
{
    public PXRCaptureRenderMode renderMode = PXRCaptureRenderMode.PXRCapture_RenderMode_LEFT;// Camera type
    public Vector2Int videoResolution = new Vector2Int(1280, 720); // Resolution of video stream captured by camera

    private readonly string mTag = "CameraCapture ---";

    private PXR_CompositionLayer compositionLayer = null;
    private byte[] imgByte;
    private bool isRuning = false;

    [HideInInspector]
    public Texture2D texture;// Convert video frames to Unity textures

    private void Awake()
    {
        compositionLayer = GetComponent<PXR_CompositionLayer>();

        if (compositionLayer == null)
        {
            Debug.LogError("PXRLog CompositionLayer is null!");
            compositionLayer = gameObject.AddComponent<PXR_CompositionLayer>();
        }

        imgByte = new byte[videoResolution.x * videoResolution.y * 4];
        texture = new Texture2D(videoResolution.x, videoResolution.y, TextureFormat.RGBA32, false);

        PXR_Enterprise.Configurefor4U();

        PXR_Enterprise.OpenCameraAsyncfor4U(ret =>
        {
            Debug.Log($"{mTag}  OpenCameraAsync ret=  {ret}");
        });
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        StartGetImageData();
    }

    // Start previewing the camera image on the surface of the medium
    public void StartPreview()
    {
        Debug.Log($"{mTag} StartPreview ");
        compositionLayer.isExternalAndroidSurface = true;
        Debug.Log($"{mTag} externalAndroidSurfaceObject " + compositionLayer.externalAndroidSurfaceObject);
        PXR_Enterprise.StartPreviewfor4U(compositionLayer.externalAndroidSurfaceObject, renderMode);
    }

    // Get passthrough camera image data
    public void StartGetImageData()
    {
        Debug.Log($"{mTag} StartGetImageData ");
        compositionLayer.isExternalAndroidSurface = false;

        IntPtr data = Marshal.UnsafeAddrOfPinnedArrayElement(imgByte, 0);
        PXR_Enterprise.SetCameraFrameBufferfor4U(videoResolution.x, videoResolution.y, ref data, (Frame frame) =>
        {
            texture.LoadRawTextureData(imgByte);
            texture.Apply();
        });

        bool ret = PXR_Enterprise.StartGetImageDatafor4U(renderMode, videoResolution.x, videoResolution.y);
        isRuning = true;
        Debug.Log($"{mTag}  OpenCameraAsync ret=  {ret}");
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (isRuning)
        {
            if (pauseStatus)
            {
                PXR_Enterprise.CloseCamerafor4U();
            }
            else
            {
                PXR_Enterprise.OpenCameraAsyncfor4U(ret =>
                {
                    Debug.Log($"{mTag}  OpenCameraAsync ret=  {ret}");
                });
            }
        }
    }
}
