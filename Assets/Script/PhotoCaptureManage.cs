using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class PhotoCaptureManage : MonoBehaviour
{
    [Header("User Info UI")]
    public InputField inputName;
    public Dropdown inputGender;

    [Header("Buttons")]
    public Button cameraButton;   // mở webcam
    public Button captureButton;  // chụp ảnh
    public Button retakeButton;   // chụp lại
    public Button confirmButton;  // xác nhận và chuyển Scene2

    [Header("Webcam UI")]
    public RawImage cameraPreview;    // webcam live
    public RawImage capturedPreview;  // ảnh đã filter

    [Header("Settings")]
    public string filterAPI = "http://127.0.0.1:5000/process";

    private WebCamTexture webcamTexture;
    private Texture2D capturedTexture;
    private byte[] imageBytes;

    void Start()
    {
        // Ẩn tất cả preview và nút Capture/Retake
        cameraPreview.gameObject.SetActive(false);
        capturedPreview.gameObject.SetActive(false);
        captureButton.gameObject.SetActive(false);
        retakeButton.gameObject.SetActive(false);

        // CameraButton và ConfirmButton luôn hiện
        cameraButton.gameObject.SetActive(true);
        confirmButton.gameObject.SetActive(true);

        // Gán sự kiện nút
        cameraButton.onClick.AddListener(OpenCamera);
        captureButton.onClick.AddListener(CapturePhoto);
        retakeButton.onClick.AddListener(RetakePhoto);
        confirmButton.onClick.AddListener(OnConfirmButton);
    }

    // Bấm CameraButton → bật webcam
    void OpenCamera()
    {
        if (string.IsNullOrEmpty(inputName.text))
        {
            Debug.LogWarning("Chưa nhập tên!");
            return;
        }

        // Hiển thị webcam
        cameraPreview.gameObject.SetActive(true);
        capturedPreview.gameObject.SetActive(false);

        if (webcamTexture == null)
        {
            var devices = WebCamTexture.devices;
            if (devices.Length == 0)
            {
                Debug.LogError("Không tìm thấy camera!");
                return;
            }
            webcamTexture = new WebCamTexture(devices[0].name, 1280, 720);
            cameraPreview.texture = webcamTexture;
            webcamTexture.Play();
        }
        else
        {
            webcamTexture.Play();
        }

        // Nút Capture hiện, Retake ẩn
        captureButton.gameObject.SetActive(true);
        retakeButton.gameObject.SetActive(false);
    }

    public void CapturePhoto()
    {
        if (webcamTexture == null || !webcamTexture.isPlaying)
        {
            Debug.LogError("Webcam chưa khởi động!");
            return;
        }

        // Chụp ảnh từ webcam
        capturedTexture = new Texture2D(webcamTexture.width, webcamTexture.height);
        capturedTexture.SetPixels(webcamTexture.GetPixels());
        capturedTexture.Apply();
        imageBytes = capturedTexture.EncodeToPNG();

        // Gửi ảnh lên server filter
        StartCoroutine(SendToFilterAPI(imageBytes));
    }

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

            var filteredBytes = www.downloadHandler.data;
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(filteredBytes);
            capturedPreview.texture = tex;
            capturedPreview.gameObject.SetActive(true);

            // ẩn camera live và dừng webcam
            if (webcamTexture != null && webcamTexture.isPlaying)
                webcamTexture.Stop();
            cameraPreview.gameObject.SetActive(false);

            // Nút Retake hiện, Capture ẩn
            retakeButton.gameObject.SetActive(true);
            captureButton.gameObject.SetActive(false);

            // Cập nhật imageBytes
            imageBytes = filteredBytes;
        }
    }

    void RetakePhoto()
    {
        // ẩn ảnh đã chụp
        capturedPreview.gameObject.SetActive(false);

        // bật lại webcam
        cameraPreview.gameObject.SetActive(true);
        if (webcamTexture != null)
            webcamTexture.Play();

        // Nút Capture hiện, Retake ẩn
        captureButton.gameObject.SetActive(true);
        retakeButton.gameObject.SetActive(false);
    }

    void OnConfirmButton()
    {
        if (string.IsNullOrEmpty(inputName.text))
        {
            Debug.LogWarning("Chưa nhập tên!");
            return;
        }

        // Dừng webcam nếu đang chạy
        if (webcamTexture != null && webcamTexture.isPlaying)
            webcamTexture.Stop();

        // Lưu dữ liệu vào PlayerData
        PlayerData.playerName = inputName.text;
        PlayerData.playerGender = inputGender.options[inputGender.value].text;
        if (imageBytes != null)
            PlayerData.playerPhoto = imageBytes;

        // Chuyển Scene2
        SceneManager.LoadScene("Scene2");
    }
}
