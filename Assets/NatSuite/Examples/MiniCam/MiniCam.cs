/* 
*   NatSuite Examples
*   Copyright (c) 2020 Yusuf Olokoba.
*/

namespace NatSuite.Examples {
    
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using System.Threading.Tasks;
    using Devices;
    using static Devices.MediaDeviceQuery;

    public class MiniCam : MonoBehaviour {
        
        [Header("Camera Preview")]
        public RawImage previewPanel;
        public AspectRatioFitter previewAspectFitter;

        [Header("Photo Capture")]
        public RawImage photoPanel;
        public AspectRatioFitter photoAspectFitter;
        public Image flashIcon;
        public Image switchIcon;

        MediaDeviceQuery query;

        #region --Setup--

        async void Start () {
            // Request camera permissions
            if (!await RequestPermissions<CameraDevice>()) {
                Debug.LogError("User did not grant camera permissions");
                return;
            }
            // Create a device query for device cameras
            // Use `GenericCameraDevice` so we also capture WebCamTexture cameras
            query = new MediaDeviceQuery(Criteria.GenericCameraDevice);
            // Start camera preview
            var device = query.currentDevice as ICameraDevice;
            var previewTexture = await device.StartRunning();
            Debug.Log($"Started camera preview with resolution {previewTexture.width}x{previewTexture.height}");
            // Display preview texture
            previewPanel.texture = previewTexture;
            previewAspectFitter.aspectRatio = previewTexture.width / (float)previewTexture.height;
            // Set UI state
            switchIcon.color = query.devices.Length > 1 ? Color.white : Color.gray;
            flashIcon.color = device is CameraDevice cameraDevice && cameraDevice.flashSupported ? Color.white : Color.gray;
        }
        #endregion


        #region --UI Delegates--

        public async void CapturePhoto () {
            // Only `CameraDevice` supports capturing photos, not `ICameraDevice`
            if (query.currentDevice is CameraDevice device) {
                // Capture photo
                var photoTexture = await device.CapturePhoto();
                Debug.Log($"Captured photo with resolution {photoTexture.width}x{photoTexture.height}");
                // Display photo texture for a few seconds
                photoPanel.gameObject.SetActive(true);
                photoPanel.texture = photoTexture;
                photoAspectFitter.aspectRatio = photoTexture.width / (float)photoTexture.height;
                await Task.Delay(3_000);
                // Restore preview and destroy photo
                photoPanel.gameObject.SetActive(false);
                Texture2D.Destroy(photoTexture);
            }
        }

        public async void SwitchCamera () {
            // Check that there is another camera to switch to
            if (query.devices.Length < 2)
                return;
            // Stop current camera
            var cameraDevice = query.currentDevice as ICameraDevice;
            cameraDevice.StopRunning();
            // Advance to next available camera
            query.Advance();
            // Start new camera
            cameraDevice = query.currentDevice as ICameraDevice;
            var previewTexture = await cameraDevice.StartRunning();
            // Display preview texture
            previewPanel.texture = previewTexture;
            previewAspectFitter.aspectRatio = previewTexture.width / (float)previewTexture.height;
        }

        public void FocusCamera (BaseEventData e) {
            // Only `CameraDevice` supports setting focus point, not `ICameraDevice`
            if (query.currentDevice is CameraDevice device) {
                // Get the touch position in viewport coordinates
                var eventData = e as PointerEventData;
                RectTransform transform = eventData.pointerPress.GetComponent<RectTransform>();
                if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(transform, eventData.pressPosition, eventData.pressEventCamera, out var worldPoint))
                    return;
                var corners = new Vector3[4];
                transform.GetWorldCorners(corners);
                var point = worldPoint - corners[0];
                var size = new Vector2(corners[3].x, corners[1].y) - (Vector2)corners[0];
                // Focus camera at point
                device.focusPoint = (point.x / size.x, point.y / size.y);
            }
        }

        public void ToggleFlashMode () {
            // Only `CameraDevice` supports setting focus point, not `ICameraDevice`
            if (query.currentDevice is CameraDevice device) {
                device.flashMode = device.flashMode == FlashMode.On ? FlashMode.Off : FlashMode.On;
                flashIcon.color = device.flashMode == FlashMode.On ? Color.white : Color.gray;
            }
        }
        #endregion
    }
}