/* 
*   NatSuite Examples
*   Copyright (c) 2020 Yusuf Olokoba
*/

namespace NatSuite.Examples {

    using UnityEngine;
    using UnityEngine.UI;
    using Devices;
    using ZXing;

    public class BarcodeDetector : MonoBehaviour {
            
        [Header(@"UI")]
        public RawImage rawImage;
        public AspectRatioFitter aspectFitter;

        private IBarcodeReader reader;
        private Texture2D previewTexture;

        async void Start () {
            // Create barcode reader
            reader = new BarcodeReader();
            // Get rear camera
            var query = new MediaDeviceQuery(MediaDeviceQuery.Criteria.RearFacing);
            var device = query.currentDevice as ICameraDevice;
            // Start the camera preview
            device.previewResolution = (1280, 720);
            previewTexture = await device.StartRunning();
            // Display preview
            rawImage.texture = previewTexture;
            aspectFitter.aspectRatio = (float)previewTexture.width / previewTexture.height;
        }

        void Update () {
            if (!previewTexture)
                return;
            // Detect barcodes
            var result = reader.Decode(previewTexture.GetPixels32(), previewTexture.width, previewTexture.height);
            if (result != null)
                Debug.Log($"Detected {result.BarcodeFormat} barcode with text: {result.Text}");
        }
    }
}