using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NatCorder;
using NatCorder.Clocks;
using NatCorder.Inputs;
using NatMic;
using NatCam;
using NatShare;

public class NatMicCorder : MonoBehaviour, IAudioProcessor {

    #region --Op vars--

    [Header("Recording")]
    public int videoWidth = 1280;
    public int videoHeight = 720;

    [Header("Sharing")]
    public bool shareRecording;

    [Header("UI")]
    public RawImage rawImage;
    public AspectRatioFitter aspectFitter;

    private MP4Recorder videoRecorder;
    private RealtimeClock recordingClock;
    private CameraInput cameraInput;

    private IAudioDevice audioDevice;
    #endregion


    #region --Recording--

    public void StartRecording () {
        var sampleRate = 44100;
        var channelCount = 1;
        // Start recording from the main camera
        recordingClock = new RealtimeClock();
        videoRecorder = new MP4Recorder(videoWidth, videoHeight, 30, sampleRate, channelCount, OnRecording);
        cameraInput = new CameraInput(videoRecorder, recordingClock, Camera.main);
        // Start the microphone
        audioDevice = AudioDevice.GetDevices()[0];
        audioDevice.StartRecording(sampleRate, channelCount, this);
    }

    public void StopRecording () {
        // Stop the microphone
        audioDevice.StopRecording();
        // Stop recording
        cameraInput.Dispose();
        videoRecorder.Dispose();
        cameraInput = null;
        videoRecorder = null;
    }
    #endregion


    #region --Callbacks--

    // Invoked by Unity when the scene opens
    private void Start () {
        // Start the camera preview with NatCam
        var cameraDevice = CameraDevice.GetDevices()[0];
        cameraDevice.StartPreview(previewTexture => {
            rawImage.texture = previewTexture;
            aspectFitter.aspectRatio = previewTexture.width / (float)previewTexture.height;
        });
    }

    // Invoked by NatMic audio device with new audio sample buffer
    public void OnSampleBuffer (float[] sampleBuffer, int sampleRate, int channelCount, long timestamp) {
        // Send sample buffers directly to the video recorder for recording
        videoRecorder.CommitSamples(sampleBuffer, recordingClock.Timestamp);
    }

    // Invoked by NatCorder video recorder once video recording is complete
    private void OnRecording (string path) {
        Debug.Log($"Saved recording to: {path}");
        var prefix = Application.platform == RuntimePlatform.IPhonePlayer ? "file://" : "";
        if (shareRecording)
            using (var payload = new SharePayload())
                payload.AddMedia(path);
        else
            Handheld.PlayFullScreenMovie($"{prefix}{path}");
    }
    #endregion
}
