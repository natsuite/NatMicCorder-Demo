/* 
*   NatSuite Examples
*   Copyright (c) 2020 Yusuf Olokoba
*/

namespace NatSuite.Examples {

    using UnityEngine;
    using System.IO;
    using System.Threading.Tasks;
    using Readers;
    using Recorders;
    using Recorders.Clocks;

    public class Transcoder : MonoBehaviour {

        async void Start () {
            // Create transcoder pair
            var videoPath = GetVideoPath(@"city.mp4");
            var reader = new MP4FrameReader(videoPath);
            var recorder = new MP4Recorder(reader.frameSize.width, reader.frameSize.height, reader.frameRate);
            Debug.Log("Starting transcode");
            // Transcode in background thread
            var path = await Task.Run(() => {
                // Commit frames
                var clock = new FixedIntervalClock(30);
                for (var i = 0; i < 2; i++) // DEBUG // REMOVE
                foreach (var (pixelBuffer, timestamp) in reader.Read())
                    recorder.CommitFrame(pixelBuffer, clock.timestamp);
                // Return path
                return recorder.FinishWriting();
            });
            reader.Dispose();
            // Playback video
            Debug.Log($"Transcoded video to path: {path}");
            Handheld.PlayFullScreenMovie($"file://{path}");
        }

        public static string GetVideoPath (string videoName) {
            var path = string.Empty;
            switch (Application.platform) {
                case RuntimePlatform.Android: path = Path.Combine(Application.persistentDataPath, videoName); break;
                case RuntimePlatform.IPhonePlayer: path = Path.Combine(Application.streamingAssetsPath, videoName); break;
                case RuntimePlatform.OSXEditor: goto case RuntimePlatform.WindowsEditor;
                case RuntimePlatform.WindowsEditor: path = Path.Combine(Directory.GetCurrentDirectory(), $"Assets/StreamingAssets/{videoName}"); break;
                default: return "";
            }
            return "file://" + path;
        }
    }
}