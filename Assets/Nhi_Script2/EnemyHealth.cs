using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public int currentHealth;

    private float lastHitTime = -10f;
    public float hitInvul = 0.1f; // tránh nhận 2 hit trong 1 frame
    [Header("Audio (2D one-shot)")]
    public AudioClip dieClip;
    [Range(0f, 1f)] public float dieVolume = 1f;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (Time.time - lastHitTime < hitInvul) return;
        lastHitTime = Time.time;

        currentHealth -= amount;
        Debug.Log($"{name} took {amount} dmg. HP = {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        AudioHelper.Play2D(dieClip, dieVolume);
        Destroy(gameObject);
    }

    // Player chạm enemy -> gây damage cho Player (không trừ HP của enemy)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            PlayerHealth ph = collision.collider.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(1); // chạm gây 1 damage cho player (tùy chỉnh)
            }
        }
    }

    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerBullet"))
        {
           
            PlayerBullet pb = other.GetComponent<PlayerBullet>();
            if (pb != null)
            {
                TakeDamage(Mathf.CeilToInt(pb.damage));
            }
            else
            {
                TakeDamage(1);
            }

            Destroy(other.gameObject);
        }
    }
}
