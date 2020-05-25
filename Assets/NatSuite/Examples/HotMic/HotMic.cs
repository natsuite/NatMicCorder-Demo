/* 
*   NatSuite Examples
*   Copyright (c) 2020 Yusuf Olokoba.
*/

namespace NatSuite.Examples {

    using UnityEngine;
    using Devices;
    using Recorders;

    public class HotMic : MonoBehaviour {

        IAudioDevice device;
        WAVRecorder recorder;

        async void Start () {
            // Request mic permissions
            if (!await MediaDeviceQuery.RequestPermissions<AudioDevice>()) {
                Debug.LogError("User did not grant microphone permissions");
                return;
            }
            // Get a microphone
            var deviceQuery = new MediaDeviceQuery(MediaDeviceQuery.Criteria.AudioDevice);
            device = deviceQuery.currentDevice as IAudioDevice;
            Debug.Log($"{device}");
        }

        public void StartRecording () {
            // Create a recorder
            Debug.Log($"Starting recording with format: {device.sampleRate}Hz with channel count {device.channelCount}");
            recorder = new WAVRecorder(device.sampleRate, device.channelCount);
            // Start streaming audio samples to the recorder
            device.StartRunning(recorder.CommitSamples);
        }

        public async void StopRecording () {
            // Stop recording
            device.StopRunning();
            var path = await recorder.FinishWriting();
            // Playback the recording
            Debug.Log($"Recorded audio to path: {path}");
            Handheld.PlayFullScreenMovie($"file://{path}");
        }
    }
}