/* 
*   NatSuite Demo
*   Copyright (c) 2020 Yusuf Olokoba.
*/

namespace NatSuite {

    using UnityEngine;
    using UnityEngine.UI;
    using System.Linq;
    using NatCorder;
    using NatCorder.Clocks;
    using NatCorder.Inputs;
    using NatDevice;
    using NatShare;
    
    public class NatSuiteDemo : MonoBehaviour {

        #region --Op vars--

        [Header("UI")]
        public RawImage rawImage;
        public AspectRatioFitter aspectFitter;

        [Header("Sharing")]
        public bool shareRecording;

        private IAudioDevice audioDevice;
        private MP4Recorder recorder;
        private CameraInput cameraInput;
        #endregion

        async void Start () { // Invoked by Unity when the scene opens
            // Request permissions
            if (!await MediaDeviceQuery.RequestPermissions<IAudioDevice>()) {
                Debug.LogError("User did not grant microphone permission");
                return;
            }
            if (!await MediaDeviceQuery.RequestPermissions<ICameraDevice>()) {
                Debug.LogError("User did not grant camera permission");
                return;
            }
            // Get a microphone and camera
            var query = new MediaDeviceQuery();
            audioDevice = query.devices.FirstOrDefault(device => device is IAudioDevice) as IAudioDevice;
            var cameraDevice = query.devices.FirstOrDefault(device => device is ICameraDevice) as ICameraDevice;
            // Start the camera preview
            var previewTexture = await cameraDevice.StartRunning();
            rawImage.texture = previewTexture;
            aspectFitter.aspectRatio = previewTexture.width / (float)previewTexture.height;
        }

        public void StartRecording () {
            // Create a recorder
            recorder = new MP4Recorder(720, 1280, 30, audioDevice.sampleRate, audioDevice.channelCount);
            // Start recording
            var clock = new RealtimeClock();
            cameraInput = new CameraInput(recorder, clock, Camera.main);
            audioDevice.StartRunning((sampleBuffer, timestamp) => recorder.CommitSamples(sampleBuffer, clock.timestamp));
        }

        public async void StopRecording () {
            // Stop the recorder inputs
            audioDevice.StopRunning();
            cameraInput.Dispose();
            // Stop recording and get the path
            var path = await recorder.FinishWriting();
            Debug.Log($"Saved recording to: {path}");
            // Share the recording
            if (shareRecording) {
                var payload = new SharePayload();
                payload.AddMedia(path);
                await payload.Commit();
            }
            // Playback the recording
            if (Application.platform == RuntimePlatform.IPhonePlayer)
                path = "file://" + path;
            Handheld.PlayFullScreenMovie(path);
        }
    }
}
