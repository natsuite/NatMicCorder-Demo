using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NatCorderU.Core;
using NatCorderU.Core.Recorders;
using NatCorderU.Core.Timing;
using NatMicU.Core;
using NatMicU.Core.Recorders;
using NatCamU.Core;
using NatShareU;

public class NatMicCorder : MonoBehaviour {

	#region --Op vars--
	public bool shareRecordings;
	public Camera recordingCamera;
	public RawImage previewRawImage;
	public AspectRatioFitter previewAspectFitter;
	private CameraRecorder cameraRecorder;
	private RealtimeClock recordingClock;
	#endregion


	#region --Operations--

	private void Start () {
		// Start the camera preview with NatCam
		NatCam.Play(DeviceCamera.FrontCamera ?? DeviceCamera.RearCamera);
		NatCam.OnStart += OnPreviewStart;
	}
	#endregion


	#region --Recording--

	public void StartRecording () {
		// Start the microphone
		var microphoneFormat = Format.Default;
		NatMic.StartRecording(microphoneFormat, OnSampleBuffer);
		// Start recording
		recordingClock = new RealtimeClock();
		var audioFormat = new AudioFormat(microphoneFormat.sampleRate, microphoneFormat.channelCount);
		NatCorder.StartRecording(Container.MP4, VideoFormat.Screen, audioFormat, OnRecording);
		// Create a camera recorder for the main cam
		cameraRecorder = CameraRecorder.Create(recordingCamera, recordingClock);
	}

	public void StopRecording () {
		// Stop the microphone
		NatMic.StopRecording();
		// Stop recording
		cameraRecorder.Dispose();
		NatCorder.StopRecording();
	}
	#endregion


	#region --Callbacks--

	// Invoked by NatCam once the camera preview starts
	private void OnPreviewStart () {
		// Display the camera preview
		previewRawImage.texture = NatCam.Preview;
		// Scale the panel to match aspect ratios
        previewAspectFitter.aspectRatio = NatCam.Preview.width / (float)NatCam.Preview.height;
	}

	// Invoked by NatMic on new microphone events
	private void OnSampleBuffer (AudioEvent audioEvent, float[] sampleBuffer, long timestamp, Format format) {
		// Send sample buffers directly to NatCorder for recording
		if (audioEvent == AudioEvent.OnSampleBuffer && NatCorder.IsRecording)
			NatCorder.CommitSamples(sampleBuffer, recordingClock.CurrentTimestamp);
	}

	// Invoked by NatCorder once video recording is complete
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
