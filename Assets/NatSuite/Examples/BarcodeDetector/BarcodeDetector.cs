/* 
*   NatSuite Examples
*   Copyright (c) 2020 Yusuf Olokoba
*/

namespace NatSuite.Examples {

    using UnityEngine;
    using UnityEngine.UI;
    using System.Threading.Tasks;
    using Devices;
    using ZXing;

    public class BarcodeDetector : MonoBehaviour {
            
        [Header(@"UI")]
        public RawImage rawImage;
        public AspectRatioFitter aspectFitter;

        private IBarcodeReader reader;
        private Texture2D previewTexture;
        private Color32[] pixelBuffer;

        async void Start () {
            // Get rear camera
            var query = new MediaDeviceQuery(MediaDeviceQuery.Criteria.RearFacing);
            var device = query.currentDevice as ICameraDevice;
            // Start the camera preview
            device.previewResolution = (640, 480);
            previewTexture = await device.StartRunning();
            pixelBuffer = previewTexture.GetPixels32();
            // Display preview
            rawImage.texture = previewTexture;
            aspectFitter.aspectRatio = (float)previewTexture.width / previewTexture.height;
            // Create barcode reader
            reader = new BarcodeReader();
        }

        async void Update () {
            // Check preview texture
            if (!previewTexture)
                return;
            // Don't detect every frame for performance
            if (Time.frameCount % 10 != 0)
                return;
            // Update pixel buffer
            var (width, height) = (previewTexture.width, previewTexture.height);
            previewTexture.GetRawTextureData<Color32>().CopyTo(pixelBuffer);
            // Detect barcodes // CHECK // ZXing seems to cause a memory leak
            var result = await Task.Run(() => reader.Decode(pixelBuffer, width, height));
            if (result != null)
                Debug.Log($"Detected {result.BarcodeFormat} barcode with text: {result.Text}");
        }
    }
}