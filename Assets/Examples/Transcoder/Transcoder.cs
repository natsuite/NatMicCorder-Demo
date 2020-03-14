/* 
*   NatSuite Examples
*   Copyright (c) 2020 Yusuf Olokoba
*/

namespace NatSuite.Examples {

    using UnityEngine;
    using Readers;
    using Recorders;
    using Recorders.Clocks;

    public class Transcoder : MonoBehaviour {

        async void Start () {
            // Create a video reader
            var videoPath = ""; // INCOMPLETE
            using (var reader = new MP4FrameReader(videoPath)) {
                // Create a recorder
                var recorder = new MP4Recorder(reader.frameSize.width, reader.frameSize.height, reader.frameRate);
                // Commit frames
                foreach (var (pixelBuffer, timestamp) in reader)
                    recorder.CommitFrame(pixelBuffer, timestamp);
                // Finish writing
                var transcodedVideoPath = await recorder.FinishWriting();
                Debug.Log($"Transcoded video to path: {transcodedVideoPath}");
            }
        }
    }
}