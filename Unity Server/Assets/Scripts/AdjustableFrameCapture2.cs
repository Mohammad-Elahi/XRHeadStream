using UnityEngine;
using System.IO;
using UnityEngine.Android;
using System.Collections;
public class AdjustableFrameCapture : MonoBehaviour
{
    // The number of frames to skip before capturing a frame
    [SerializeField] private int captureEveryNthFrame = 5;
    // The path where the captured frames will be saved
    [SerializeField] private string path = "/storage/emulated/0/Download/VRCapture/";

    // A reference to the OVRCameraRig component
    private OVRCameraRig cameraRig;
    // A flag to indicate whether frame capturing is currently active
    private bool isCapturing;
    // A counter for the number of frames captured
    private int frameCount;
    // A counter for the number of frames to skip
    private int framesToSkip;

    private void Start()
    {
        // Initialize the OVRCameraRig component
        InitializeCameraRig();
        // Handle permissions for writing to external storage
        HandlePermissions();
    }

    // Initialize the OVRCameraRig component
    private void InitializeCameraRig()
    {
        // Get the OVRCameraRig component attached to the same GameObject
        cameraRig = GetComponent<OVRCameraRig>();
        if (cameraRig == null)
        {
            Debug.LogError("OVRCameraRig not found in the scene.");
        }
    }

    // Handle permissions for writing to external storage
    private void HandlePermissions()
    {
        // If the app has permission to write to external storage, start capturing frames
        if (HasWritePermission())
        {
            StartCapturing();
        }
        else
        {
            // If the app does not have permission to write to external storage, request it
            RequestWritePermission();
        }
    }

    // Check if the app has permission to write to external storage
    private bool HasWritePermission()
    {
        return Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite);
    }

    // Request permission to write to external storage
    private void RequestWritePermission()
    {
        Permission.RequestUserPermission(Permission.ExternalStorageWrite);
    }

    // When the app gains focus, check if it has permission to write to external storage
    // If it does and is not currently capturing, start capturing
    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && HasWritePermission() && !isCapturing)
        {
            StartCapturing();
        }
    }

    // Start capturing frames
    private void StartCapturing()
    {
        isCapturing = true;
        // Create the directory if it doesn't exist
        Directory.CreateDirectory(path);
        // Start the coroutine to capture frames
        StartCoroutine(CaptureFrames());
    }

    // Coroutine to capture frames
    private IEnumerator CaptureFrames()
    {
        while (isCapturing)
        {
            yield return new WaitForEndOfFrame();

            // If the number of frames to skip has been reached, capture a frame
            if (++framesToSkip >= captureEveryNthFrame)
            {
                CaptureFrame();
                framesToSkip = 0;
            }
        }
    }

    // Capture a frame and save it as a PNG image
    private void CaptureFrame()
    {
        // Get the cameras for the left and right eyes
        var leftEyeCamera = cameraRig.leftEyeAnchor.GetComponent<Camera>();
        var rightEyeCamera = cameraRig.rightEyeAnchor.GetComponent<Camera>();

        // Create render textures for the left and right eye cameras
        var leftRT = new RenderTexture(Screen.width, Screen.height, 24);
        var rightRT = new RenderTexture(Screen.width, Screen.height, 24);

        // Render the frame and save it as a PNG image
        RenderAndSaveFrame(leftEyeCamera, rightEyeCamera, leftRT, rightRT);

        // Clean up the render textures
        CleanupRenderTextures(leftRT, rightRT);
    }

    // Render the frame and save it as a PNG image
    private void RenderAndSaveFrame(Camera leftEyeCamera, Camera rightEyeCamera, RenderTexture leftRT, RenderTexture rightRT)
    {
        // Set the target textures for the left and right eye cameras
        leftEyeCamera.targetTexture = leftRT;
        rightEyeCamera.targetTexture = rightRT;
        // Render the left and right eye cameras
        leftEyeCamera.Render();
        rightEyeCamera.Render();

        // Create a texture to hold the stereo shot
        var stereoShot = new Texture2D(Screen.width * 2, Screen.height, TextureFormat.RGB24, false);

        // Read the pixels from the left and right render textures into the stereo shot texture
        ReadPixelsIntoTexture(leftRT, rightRT, stereoShot);

        // Save the stereo shot texture as a PNG image
        SaveTextureAsPng(stereoShot);

        // Destroy the stereo shot texture
        Destroy(stereoShot);
    }

    // Read the pixels from the left and right render textures into the stereo shot texture
    private static void ReadPixelsIntoTexture(RenderTexture leftRT, RenderTexture rightRT, Texture2D stereoShot)
    {
        // Read the pixels from the left render texture into the left half of the stereo shot texture
        RenderTexture.active = leftRT;
        stereoShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        // Read the pixels from the right render texture into the right half of the stereo shot texture
        RenderTexture.active = rightRT;
        stereoShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), Screen.width, 0);
        // Apply the changes to the stereo shot texture
        stereoShot.Apply();
    }

    // Save the stereo shot texture as a PNG image
    private void SaveTextureAsPng(Texture2D stereoShot)
    {
        // Encode the stereo shot texture to a PNG image
        var bytes = stereoShot.EncodeToPNG();
        // Create a filename for the frame
        var filename = $"frame_{frameCount:D6}.png";
        // Combine the path and filename to get the full path
        var fullPath = Path.Combine(path, filename);
        // Write the PNG image to the file
        File.WriteAllBytes(fullPath, bytes);
        // Increment the frame count
        frameCount++;
    }

    // Clean up the render textures
    private static void CleanupRenderTextures(RenderTexture leftRT, RenderTexture rightRT)
    {
        // Reset the active render texture
        RenderTexture.active = null;
        // Destroy the left and right render textures
        Destroy(leftRT);
        Destroy(rightRT);
    }

    // When the application quits, stop capturing
    private void OnApplicationQuit()
    {
        isCapturing = false;
    }
}
