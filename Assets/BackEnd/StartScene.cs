using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public GameObject continueButton;
    public GameObject characterObject;

    [Header("Audio Settings")]
    public AudioSource audioSource; // Cái loa
    public AudioClip typingSound;   // Tiếng gõ chữ
    public AudioClip openDialogSound; // Tiếng khi hiện khung hội thoại
    public bool playTypingSoundEveryChar = true; // Phát âm thanh mỗi ký tự
    public int typingSoundInterval = 1; // Phát âm thanh mỗi X ký tự (1 = mỗi chữ, 2 = cách 1 chữ, 3 = cách 2 chữ)

    [Header("Dialogue Data")]
    private string[] sentences;
    private int sentenceIndex = 0;
    private Coroutine currentTypingCoroutine; // Lưu coroutine hiện tại
    private bool isDialogueComplete = false; // Cờ kiểm tra hội thoại đã hoàn thành

    [Header("Typing Settings")]
    public float typingSpeed = 0.005f;
    public float delayBeforeSecondSentence = 1.0f; // Thời gian chờ trước khi hiện câu thứ 2
    
    private readonly string sentence1 = "Trong vai đặc vụ ưu tú của LAXA, hãy điều khiển đĩa bay thám hiểm dọc ba miền Việt Nam, thu thập items bonus và sử dụng chúng vô hiệu hóa mọi mối đe dọa để hoàn thành sứ mệnh nghiên cứu Trái Đất.";
    private readonly string sentence2 = "Nhưng trước tiên, phải định danh thông tin đã, hãy bấm tam giác quỷ màu xanh để bắt đầu !!!";
    
    private string[] welcomeDialogue;


    void Start()
    {
        // Khởi tạo mảng dialogue từ các biến string
        welcomeDialogue = new string[] { sentence1, sentence2 };
        
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (characterObject != null) characterObject.SetActive(false);
        if (continueButton != null) continueButton.SetActive(false);
        StartCoroutine(StagingIntroSequence());
    }

    IEnumerator StagingIntroSequence()
    {
        if (continueButton != null) continueButton.SetActive(false);

        // 1. Chờ và hiện Character
        yield return new WaitForSeconds(0.5f);
        if (characterObject != null) characterObject.SetActive(true);

        // 2. Chờ tiếp để hiện Dialogue System
        yield return new WaitForSeconds(0.8f);

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        // 3. Bắt đầu chạy chữ
        StartDialogue(welcomeDialogue);
    }

    public void StartDialogue(string[] dialogue)
    {
        sentences = dialogue;
        sentenceIndex = 0;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentenceIndex >= sentences.Length)
        {
            EndDialogue();
            return;
        }
        // Kiểm tra xem có phải câu cuối không (trước khi tăng index)
        bool isLastSentence = (sentenceIndex == sentences.Length - 1);
        StartCoroutine(TypeSentence(sentences[sentenceIndex], isLastSentence));
        sentenceIndex++; // Tăng index sau khi đã kiểm tra
    }

    IEnumerator TypeSentence(string sentence, bool isLastSentence)
    {
        // Vô hiệu hóa nút khi đang chạy chữ
        if (continueButton != null) 
        {
            continueButton.SetActive(false);
            var btn = continueButton.GetComponent<UnityEngine.UI.Button>();
            if (btn != null) btn.interactable = false;
        }
        
        dialogueText.text = "";

        // Phát âm thanh mở dialog chỉ cho câu đầu tiên
        if (sentenceIndex == 1 && audioSource != null && openDialogSound != null)
        {
            audioSource.PlayOneShot(openDialogSound);
        }

        int charCount = 0;
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            
            // Chỉ phát âm thanh theo interval để tránh đè lên nhau
            if (playTypingSoundEveryChar && audioSource != null && typingSound != null)
            {
                if (charCount % typingSoundInterval == 0 && !char.IsWhiteSpace(letter))
                {
                    // Dừng âm thanh cũ nếu đang phát (tránh overlap)
                    if (audioSource.isPlaying)
                    {
                        audioSource.Stop();
                    }
                    audioSource.PlayOneShot(typingSound);
                }
            }
            
            charCount++;
            yield return new WaitForSeconds(typingSpeed);
        }

        // Dừng âm thanh ngay sau khi gõ xong
        if (audioSource != null)
        {
            audioSource.Stop();
        }

        // Chỉ tự động chuyển nếu KHÔNG phải câu cuối
        if (!isLastSentence)
        {
            // Câu thứ 2 sẽ bắt đầu sau delayBeforeSecondSentence giây
            yield return new WaitForSeconds(delayBeforeSecondSentence);
            DisplayNextSentence();
        }
        else
        {
            // Nếu là câu cuối, hiện nút và cho phép bấm để chuyển scene
            isDialogueComplete = true;
            if (continueButton != null) 
            {
                continueButton.SetActive(true);
                var btn = continueButton.GetComponent<UnityEngine.UI.Button>();
                if (btn != null) btn.interactable = true;
            }
        }
    }

    void EndDialogue()
    {
        // Không ẩn dialogue panel nữa, để nó hiển thị
        // Player sẽ bấm continue để chuyển scene hoặc làm gì đó khác
        Debug.Log("Hội thoại kết thúc. Chờ player bấm nút continue.");
    }

    // Hàm gọi khi bấm nút Continue
    public void OnContinueButtonClick()
    {
        // Chỉ cho phép chuyển scene khi hội thoại đã hoàn thành
        if (isDialogueComplete)
        {
            Debug.Log("Chuyển sang Scene1...");
            SceneManager.LoadScene("Scene1");
        }
        else
        {
            Debug.LogWarning("Hội thoại chưa kết thúc!");
        }
    }
}
