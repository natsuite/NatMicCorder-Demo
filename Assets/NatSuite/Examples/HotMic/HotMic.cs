/* 
*   NatSuite Examples
*   Copyright (c) 2020 Yusuf Olokoba.
*/

namespace NatSuite.Examples {

    using UnityEngine;
    using Components;
    using Devices;

    public class HotMic : MonoBehaviour {

        IAudioDevice device;
        ClipRecorder recorder;

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
            recorder = new ClipRecorder(device.sampleRate, device.channelCount);
            // Start streaming audio samples to the recorder
            device.StartRunning(recorder.CommitSamples);
        }

        public void StopRecording () {
            // Stop recording
            device.StopRunning();
            var audioClip = recorder.FinishWriting();
            // Playback the recording
            AudioSource.PlayClipAtPoint(audioClip, Vector3.zero);
        }
    }
}