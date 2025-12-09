using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public int currentHealth;

    [Header("UI Hearts")]
    public Image[] heartImages;      // gán 3 image trái tim
    public Sprite heartFull;         // chỉ cần 1 sprite

    [Header("Death UI")]
    public GameObject deathPanel;    // gán panel chết vào đây

    private void Awake()
    {
        currentHealth = maxHealth;
        UpdateHearts();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= Mathf.RoundToInt(amount);   // chuyển damage float → int
        currentHealth = Mathf.Max(currentHealth, 0);

        UpdateHearts();

        if (currentHealth <= 0)
            Die();
    }


    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UpdateHearts();
    }

    void UpdateHearts()
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < currentHealth)
            {
                heartImages[i].enabled = true;      // trái tim còn máu → bật
                heartImages[i].sprite = heartFull;  // đặt sprite trái tim
            }
            else
            {
                heartImages[i].enabled = false;     // trái tim mất máu → ẩn hẳn
            }
        }
    }

    void Die()
    {
        Debug.Log("Player died");

        var movement = GetComponent<NewBehaviourScript>();
        if (movement != null)
            movement.enabled = false;

        if (deathPanel != null)
            deathPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
