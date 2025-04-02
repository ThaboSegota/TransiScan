using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class SIRVUploader : MonoBehaviour
{
    public string authURL = "https://api.sirv.com/v2/token";
    public string uploadURL = "https://api.sirv.com/v2/files/upload";
    public string clientID = "OLOqECQ5BxHqzqq9acG48taM2mY"; 
    public string clientSecret = "QUtKGzH+kK+nrzJWPRYyJy1+pEjJ5eME0/rwiO4i03/PtoVbVS/ZzBo/vbz+0j2obBiCXrsIv8JdYLsmfrXp/w=="; 

    private string accessToken;

    void Start()
    {
        StartCoroutine(GetAccessToken());
    }

    IEnumerator GetAccessToken()
    {
        string jsonBody = $"{{\"clientId\":\"{clientID}\",\"clientSecret\":\"{clientSecret}\"}}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(authURL, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                accessToken = JsonUtility.FromJson<TokenResponse>(request.downloadHandler.text).token;
                Debug.Log("Sirv Access Token: " + accessToken);
            }
            else
            {
                Debug.LogError("Error Getting Access Token: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);
            }
        }
    }

    public void UploadImage(byte[] imageData, string fileName)
    {
        StartCoroutine(UploadImageCoroutine(imageData, fileName));
    }

    private IEnumerator UploadImageCoroutine(byte[] imageData, string fileName)
    {
        if (string.IsNullOrEmpty(accessToken))
        {
            Debug.LogError("Access Token is not available.");
            yield break;
        }

        Debug.Log("Uploading image with Access Token: " + accessToken);
        Debug.Log("Image byte length: " + imageData.Length);

        // Correct upload URL format with filename as a query parameter
        string fullUploadURL = $"{uploadURL}?filename=/{fileName}";

        using (UnityWebRequest request = new UnityWebRequest(fullUploadURL, "PUT"))
        {
            request.uploadHandler = new UploadHandlerRaw(imageData);
            request.downloadHandler = new DownloadHandlerBuffer();

            // Set headers
            request.SetRequestHeader("Authorization", "Bearer " + accessToken);
            request.SetRequestHeader("Content-Type", "image/jpeg");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Image uploaded successfully: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Error uploading image: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);
            }
        }
    }
}

[System.Serializable]
public class TokenResponse
{
    public string token;
}
