using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement2D : MonoBehaviour
{
    public UIManager uiManager;   // quản lý UI tim

    [Header("Movement")]
    public float moveSpeed = 3f;               // giá trị khởi tạo từ Inspector
    public float laneSwitchSpeed = 5f;
    public float dashSpeedMutiplier = 3f;
    public float dashDuration = 0.5f;
    public float maxSpeedCap = 12f;            // giới hạn tốc độ tuyệt đối

    private Rigidbody2D rb;
    private bool isDashing = false;

    // ---- speed system (không thay trực tiếp moveSpeed) ----
    private float baseMoveSpeed;                       // lưu giá trị gốc từ Inspector
    private List<float> activeSpeedMultipliers = new List<float>(); // stack các multiplier từ buff

    [Header("Lanes")]
    public float topLaneY = 1.7f;    // giá trị mặc định hợp lý cho world scale của bạn
    public float lowerLaneY = -1.7f;
    protected float targetYLane;

    // ---------- PLAYER STATS ----------
    public int health = 3;
    public int maxHealth = 3;
    [HideInInspector]
    public float localScore = 0f;     // giữ cho tương thích dự án cũ

    //public int coins = 100;

    public bool IsDashing => isDashing;

    // ---------- INVINCIBLE ----------
    public float invincibilityDuration = 1.5f;
    public float blinkInterval = 0.1f;
    private bool isInvincible = false;
    private SpriteRenderer spriteRenderer;
    private Coroutine invincibilityCoroutine = null;

    // ---------- MYSTERY BOX EFFECTS ----------
    private bool isInverted = false;
    private bool isDashLocked = false;
    private bool isMagnetActive = false;
    public float magnetRadius = 5f;
    public float magnetForce = 10f;

    private bool isDirtyBuffActive = false;
    private Color originalColor = Color.white;
    public float dirtyBuffAlpha = 0.6f; // độ trong suốt khi buff
    private Vector3 lastDeathPosition;
    private float scoreMultiplier = 1f;
    private Coroutine scoreBoostCoroutine = null;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        baseMoveSpeed = moveSpeed; // lưu speed gốc, không thay đổi trực tiếp
        targetYLane = transform.position.y;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
        rb.freezeRotation = true;

        // cập nhật UI tim
        if (uiManager != null)
            uiManager.UpdateHearts(health);
        isInvincible = false;
    }

    void Update()
    {
        KeyCode keyUp = isInverted ? KeyCode.S : KeyCode.W;
        KeyCode keyDown = isInverted ? KeyCode.W : KeyCode.S;

        // ĐỔI LANE
        if (Input.GetKeyDown(keyUp))
            targetYLane = topLaneY;
        else if (Input.GetKeyDown(keyDown))
            targetYLane = lowerLaneY;

        // >>> RỄ PHẢI (TURN RIGHT) = nhấn S <<<
        if (Input.GetKeyDown(keyDown))
        {
            LevelRuntimeManager.I?.ReportPlayerTurned(true);
            LevelRuntimeManager.I?.ReportPlayerTurnedRight();
        }

        if (Input.GetKeyDown(KeyCode.Space) && !isDashing && !isDashLocked)
            StartCoroutine(Dash());
        // delta điểm frame này
        float delta = Time.deltaTime * scoreMultiplier;

        // cập nhật local score cũ (nếu có UI nào đang dùng)
        localScore += delta;

        // gửi score chính vào ScoreManager
        if (Nhi_ScoreManager.I != null)
            Nhi_ScoreManager.I.AddScore(delta);



        if (isMagnetActive) PullItems();
    }

    private void FixedUpdate()
    {
        // Tính product của các multiplier active (stack multiplicative)
        float productMultiplier = 1f;
        for (int i = 0; i < activeSpeedMultipliers.Count; i++)
            productMultiplier *= activeSpeedMultipliers[i];

        // Tốc độ hiện tại = base * productMultiplier * (dash ? dashMultiplier : 1)
        float currentSpeed = baseMoveSpeed * productMultiplier * (isDashing ? dashSpeedMutiplier : 1f);

        // Clamp để tránh chạy quá nhanh
        currentSpeed = Mathf.Min(currentSpeed, maxSpeedCap);

        float newY = Mathf.Lerp(transform.position.y, targetYLane, Time.fixedDeltaTime * laneSwitchSpeed);
        float newX = transform.position.x + currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(new Vector2(newX, newY));
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        AudioManager.Instance?.PlaySFX(SFXType.Dash);
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
    }

    // ---------- INVINCIBILITY BLINK ----------
    private IEnumerator StartInvincibilityBlink()
    {
        isInvincible = true;
        float timer = 0f;

        while (timer < invincibilityDuration)
        {
            if (spriteRenderer != null) spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        if (spriteRenderer != null) spriteRenderer.enabled = true;
        isInvincible = false;
        // Clear tham chiếu coroutine (không cần StopCoroutine nếu đã hoàn tất)
        invincibilityCoroutine = null;
    }

    // ---------- LOSE LIFE ----------
    public void TakeDamage()
    {
        if (isInvincible) return;

        health--;
        AudioManager.Instance?.PlaySFX(SFXType.Hit);
        if (invincibilityCoroutine != null)
            StopCoroutine(invincibilityCoroutine);
        invincibilityCoroutine = StartCoroutine(StartInvincibilityBlink());

        if (uiManager != null)
            uiManager.UpdateHearts(health);

        LevelRuntimeManager.I?.ReportPlayerTookDamage(1);
        if (health <= 0)
        {
            lastDeathPosition = transform.position;
            OutOfLives();
            gameObject.SetActive(false);
        }
    }

    public void OutOfLives()
    {
        Debug.Log("Player is out of lives.");
        AudioManager.Instance?.StopBGM(0.25f);

        // play death SFX
        AudioManager.Instance?.PlaySFX(SFXType.Death);
        // Dừng bất tử nếu còn
        if (invincibilityCoroutine != null)
        {
            StopCoroutine(invincibilityCoroutine);
            invincibilityCoroutine = null;
        }
        isInvincible = false;

        // Tắt player để ngừng điều khiển
        if (rb != null) rb.simulated = false;
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        this.enabled = false; // khóa input

        // Gọi UI chết từ UIManager
        if (uiManager != null)
        {
            uiManager.ShowDeathWindow();
        }
        gameObject.SetActive(false);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Nếu đang bất tử → không trừ máu, không tương tác
        if (isInvincible) return;

        // Lấy tag để xử lý (tiện debug)
        string tag = collision.tag;

        // Nếu là vật cản có thể dash qua
        if (tag == "Dashable")
        {
            if (isDashing)
            {
                LevelRuntimeManager.I?.AddProgressByType(TaskType.DashThroughCount, 1);

                // Dash thành công: không trừ máu, KHÔNG destroy obstacle.
                // Optional: trigger một hiệu ứng "phá chắn" hoặc âm thanh để player biết dash thành công.
                // Ví dụ (nếu bạn có particle/FX):
                // var obstacle = collision.GetComponent<Obstacle>(); if (obstacle!=null) obstacle.OnDashedByPlayer();
                Debug.Log("Dashed through Dashable (kept obstacle).");
            }
            else
            {
                // Không dash: player bị mất máu, destroy obstacle như trước
                TakeDamage();
                LevelRuntimeManager.I?.ReportCollisionWithObstacle();
                Destroy(collision.gameObject);
                Debug.Log("Hit Dashable while NOT dashing -> took damage and destroyed obstacle.");
            }
        }
        // Nếu là vật cản không thể dash qua
        else if (tag == "Undashable")
        {
            // Luôn gây sát thương, giữ hoặc destroy tuỳ ý (giữ nguyên hành vi hiện tại: destroy)
            TakeDamage();
            Destroy(collision.gameObject);
            Debug.Log("Hit Undashable -> took damage.");
            LevelRuntimeManager.I?.ReportCollisionWithObstacle();
            LevelRuntimeManager.I?.ReportPlayerTookDamage(1);
        }
    }


    void PullItems()
    {
        //Collider2D[] items = Physics2D.OverlapCircleAll(transform.position, magnetRadius);
        //foreach (var item in items)
        //{
        //    if (item.CompareTag("Item"))
        //    {
        //        item.transform.position =
        //            Vector3.MoveTowards(item.transform.position,
        //                                transform.position,
        //                                magnetForce * Time.deltaTime);
        //    }
        //}
        Collider2D[] items = Physics2D.OverlapCircleAll(transform.position, magnetRadius);
        foreach (var item in items)
        {
            if (item.CompareTag("Item"))
            {
                // Tắt bob để khỏi đánh nhau transform
                var bob = item.GetComponent<ItemBobWithPhaseAndDisable>();
                if (bob != null) bob.enabled = false;

                item.transform.position =
                    Vector3.MoveTowards(item.transform.position,
                                        transform.position,
                                        magnetForce * Time.deltaTime);
            }
        }
    }

    // ---------- EFFECTS ----------
    public void ApplyInvertControls(float duration)
    {
        uiManager?.ShowEffectStatus("ĐIỀU KHIỂN BỊ HOÁN ĐỔI", duration);
        StartCoroutine(EffectRoutine(() => isInverted = true, () => isInverted = false, duration));
    }

    public void ApplyDashLock(float duration)
    {
        uiManager?.ShowEffectStatus("DASH ĐÃ BỊ KHÓA!", duration);
        StartCoroutine(EffectRoutine(() => isDashLocked = true, () => isDashLocked = false, duration));
    }

    /// <summary>
    /// ApplySpeedBoost: thêm multiplier vào danh sách activeMultipliers trong duration giây.
    /// multiplier > 1 (ví dụ 1.5f). Hỗ trợ stacking: nếu nhiều buff cùng lúc thì chúng nhân với nhau.
    /// Speed cuối cùng vẫn bị giới hạn bởi maxSpeedCap.
    /// </summary>
    public void ApplySpeedBoost(float duration, float multiplier)
    {
        uiManager?.ShowEffectStatus("ĐANG TĂNG TỐC", duration);
        if (multiplier <= 0f) return;
        StartCoroutine(SpeedBoostRoutine(duration, multiplier));
    }

    private IEnumerator SpeedBoostRoutine(float duration, float multiplier)
    {
        activeSpeedMultipliers.Add(multiplier);
        // (optional) gọi UI: uiManager.ShowSpeedBoost(multiplier, duration);

        yield return new WaitForSeconds(duration);

        // remove 1 phần tử bằng multiplier (remove lần đầu tìm thấy)
        for (int i = 0; i < activeSpeedMultipliers.Count; i++)
        {
            if (Mathf.Approximately(activeSpeedMultipliers[i], multiplier))
            {
                activeSpeedMultipliers.RemoveAt(i);
                break;
            }
        }
    }

    public void ApplyMagnet(float duration)
    {
        uiManager?.ShowEffectStatus("ĐANG HÚT ITEM", duration);
        StartCoroutine(EffectRoutine(() => isMagnetActive = true, () => isMagnetActive = false, duration));
    }

    public void ApplyBlindness(float duration)
    {
        uiManager?.ShowEffectStatus("ĐANG BỊ HIỆU ỨNG MÙ!", duration);
        if (uiManager != null) uiManager.TriggerBlindEffect(duration);
        AudioManager.Instance?.PlaySFX(SFXType.BlindUI);
    }

    private IEnumerator EffectRoutine(System.Action start, System.Action end, float time)
    {
        start?.Invoke();
        yield return new WaitForSeconds(time);
        end?.Invoke();
    }

    public void ReviveWithOneLife()
    {
        // Dừng coroutine invincibility nếu có (phòng trường hợp còn sót)
        if (invincibilityCoroutine != null)
        {
            StopCoroutine(invincibilityCoroutine);
            invincibilityCoroutine = null;
        }
        isInvincible = false;

        health = 1;

        // ----- RESET TOÀN BỘ FLAG RUNTIME QUAN TRỌNG -----
        isInvincible = false;
        isDashing = false;    // QUAN TRỌNG: nếu không, có thể bị kẹt true sau khi chết lúc đang dash
        isDashLocked = false;    // QUAN TRỌNG: nếu chết khi đang bị dash lock, sẽ kẹt
        isInverted = false;
        isMagnetActive = false;
        isDirtyBuffActive = false;

        // Clear các speed buff đang active
        activeSpeedMultipliers.Clear();

        // Nếu có hiệu ứng mù hoặc text hiệu ứng, có thể clear ở UI:
        uiManager?.ClearEffectStatus();
        if (uiManager != null && uiManager.blindPanel != null)
            uiManager.blindPanel.SetActive(false);

        if (uiManager != null)
            uiManager.UpdateHearts(health);

        transform.position = lastDeathPosition;

        // bật lại player
        gameObject.SetActive(true);
        enabled = true;
        if (rb != null) rb.simulated = true;
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = originalColor;   // <- QUAN TRỌNG: xoá hiệu ứng mờ của Dirty Buff
        }

        targetYLane = transform.position.y;
        if (SceneAudioBinder.ActiveBGM != null)
        {
            AudioManager.Instance?.PlayBGM(SceneAudioBinder.ActiveBGM, 0.25f, true, true);
        }
        else
        {
            Debug.LogWarning("Revive: SceneAudioBinder.ActiveBGM is null — no BGM to restart.");
        }
        Debug.Log("Player revived with 1 life!");
    }


    // ---------- NEW: Heal method ----------
    public void Heal(int amount)
    {
        uiManager?.ShowEffectStatus("ĐƯỢC THÊM 1 MẠNG", 1.2f);

        if (amount <= 0) return;

        health += amount;
        if (health > maxHealth) health = maxHealth;

        if (uiManager != null)
            uiManager.UpdateHearts(health);

        Debug.Log("Player healed by " + amount + ". Current health: " + health);
        // Nếu player vừa được hồi từ trạng thái 0 -> >0 => restart BGM
        bool wasDead = (health <= 0);

        if (wasDead && SceneAudioBinder.ActiveBGM != null)
        {
            AudioManager.Instance?.PlayBGM(SceneAudioBinder.ActiveBGM, 0.25f, true, true);
        }
    }

    // ---------- NEW: Dirty Buff (bất tử tạm thời) ----------
    public void StartDirtyBuff(float duration)
    {
        uiManager?.ShowEffectStatus("VƯỢT MỌI VẬT CẢN!!!", duration);
        StartCoroutine(DirtyBuffRoutine(duration));
    }

    private IEnumerator DirtyBuffRoutine(float duration)
    {
        if (isDirtyBuffActive) yield break;
        isDirtyBuffActive = true;

        // KHÔNG dùng prevInvincible nữa, buff này tự quản lý invincible của nó
        isInvincible = true;

        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = dirtyBuffAlpha;
            spriteRenderer.color = c;
        }

        Debug.Log("Dirty Buff started for " + duration + "s");

        yield return new WaitForSeconds(duration);

        // Khi buff kết thúc, tắt bất tử do buff này gây ra
        isInvincible = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
            spriteRenderer.enabled = true;
        }

        isDirtyBuffActive = false;
        Debug.Log("Dirty Buff ended");
    }


    public void ResetStateAfterSceneLoad()
    {
        // Stop any running invincibility coroutine
        if (invincibilityCoroutine != null)
        {
            StopCoroutine(invincibilityCoroutine);
            invincibilityCoroutine = null;
        }

        // Reset flags
        isInvincible = false;
        isDirtyBuffActive = false;
        isMagnetActive = false;
        isDashLocked = false;
        isInverted = false;

        // Stop any other running coroutines that might cause issues? (optional)
        // StopAllCoroutines(); // *không* gọi nếu bạn muốn giữ effect coroutines intentionally

        // Reset visual / physics / component states
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        if (rb != null) rb.simulated = true;
        this.enabled = true; // enable player script (allow input)

        // reset movement-related fields
        activeSpeedMultipliers.Clear();
        targetYLane = transform.position.y;

        // Ensure hearts UI will be updated by UIManager if present
        if (uiManager != null)
        {
            uiManager.UpdateHearts(health);
        }

        // (Optional) reset position to spawn if you want:
        // transform.position = Vector3.zero;

        Debug.Log("Player.ResetStateAfterSceneLoad called: invincibility cleared, flags reset.");
    }
    public void StartScoreBoost(float duration, float targetMultiplier, float rampTime = 0.5f)
    {
        Debug.Log($"StartScoreBoost called on {gameObject.name}: duration={duration}, targetMultiplier={targetMultiplier}");

        if (scoreBoostCoroutine != null)
            StopCoroutine(scoreBoostCoroutine);

        scoreBoostCoroutine = StartCoroutine(ScoreBoostRoutine(duration, targetMultiplier, rampTime));
    }

    private IEnumerator ScoreBoostRoutine(float duration, float targetMultiplier, float rampTime)
    {
        float startMultiplier = scoreMultiplier;

        // Ramp up
        if (rampTime > 0f)
        {
            float t = 0f;
            while (t < rampTime)
            {
                scoreMultiplier = Mathf.Lerp(startMultiplier, targetMultiplier, t / rampTime);
                t += Time.deltaTime;
                yield return null;
            }
        }
        scoreMultiplier = targetMultiplier;
        Debug.Log("ScoreBoost reached targetMultiplier: " + scoreMultiplier);

        // Hold
        yield return new WaitForSeconds(duration);

        // Ramp down
        if (rampTime > 0f)
        {
            float t = 0f;
            while (t < rampTime)
            {
                scoreMultiplier = Mathf.Lerp(targetMultiplier, 1f, t / rampTime);
                t += Time.deltaTime;
                yield return null;
            }
        }
        scoreMultiplier = 1f;
        Debug.Log("ScoreBoost ended, multiplier reset to " + scoreMultiplier);
        scoreBoostCoroutine = null;
    }

}
