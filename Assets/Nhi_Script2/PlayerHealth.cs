using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 3f;
    public float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    // Gọi khi nhận damage
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0f);
        Debug.Log($"Player took {amount} dmg. HP = {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player died");
        // TODO: xử lý khi player chết (disable control, play anim, respawn, v.v.)
        // ví dụ: gameObject.SetActive(false);
    }

    // Optional: heal
    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }
}
