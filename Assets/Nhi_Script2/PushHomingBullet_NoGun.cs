using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PushHomingBullet : MonoBehaviour
{
    [Header("Push settings")]
    public float pushForce = 8f;
    public float rotateSpeed = 720f;
    public float homingTime = 2f;
    public float lifeAfterHoming = 0.3f;
    public GameObject hitEffect;

    Rigidbody2D rb;
    Transform target;
    float speed = 8f;
    float spawnTime;
    bool isHoming = false;

    [Header("Hit Sound")]
    public AudioClip obstacleHitSound;
    [Range(0f, 1f)] public float hitVolume = 1f;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public void Init(Transform targetTransform, float initialSpeed, float homingDuration)
    {
        target = targetTransform;
        speed = initialSpeed;
        homingTime = homingDuration;
        spawnTime = Time.time;
        isHoming = true;

        Vector2 dir = (target.position - transform.position).normalized;
        rb.velocity = dir * speed;
    }

    void FixedUpdate()
    {
        transform.Rotate(0f, 0f, rotateSpeed * Time.fixedDeltaTime);

        if (isHoming && target != null)
        {
            Vector2 toTarget = (target.position - transform.position);
            Vector2 desired = toTarget.normalized * speed;

            rb.velocity = Vector2.Lerp(rb.velocity, desired, 10f * Time.fixedDeltaTime);

            if (Time.time >= spawnTime + homingTime)
            {
                isHoming = false;
                Destroy(gameObject, lifeAfterHoming);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
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
            Rigidbody2D prb = other.GetComponentInParent<Rigidbody2D>();
            if (prb != null)
            {
                Vector2 pushDir = (other.transform.position - transform.position).normalized;
                prb.AddForce(pushDir * pushForce, ForceMode2D.Impulse);
            }

            if (hitEffect != null)
                Instantiate(hitEffect, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }

    void Start()
    {
        Destroy(gameObject, homingTime + lifeAfterHoming + 0.5f);
    }
}
