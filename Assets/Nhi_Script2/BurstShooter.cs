using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BurstShooter : MonoBehaviour
{
    [Header("Detect & Move")]
    public float detectionRadius = 10f;
    public float speed = 3f;
    public float desiredDistance = 6f;

    [Header("Vertical Movement (optional)")]
    public bool allowMoveY = false;
    public float verticalSpeed = 2.5f;
    public float yThreshold = 0.12f;

    [Header("Shoot")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 8f;
    public float fireRate = 0.1f;
    public float burstDuration = 0.5f;
    public float burstCooldown = 2f;

    public Animator animator;

    Rigidbody2D rb;
    Transform player;
    bool playerInRange = false;
    bool isBursting = false;
    Coroutine burstRoutine;
    Vector2 velocity = Vector2.zero;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Start()
    {
        // tìm player an toàn
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        playerInRange = dist <= detectionRadius;

        if (playerInRange)
            FlipToPlayer();
    }

    void FixedUpdate()
    {
        if (player == null)
        {
            rb.velocity = Vector2.zero;
            if (animator) animator.SetBool("IsMoving", false);
            return;
        }

        if (!playerInRange)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            if (animator) animator.SetBool("IsMoving", false);
            return;
        }

        float dx = player.position.x - transform.position.x;
        float absDx = Mathf.Abs(dx);

        // === MOVE X GIỮ KHOẢNG CÁCH ===
        if (absDx > desiredDistance + 0.1f)
        {
            float dir = Mathf.Sign(dx);
            velocity.x = dir * speed;
            if (animator) animator.SetBool("IsMoving", true);
        }
        else if (absDx < desiredDistance - 0.1f)
        {
            float dir = -Mathf.Sign(dx);
            velocity.x = dir * speed;
            if (animator) animator.SetBool("IsMoving", true);
        }
        else
        {
            // đúng khoảng cách → đứng im → bắn
            velocity.x = 0f;
            if (animator) animator.SetBool("IsMoving", false);

            if (!isBursting && burstRoutine == null)
                burstRoutine = StartCoroutine(BurstLoop());
        }

        // === MOVE Y (OPTIONAL) ===
        if (allowMoveY)
        {
            float dy = player.position.y - transform.position.y;
            if (Mathf.Abs(dy) > yThreshold)
                velocity.y = Mathf.Sign(dy) * verticalSpeed;
            else
                velocity.y = 0f;
        }
        else
        {
            velocity.y = rb.velocity.y;   // giữ nguyên y
        }

        rb.velocity = velocity;
    }

    IEnumerator BurstLoop()
    {
        if (isBursting) yield break;
        if (bulletPrefab == null || firePoint == null) yield break;

        isBursting = true;

        float endTime = Time.time + burstDuration;
        while (Time.time < endTime)
        {
            if (!playerInRange || player == null) break;

            Shoot();
            yield return new WaitForSeconds(Mathf.Max(0.02f, fireRate));
        }

        yield return new WaitForSeconds(burstCooldown);

        isBursting = false;
        burstRoutine = null;
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null || player == null) return;

        Vector2 dir = (player.position - firePoint.position).normalized;

        GameObject b = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        Rigidbody2D rbBullet = b.GetComponent<Rigidbody2D>();
        if (rbBullet != null)
        {
            rbBullet.velocity = dir * bulletSpeed;
        }
        else
        {
            // fallback cho loại đạn dùng Init()
            b.SendMessage("Init", dir * bulletSpeed, SendMessageOptions.DontRequireReceiver);
        }
    }

    void FlipToPlayer()
    {
        float dx = player.position.x - transform.position.x;
        if (dx == 0) return;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Sign(dx) * Mathf.Abs(scale.x);
        transform.localScale = scale;
    }
}
