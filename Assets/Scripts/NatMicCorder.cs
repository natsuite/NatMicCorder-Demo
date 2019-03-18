using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NatCorder;
using NatCorder.Clocks;
using NatCorder.Inputs;
using NatMicU.Core;
using NatMicU.Core.Recorders;
using NatCam;
using NatShareU;

public class NatMicCorder : MonoBehaviour {

	#region --Op vars--
	public Vector2Int recordingSize;
	public bool shareRecordings;
	public RawImage previewRawImage;
	public AspectRatioFitter previewAspectFitter;
    private MP4Recorder videoRecorder;
    private IClock recordingClock;
	private CameraInput cameraInput;
	#endregion


	#region --Operations--

	private void Start () {
		// Start the camera preview with NatCam
        DeviceCamera.RearCamera.StartPreview(OnPreviewStart);
	}
	#endregion


	#region --Recording--

	public void StartRecording () {
		// Start the microphone
		var microphoneFormat = Format.Default;
		NatMic.StartRecording(Device.Default, microphoneFormat, OnSampleBuffer);
		// Start recording from the main camera
		recordingClock = new RealtimeClock();
        videoRecorder = new MP4Recorder(recordingSize.x, recordingSize.y, 30, microphoneFormat.sampleRate, microphoneFormat.channelCount, OnRecording);
		cameraInput = new CameraInput(videoRecorder, Camera.main, recordingClock);
	}

	public void StopRecording () {
		// Stop the microphone
		NatMic.StopRecording();
		// Stop recording
		cameraInput.Dispose();
		videoRecorder.Dispose();
        cameraInput = null;
        videoRecorder = null;
	}
	#endregion


	#region --Callbacks--

	// Invoked by NatCam once the camera preview starts
	private void OnPreviewStart (Texture preview) {
		// Display the camera preview
		previewRawImage.texture = preview;
		// Scale the panel to match aspect ratios
        previewAspectFitter.aspectRatio = preview.width / (float)preview.height;
	}

	// Invoked by NatMic on new microphone audio data
	private void OnSampleBuffer (float[] sampleBuffer, long timestamp) {
		// Send sample buffers directly to the video recorder for recording
		if (videoRecorder != null)
			videoRecorder.CommitSamples(sampleBuffer, recordingClock.Timestamp);
	}

	// Invoked by video recorder once video recording is complete
	private void OnRecording (string path) {
		// Share
		if (shareRecordings) 
			NatShare.ShareMedia(path);
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
