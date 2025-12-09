using System.Collections;
using UnityEngine;
using TMPro;

public class Chest : MonoBehaviour
{
    [Header("References")]
    public Animator animator;

    public Transform spawnPoint;
    public GameObject mapPrefab;
    public string playerTag = "Player";

    [Header("Prompt UI")]
    public GameObject promptPrefab;
    public Vector3 promptWorldOffset = new Vector3(0f, 1.2f, 0f);

    [Header("Win UI Controller")]
    public WinUIController winUIController; // thay cho winUIPanel

    [Header("Audio - Chest Open")]
    public AudioClip openSound;
    [Range(0f, 1f)] public float openSoundVolume = 1f;

    [Header("Audio - Win")]
    public AudioClip winSound;
    [Range(0f, 1f)] public float winSoundVolume = 1f;

    [Header("Settings")]
    public KeyCode openKey = KeyCode.E;
    public bool opened = false;

    private bool playerInRange = false;
    private GameObject promptInstance;
    private TextMeshProUGUI promptText;
    private RectTransform promptRect;
    private Canvas mainCanvas;

    void Start()
    {
        if (spawnPoint == null)
        {
            Transform sp = transform.Find("SpawnPoint");
            if (sp != null) spawnPoint = sp;
        }

        mainCanvas = FindObjectOfType<Canvas>();
    }

    void Update()
    {
        if (playerInRange && promptInstance != null)
            UpdatePromptPosition();

        if (playerInRange && !opened && Input.GetKeyDown(openKey))
            OpenChest();
    }

    void OpenChest()
    {
        opened = true;
        animator?.SetTrigger("Open");

        if (openSound != null)
            AudioSource.PlayClipAtPoint(openSound, transform.position, openSoundVolume);

        if (promptInstance != null)
            promptInstance.SetActive(false);

        StartCoroutine(SpawnMapWithDelay(1f));
    }

    private IEnumerator SpawnMapWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Spawn map sau khi mở rương
        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position + Vector3.up;
        if (mapPrefab != null)
            Instantiate(mapPrefab, pos, Quaternion.identity);

        // Đợi thêm chút cho đẹp
        yield return new WaitForSeconds(1f);

        // === HIỆN UI THẮNG ===
        if (winUIController != null)
        {
            winUIController.ShowWinUI();

            if (winSound != null)
                AudioSource.PlayClipAtPoint(winSound, transform.position, winSoundVolume);
        }

        // === BÁO LRM LÀ BOSS ĐÃ BỊ ĐÁNH BẠI ===
        if (LevelRuntimeManager.I != null)
            LevelRuntimeManager.I.OnBossDefeated();
    }

    private void CreatePromptInstance()
    {
        if (promptPrefab == null || mainCanvas == null) return;

        promptInstance = Instantiate(promptPrefab, mainCanvas.transform);
        promptText = promptInstance.GetComponentInChildren<TextMeshProUGUI>();
        promptRect = promptInstance.GetComponent<RectTransform>();

        if (promptText != null)
            promptText.text = "Open (E)";

        promptInstance.SetActive(false);
    }

    private void UpdatePromptPosition()
    {
        if (promptRect == null || Camera.main == null) return;

        Vector3 worldPos = transform.position + promptWorldOffset;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        RectTransform canvasRect = mainCanvas.transform as RectTransform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            null,
            out Vector2 anchoredPos
        );

        promptRect.anchoredPosition = anchoredPos;
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
            promptInstance.SetActive(true);
            UpdatePromptPosition();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInRange = false;

        if (promptInstance != null)
            promptInstance.SetActive(false);
    }
}
