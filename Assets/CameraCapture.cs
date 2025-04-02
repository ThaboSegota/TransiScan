using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CameraCapture : MonoBehaviour
{
    public GameObject sirvManager; // Assign SIRVUploader in Unity
    public RawImage displayImage;  // Assign UI RawImage to show camera preview

    private WebCamTexture webcamTexture;

    void Start()
    {
        StartCoroutine(InitializeCamera());
    }

    private IEnumerator InitializeCamera()
    {
        // Request camera permission explicitly on Android
        if (Application.platform == RuntimePlatform.Android)
        {
            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
            if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                Debug.LogError("Camera permission denied!");
                yield break;
            }
        }

        // Check available cameras
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            Debug.LogError("No camera found!");
            yield break;
        }

        // Start camera preview
        webcamTexture = new WebCamTexture(devices[0].name);
        displayImage.texture = webcamTexture;
        displayImage.gameObject.SetActive(true);
        webcamTexture.Play();
    }

    public void CaptureAndUpload()
    {
        StartCoroutine(TakePhoto());
    }

    private IEnumerator TakePhoto()
    {
        yield return new WaitForEndOfFrame();

        if (webcamTexture == null || !webcamTexture.isPlaying)
        {
            Debug.LogError("Camera is not active!");
            yield break;
        }

        // Capture and encode the image
        Texture2D photo = new Texture2D(webcamTexture.width, webcamTexture.height);
        photo.SetPixels(webcamTexture.GetPixels());
        photo.Apply();

        byte[] imageBytes = photo.EncodeToJPG();

        // Log the byte length to ensure it's not empty
        Debug.Log("Captured image byte length: " + imageBytes.Length);

        // Send image data to SIRVUploader
        sirvManager.GetComponent<SIRVUploader>().UploadImage(imageBytes, "myphoto.jpg");
    }
}
