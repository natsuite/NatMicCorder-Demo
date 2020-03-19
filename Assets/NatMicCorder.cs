using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NatCorder;
using NatCorder.Clocks;
using NatCorder.Inputs;
using NatMic; using NatShare;
// 20200319
public class NatMicCorder : MonoBehaviour, IAudioProcessor
{

    #region --Op vars--

    [Header("Recording")]
    public int videoWidth = 1280;
    public int videoHeight = 720;

    private CameraInput cameraInput;
    private MP4Recorder videoRecorder;
    private RealtimeClock recordingClock; 

    private IAudioDevice audioDevice;
    #endregion
 

    #region --Recording--

    public void StartRecording()
    {
        var sampleRate = 44100;
        var channelCount = 1;
        // Start recording from the main camera
        recordingClock = new RealtimeClock();
        videoWidth = Screen.width;
        videoHeight = Screen.height;
        videoRecorder = new MP4Recorder(videoWidth, videoHeight, 30, sampleRate, channelCount);
        cameraInput = new CameraInput(videoRecorder, recordingClock, Camera.main);
        // Start the microphone
        audioDevice = AudioDevice.GetDevices()[0];
        audioDevice.StartRecording(sampleRate, channelCount, this);
    }

    public async void StopRecording()
    {
        // Stop the microphone
        audioDevice.StopRecording();
        cameraInput.Dispose();
        // Stop recording 
        var path = await videoRecorder.FinishWriting();
 
        using (var payload = new SavePayload())
        {
            payload.AddMedia(path);
        }

        videoRecorder = null;
        cameraInput = null;
    }
    #endregion


    #region --Callbacks--
 

    // Invoked by NatMic audio device with new audio sample buffer
    public void OnSampleBuffer(float[] sampleBuffer, int sampleRate, int channelCount, long timestamp)
    {
        // Send sample buffers directly to the video recorder for recording
        videoRecorder.CommitSamples(sampleBuffer, recordingClock.timestamp);


    }
 

    #endregion
}
