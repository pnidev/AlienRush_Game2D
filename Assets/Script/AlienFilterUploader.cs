using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class AlienFilterUploader : MonoBehaviour
{
    public IEnumerator SendImageToServer(Texture2D image)
    {
        byte[] bytes = image.EncodeToJPG();
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", bytes, "photo.jpg", "image/jpeg");

        using (UnityWebRequest www = UnityWebRequest.Post("http://127.0.0.1:5000/process", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Upload failed: " + www.error);
            }
            else
            {
                Texture2D result = new Texture2D(2, 2);
                result.LoadImage(www.downloadHandler.data);
                Debug.Log("Filter applied successfully!");
            }
        }
    }
}
