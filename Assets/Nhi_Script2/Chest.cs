using System.Collections;
using UnityEngine;
using TMPro;

public class Chest : MonoBehaviour
{
    [Header("References")]
    public Animator animator;

    // Spawn map – CHỈ dùng cho spawn map, không liên quan UI prompt
    public Transform spawnPoint;
    public GameObject mapPrefab;
    public string playerTag = "Player";

    [Header("Prompt UI")]
    public GameObject promptPrefab;
    // Offset UI so với tâm Chest (chỉnh trong Inspector cho vừa mắt)
    public Vector3 promptWorldOffset = new Vector3(0f, 1.2f, 0f);

    [Header("Win UI")]
    [Tooltip("Panel hoặc UI GameObject 'Bạn đã thắng', đặt sẵn trong Canvas và tắt đi.")]
    public GameObject winUIPanel;
    [Tooltip("Thời gian chờ (giây) sau khi map spawn rồi mới hiện UI thắng.")]
    public float winDelayAfterMap = 2f;

    [Header("Audio - Chest Open")]
    public AudioClip openSound;
    [Range(0f, 1f)] public float openSoundVolume = 1f;

    [Header("Audio - Win")]
    public AudioClip winSound;
    [Range(0f, 1f)] public float winSoundVolume = 1f;

    [Header("Settings")]
    public KeyCode openKey = KeyCode.E;
    public bool opened = false;

    // Runtime
    private bool playerInRange = false;
    private GameObject promptInstance;
    private TextMeshProUGUI promptText;
    private RectTransform promptRect;
    private Canvas mainCanvas;   // Cache Canvas để dùng nhanh

    void Start()
    {
        // SpawnPoint chỉ phục vụ cho spawn map
        if (spawnPoint == null)
        {
            Transform sp = transform.Find("SpawnPoint");
            if (sp != null) spawnPoint = sp;
        }

        // Cache Canvas ngay từ đầu (chỉ tìm 1 lần)
        mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogError("Không tìm thấy Canvas trong scene! Tạo một Canvas (Screen Space - Overlay) đi!");
        }

        // Đảm bảo UI thắng đang tắt lúc bắt đầu
        if (winUIPanel != null)
        {
            winUIPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (playerInRange && promptInstance != null)
            UpdatePromptPosition();   // UI luôn bám theo Chest

        if (playerInRange && !opened && Input.GetKeyDown(openKey))
            OpenChest();
    }

    private void CreatePromptInstance()
    {
        if (promptPrefab == null)
        {
            Debug.LogWarning($"[{name}] promptPrefab chưa gán!");
            return;
        }
        if (mainCanvas == null) return;

        // Xóa prompt cũ nếu có (tránh bị sót lại từ lần test trước)
        if (promptInstance != null) Destroy(promptInstance);

        // Luôn spawn UI dưới Canvas
        promptInstance = Instantiate(promptPrefab, mainCanvas.transform);
        promptInstance.name = $"ChestPrompt_{name}";

        promptText = promptInstance.GetComponentInChildren<TextMeshProUGUI>();
        promptRect = promptInstance.GetComponent<RectTransform>();

        // Có thể bỏ nếu bạn đã chỉnh trong prefab
        if (promptRect != null)
            promptRect.sizeDelta = new Vector2(160, 40);
        if (promptText != null)
            promptText.fontSize = 20;

        promptInstance.SetActive(false);
    }

    // === HÀM QUAN TRỌNG: tính vị trí UI từ vị trí Chest ===
    private void UpdatePromptPosition()
    {
        if (promptRect == null || mainCanvas == null || Camera.main == null)
            return;

        // 1) Vị trí world: tâm Chest + offset
        Vector3 worldPos = transform.position + promptWorldOffset;

        // 2) Đổi sang screen point
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        // 3) Đổi sang anchoredPosition trong Canvas
        RectTransform canvasRect = mainCanvas.transform as RectTransform;
        Vector2 anchoredPos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
            out anchoredPos
        );

        // 4) Gán vào anchoredPosition của UI
        promptRect.anchoredPosition = anchoredPos;
    }

    void OpenChest()
    {
        opened = true;
        animator?.SetTrigger("Open");

        if (promptInstance != null)
            promptInstance.SetActive(false);

        // Sound mở rương
        if (openSound != null)
            AudioSource.PlayClipAtPoint(openSound, transform.position, openSoundVolume);

        // Delay 1 giây rồi spawn map
        StartCoroutine(SpawnMapWithDelay(1f));
    }

    private IEnumerator SpawnMapWithDelay(float delay)
    {
        // 1) Chờ trước khi spawn map (ví dụ cho animation rương mở xong)
        yield return new WaitForSeconds(delay);

        // 2) Spawn map
        Vector3 pos = spawnPoint != null
            ? spawnPoint.position
            : transform.position + Vector3.up * 0.5f;

        if (mapPrefab != null)
            Instantiate(mapPrefab, pos, Quaternion.identity);

        // 3) Nếu có UI thắng: chờ thêm winDelayAfterMap rồi hiện + chơi sound thắng
        if (winUIPanel != null)
        {
            yield return new WaitForSeconds(winDelayAfterMap);

            winUIPanel.SetActive(true);

            // Play sound thắng
            if (winSound != null)
                AudioSource.PlayClipAtPoint(winSound, transform.position, winSoundVolume);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag) || opened)
            return;

        playerInRange = true;

        if (promptInstance == null)
            CreatePromptInstance();

        if (promptInstance != null)
        {
            if (promptText != null)
                promptText.text = "Open (E)";

            promptInstance.SetActive(true);
            UpdatePromptPosition(); // Cập nhật vị trí ngay lập tức
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInRange = false;

        if (promptInstance != null)
            promptInstance.SetActive(false);
    }

    void OnDestroy()
    {
        if (promptInstance != null)
            Destroy(promptInstance);
    }
}
