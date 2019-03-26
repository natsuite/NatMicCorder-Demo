using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NatCorder;
using NatCorder.Clocks;
using NatCorder.Inputs;
using NatMic;
using NatCam;
using NatShareU;

public class NatMicCorder : MonoBehaviour, IAudioProcessor {

	#region --Op vars--

	[Header("Recording")]
	public int videoWidth = 1280;
    public int videoHeight = 720;

	[Header("Sharing")]
	public bool shareRecordings;

	[Header("UI")]
	public RawImage previewRawImage;
	public AspectRatioFitter previewAspectFitter;

    private MP4Recorder videoRecorder;
    private RealtimeClock recordingClock;
	private CameraInput cameraInput;

	private IAudioDevice audioDevice;
	#endregion


	#region --Recording--

	public void StartRecording () {
		var sampleRate = 44100;
		var channelCount = 2;
        // Start recording from the main camera
		recordingClock = new RealtimeClock();
        videoRecorder = new MP4Recorder(videoWidth, videoHeight, 30, sampleRate, channelCount, OnRecording);
		cameraInput = new CameraInput(videoRecorder, recordingClock, Camera.main);
        // Start the microphone
		audioDevice = AudioDevice.Devices[0];
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
        DeviceCamera.RearCamera.StartPreview(OnPreviewStart);
	}

	// Invoked by NatCam device camera once the camera preview starts
	private void OnPreviewStart (Texture preview) {
		// Display the camera preview
		previewRawImage.texture = preview;
		// Scale the panel to match aspect ratios
        previewAspectFitter.aspectRatio = preview.width / (float)preview.height;
	}

	// Invoked by NatMic audio device with new audio sample buffer
	public void OnSampleBuffer (float[] sampleBuffer, int sampleRate, int channelCount, long timestamp) {
		// Send sample buffers directly to the video recorder for recording
		videoRecorder.CommitSamples(sampleBuffer, recordingClock.Timestamp);
	}

	// Invoked by NatCorder video recorder once video recording is complete
	private void OnRecording (string path) {
		Debug.Log("Recording saved to path: "+path);
		// Share
		if (shareRecordings)
			NatShare.Share(path);
		// Playback the recording
		else {
			#if UNITY_EDITOR
			UnityEditor.EditorUtility.OpenWithDefaultApp(path);
			#elif UNITY_IOS
			Handheld.PlayFullScreenMovie("file://" + path);
			#elif UNITY_ANDROID
			Handheld.PlayFullScreenMovie(path);
			#endif
		}
	}
	#endregion
}
