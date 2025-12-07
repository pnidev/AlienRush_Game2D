using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyTreeBullet : MonoBehaviour
{
    public float speed = 8f;
    public float lifeTime = 4f;
    public float damage = 1f;

    private Rigidbody2D rb;

    [Header("Hit Sound")]
    public AudioClip obstacleHitSound;
    [Range(0f, 1f)] public float hitVolume = 1f;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic; // kinematic phù hợp cho bullet
    }

    // Gọi ngay sau Instantiate để truyền hướng
    public void Init(Vector2 direction)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        Vector2 dir = direction.normalized;
        rb.velocity = dir * speed;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Nếu gặp tảng đá (Obstacle)
        if (other.CompareTag("Obstacle"))
        {
            if (obstacleHitSound != null)
            {
                Vector3 soundPos = new Vector3(
                    transform.position.x,
                    transform.position.y,
                    Camera.main.transform.position.z
                );
                AudioSource.PlayClipAtPoint(obstacleHitSound, soundPos, hitVolume);
            }

            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponentInParent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Nếu muốn đạn bị hủy khi chạm environment
        if (other.gameObject.layer == LayerMask.NameToLayer("Environment"))
        {
            Destroy(gameObject);
        }
    }
}
