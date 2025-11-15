using SimpleWebRTC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PICOCameraKit.WebRTC
{
    public class WebRTCController : MonoBehaviour
    {
        [Tooltip("UI RawImage where the local passthrough camera feed will be displayed.")]
        [SerializeField] private RawImage canvasRawImage;

        [Tooltip("UI Text element that shows the current prompt fed to the Decart model.")]
        [SerializeField] private TMP_Text promptNameText;

        [Tooltip("Reference to the WebRTCConnection handling signaling and video streaming.")]
        [SerializeField] private WebRTCConnection webRtcConnection;

        [Tooltip("Manager for controlling the WebCamTexture on PC devices.")]
        [SerializeField] private WebCamTextureManager pcCameraManager;

        [Tooltip("Manager for controlling the Texture on PICO devices.")]
        [SerializeField] private PassThroughCameraManager passthroughCameraManager;

        private bool _videoReceivedAndReady;
        private WebCamTexture _webcamTexture;
        //private readonly Queue<string> _promptQueue = new();

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        IEnumerator Start()
        {
            if (webRtcConnection == null)
            {
                webRtcConnection = FindFirstObjectByType<WebRTCConnection>();
            }

#if UNITY_EDITOR
            if (pcCameraManager == null)
            {
                pcCameraManager = FindFirstObjectByType<WebCamTextureManager>();
            }

            if (pcCameraManager == null || webRtcConnection == null)
            {
                Debug.LogError("WebRTCController: Missing required components.");
                yield break;
            }

            var timeout = Time.time + 5f;
            yield return new WaitUntil(() =>
                (pcCameraManager.WebCamTexture != null &&
                 pcCameraManager.WebCamTexture.isPlaying) ||
                Time.time > timeout);

            if (pcCameraManager.WebCamTexture == null || !pcCameraManager.WebCamTexture.isPlaying)
            {
                Debug.LogError("WebRTCController: Camera failed to start.");
                yield break;
            }

            _webcamTexture = pcCameraManager.WebCamTexture;
            if (canvasRawImage != null)
            {
                canvasRawImage.texture = _webcamTexture;
            }
#else
            if (passthroughCameraManager == null)
            {
                passthroughCameraManager = FindFirstObjectByType<PassThroughCameraManager>();
            }

            if (passthroughCameraManager == null || webRtcConnection == null)
            {
                Debug.LogError("WebRTCController: Missing required components.");
                yield break;
            }

            var timeoutMobile = Time.time + 5f;
            yield return new WaitUntil(() =>
                (passthroughCameraManager.texture != null) ||
                Time.time > timeoutMobile);

            if (passthroughCameraManager.texture == null)
            {
                Debug.LogError("WebRTCController: Camera failed to start.");
                yield break;
            }

            if (canvasRawImage != null)
            {
                 canvasRawImage.texture = passthroughCameraManager.texture;
                 //passthroughCameraManager.StartGetImageData((texture) =>
                 //{
                 //    canvasRawImage.texture = texture;
                 //});
            }
#endif

            webRtcConnection.VideoTransmissionReceived.AddListener(OnVideoReceived);
            webRtcConnection.PromptNameUpdated.AddListener(UpdatePromptName);
            Debug.Log("WebRTCController: Initialized successfully.");
        }

        private void OnDestroy()
        {
            if (webRtcConnection == null)
            {
                return;
            }
            webRtcConnection.VideoTransmissionReceived.RemoveListener(OnVideoReceived);
            webRtcConnection.PromptNameUpdated.RemoveListener(UpdatePromptName);
        }

        private void OnVideoReceived()
        {
            _videoReceivedAndReady = true;
        }

        private void UpdatePromptName(string promptKey)
        {
            if (promptNameText != null)
            {
                promptNameText.text = string.IsNullOrEmpty(promptKey) ? "" : promptKey;
            }
        }

        //public void QueueCustomPrompt(string prompt)
        //{
        //    if (!string.IsNullOrEmpty(prompt))
        //    {
        //        _promptQueue.Enqueue(prompt);
        //    }
        //}

        // Update is called once per frame
        void Update()
        {
            if (!_videoReceivedAndReady || !webRtcConnection)
            {
                return;
            }

            HandleInput();
            //SendQueuedPrompts();
        }

        // Execute within Unity Editor
        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                Debug.Log("Press the A key on the keyboard");

                webRtcConnection.SendNextPrompt(true);
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                Debug.Log("Press the B key on the keyboard");

                webRtcConnection.SendNextPrompt(false);
            }
        }

        // Controller button event - For GameManager to call
        public void ControllerEvent(string keyName)
        {
            switch (keyName)
            {
                case "A":
                    webRtcConnection.SendNextPrompt(true);
                    break;
                case "B":
                    webRtcConnection.SendNextPrompt(false);
                    break;
            }
        }

        //private void SendQueuedPrompts()
        //{
        //    while (_promptQueue.Count > 0)
        //    {
        //        var prompt = _promptQueue.Dequeue();
        //        webRtcConnection.SendCustomPrompt(prompt);
        //    }
        //}
    }
}

