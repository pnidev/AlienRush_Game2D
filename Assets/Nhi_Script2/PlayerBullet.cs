using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float moveSpeed = 25f;
    public float timeDestroy = 0.5f;
    public float damage = 1f;

    private Rigidbody2D rb;

    [Header("Hit Sound")]
    public AudioClip obstacleHitSound;      // âm thanh va chạm khi dính obstacle
    [Range(0f, 1f)] public float hitVolume = 1f;   // volume


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // đảm bảo bullet không bị trọng lực ảnh hưởng
        rb.gravityScale = 0f;
        // Kinematic phù hợp khi ta set velocity thủ công
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void Start()
    {
        // dùng velocity của rigidbody để physics engine xử lý chính xác va chạm
        rb.velocity = transform.right * moveSpeed;
        Destroy(gameObject, timeDestroy);
    }

    // Nếu bạn muốn dùng MovePosition (nếu cần tương tác vật lý hơn), thay Start/Update bằng:
    // void FixedUpdate() { rb.MovePosition(rb.position + (Vector2)(transform.right * moveSpeed * Time.fixedDeltaTime)); }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Obstacle"))
        {
            if (obstacleHitSound != null)
            {
                Vector3 soundPos = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);
                AudioSource.PlayClipAtPoint(obstacleHitSound, soundPos, hitVolume);
            }

            Destroy(gameObject);
            return;
        }

    }

}
