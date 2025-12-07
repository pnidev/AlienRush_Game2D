using System.Collections;
using UnityEngine;
using TMPro;

public class TrapChest : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public string playerTag = "Player";

    [Header("Prompt UI")]
    public GameObject promptPrefab;
    // Offset UI so với tâm Chest (chỉnh trong Inspector cho vừa mắt)
    public Vector3 promptWorldOffset = new Vector3(0f, 1.2f, 0f);

    [Header("Trap Effect")]
    [Tooltip("Particle system hoặc prefab effect bẫy (ví dụ khói, nổ, poison...).")]
    public GameObject trapEffectPrefab;
    [Tooltip("Thời gian tồn tại effect bẫy (giây).")]
    public float trapEffectDuration = 6f;

    [Header("Enemy Spawn")]
    [Tooltip("Các prefab enemy sẽ spawn ra từ bẫy.")]
    public GameObject[] enemyPrefabs;
    [Tooltip("Các điểm spawn enemy (tạo empty object quanh chest và gán vào).")]
    public Transform[] enemySpawnPoints;
    [Tooltip("Delay (giây) sau khi bẫy kích hoạt rồi mới cho enemy xuất hiện.")]
    public float enemySpawnDelay = 4f;

    [Header("Audio")]
    [Tooltip("Sound khi chest bẫy mở / bẫy kích hoạt.")]
    public AudioClip trapSound;
    [Range(0f, 1f)] public float trapSoundVolume = 1f;

    [Header("Settings")]
    public KeyCode openKey = KeyCode.E;
    public bool opened = false;   // để bẫy chỉ kích hoạt 1 lần

    // Runtime
    private bool playerInRange = false;
    private GameObject promptInstance;
    private TextMeshProUGUI promptText;
    private RectTransform promptRect;
    private Canvas mainCanvas;   // Cache Canvas để dùng nhanh

    void Start()
    {
        // Cache Canvas ngay từ đầu (chỉ tìm 1 lần)
        mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogError("TrapChest: Không tìm thấy Canvas trong scene! Tạo một Canvas (Screen Space - Overlay) đi!");
        }
    }

    void Update()
    {
        if (playerInRange && promptInstance != null)
            UpdatePromptPosition();   // UI luôn bám theo Chest

        if (playerInRange && !opened && Input.GetKeyDown(openKey))
            ActivateTrap();
    }

    private void CreatePromptInstance()
    {
        if (promptPrefab == null)
        {
            Debug.LogWarning($"TrapChest [{name}] promptPrefab chưa gán!");
            return;
        }
        if (mainCanvas == null) return;

        // Xóa prompt cũ nếu có (tránh bị sót lại từ lần test trước)
        if (promptInstance != null) Destroy(promptInstance);

        // Luôn spawn UI dưới Canvas
        promptInstance = Instantiate(promptPrefab, mainCanvas.transform);
        promptInstance.name = $"TrapChestPrompt_{name}";

        promptText = promptInstance.GetComponentInChildren<TextMeshProUGUI>();
        promptRect = promptInstance.GetComponent<RectTransform>();

        // Có thể bỏ nếu bạn đã chỉnh trong prefab
        if (promptRect != null)
            promptRect.sizeDelta = new Vector2(160, 40);
        if (promptText != null)
            promptText.fontSize = 20;

        promptInstance.SetActive(false);
    }

    // === HÀM TÍNH VỊ TRÍ UI TỪ VỊ TRÍ CHEST ===
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

    // === HÀM KÍCH HOẠT BẪY KHI NHẤN E ===
    void ActivateTrap()
    {
        opened = true;

        // Animation mở chest (nếu có)
        animator?.SetTrigger("Open");

        // Tắt prompt "Open (E)"
        if (promptInstance != null)
            promptInstance.SetActive(false);

        // Sound bẫy
        if (trapSound != null)
            PlaySoundAtPosition(trapSound, transform.position, 1f, true);


        // Spawn effect bẫy
        if (trapEffectPrefab != null)
        {
            GameObject effect = Instantiate(trapEffectPrefab, transform.position, Quaternion.identity);
            if (trapEffectDuration > 0f)
                Destroy(effect, trapEffectDuration);   // effect tự mất sau X giây
        }

        // Bắt đầu sequence spawn enemy
        StartCoroutine(SpawnEnemiesAfterDelay());
    }
    // ==========================
    // HÀM PLAY SOUND CHUẨN KHÔNG BỊ NHỎ
    // ==========================
    private void PlaySoundAtPosition(AudioClip clip, Vector3 pos, float volume = 1f, bool force2D = true)
    {
        if (clip == null) return;

        GameObject go = new GameObject("OneShotAudio");
        go.transform.position = pos;

        AudioSource src = go.AddComponent<AudioSource>();
        src.clip = clip;
        src.playOnAwake = false;

        // Nếu muốn không giảm âm theo khoảng cách => 2D
        src.spatialBlend = force2D ? 0f : 1f;

        // Volume
        src.volume = Mathf.Clamp01(volume);

        // Nếu bạn muốn dùng AudioMixer, gán thêm dòng này:
        // src.outputAudioMixerGroup = sfxMixerGroup;

        // Nếu chơi ở mode 3D, chỉnh minDistance cho to hơn
        if (!force2D)
        {
            src.minDistance = 1f;
            src.maxDistance = 20f;
            src.rolloffMode = AudioRolloffMode.Linear;
        }

        src.Play();
        Destroy(go, clip.length + 0.25f);
    }

    private IEnumerator SpawnEnemiesAfterDelay()
    {
        // Chờ 1.5s (hoặc enemySpawnDelay bạn set)
        yield return new WaitForSeconds(enemySpawnDelay);

        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning($"TrapChest [{name}] không có enemyPrefabs để spawn!");
            yield break;
        }

        if (enemySpawnPoints == null || enemySpawnPoints.Length == 0)
        {
            Debug.LogWarning($"TrapChest [{name}] không có enemySpawnPoints! Sẽ spawn ngay tại vị trí chest.");
            // Trường hợp không set spawn point, spawn tất cả enemy ngay tại chest
            foreach (var enemyPrefab in enemyPrefabs)
            {
                if (enemyPrefab == null) continue;
                Instantiate(enemyPrefab, transform.position, Quaternion.identity);
            }
            yield break;
        }

        // Trường hợp có cả enemyPrefabs và spawnPoints:
        // Cách đơn giản: mỗi spawnPoint spawn RANDOM 1 enemy trong list
        foreach (Transform spawnPoint in enemySpawnPoints)
        {
            if (spawnPoint == null) continue;

            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            if (enemyPrefab == null) continue;

            Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        }

        // Sau khi spawn enemy xong, bạn có thể:
        // - Destroy chest
        // - Hoặc tắt collider để player không bị trigger lại
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;
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
