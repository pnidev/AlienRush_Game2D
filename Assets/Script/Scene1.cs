using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PhotoCaptureManager : MonoBehaviour
{
    [Header("User Info UI")]
    public InputField inputName;
    public Dropdown inputGender;

    [Header("Buttons")]
    public Button cameraButton;   // mở webcam
    public Button confirmButton;  // xác nhận tên, giới tính

    [Header("Panels")]
    public GameObject panelInfo;      // panel nhập tên + nút Camera / Confirm
    public GameObject panelCamera;    // panel webcam + RawImage + Capture/Retake/Cancel

    [Header("Webcam UI")]
    public RawImage cameraPreview;    
    public RawImage capturedPreview;  

    [Header("Settings")]
    public string saveFolder = @"D:\Unity\Project\My project\Assets\Sprite\captured_face";
    public string filterAPI = "http://127.0.0.1:5000/process";

    private WebCamTexture webcamTexture;
    private Texture2D capturedTexture;
    private byte[] imageBytes;

    void Start()
    {
        panelInfo.SetActive(true);
        panelCamera.SetActive(false);
        capturedPreview.gameObject.SetActive(false);

        cameraButton.onClick.AddListener(OpenCameraPanel);
        confirmButton.onClick.AddListener(ConfirmInfo);
    }

    // Khi bấm CameraButton
    void OpenCameraPanel()
    {
        if (string.IsNullOrEmpty(inputName.text))
        {
            Debug.LogWarning("Chưa nhập tên!");
            return;
        }

        panelInfo.SetActive(false);
        panelCamera.SetActive(true);

        StartWebcam();
    }

    void StartWebcam()
    {
        if (webcamTexture == null)
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            if (devices.Length == 0)
            {
                Debug.LogError("Không tìm thấy camera!");
                return;
            }

            webcamTexture = new WebCamTexture(devices[0].name, 1280, 720);
            cameraPreview.texture = webcamTexture;
            cameraPreview.material.mainTexture = webcamTexture;
            webcamTexture.Play();
        }
        else
        {
            webcamTexture.Play();
        }
    }

    // Chụp ảnh từ webcam
    public void CapturePhoto()
    {
        if (webcamTexture == null || !webcamTexture.isPlaying)
        {
            Debug.LogError("Webcam chưa khởi động!");
            return;
        }

        capturedTexture = new Texture2D(webcamTexture.width, webcamTexture.height);
        capturedTexture.SetPixels(webcamTexture.GetPixels());
        capturedTexture.Apply();
        imageBytes = capturedTexture.EncodeToPNG();

        StartCoroutine(SendToFilterAPI(imageBytes));
    }

    // Gửi ảnh lên API filter
    IEnumerator SendToFilterAPI(byte[] photoBytes)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", photoBytes, "photo.png", "image/png");

        using (UnityWebRequest www = UnityWebRequest.Post(filterAPI, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("API lỗi: " + www.error);
                yield break;
            }

            byte[] filteredBytes = www.downloadHandler.data;
            Texture2D filteredTexture = new Texture2D(2, 2);
            filteredTexture.LoadImage(filteredBytes);

            capturedPreview.texture = filteredTexture;
            capturedPreview.gameObject.SetActive(true);
            imageBytes = filteredBytes;
        }
    }

    // Khi bấm ConfirmButton
    void ConfirmInfo()
    {
        if (string.IsNullOrEmpty(inputName.text))
        {
            Debug.LogWarning("Chưa nhập tên!");
            return;
        }

        // Lưu dữ liệu vào PlayerData
        PlayerData.playerName = inputName.text;
        PlayerData.playerGender = inputGender.options[inputGender.value].text;
        if (imageBytes != null)
            PlayerData.playerPhoto = imageBytes;

        // Load Scene2
        SceneManager.LoadScene("Scene2"); // đổi tên scene nếu cần
    }
}
