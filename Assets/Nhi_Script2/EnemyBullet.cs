using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBullet : MonoBehaviour
{
    public float speed = 8f;
    public float lifeTime = 5f;
    public float damage = 1f; // <-- khai báo damage ở đây

    private Rigidbody2D rb;

    [Header("Hit Sound")]
    public AudioClip obstacleHitSound;
    [Range(0f, 1f)] public float hitVolume = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Nếu spawner không set velocity, set mặc định theo hướng hiện tại (transform.right)
        if (rb.velocity.magnitude < 0.01f)
            rb.velocity = transform.right * speed;

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

        // Chỉ xử lý khi chạm Player (theo tag "Player")
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damage);
            }
            Destroy(gameObject);
            return;
        }

        // Nếu chạm environment (ví dụ layer Environment) thì hủy
        if (other.gameObject.layer == LayerMask.NameToLayer("Environment"))
        {
            Destroy(gameObject);
            return;
        }

        // Nếu muốn đạn bị hủy khi chạm bất kỳ collider khác (vd: ground)
        // Destroy(gameObject);
    }
}
