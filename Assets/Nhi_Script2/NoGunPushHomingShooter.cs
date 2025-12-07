using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class NoGunPushHomingShooter : MonoBehaviour
{
    [Header("Detection & Movement")]
    public float detectionRadius = 10f;
    public float speed = 3f;
    public float desiredDistance = 6f;
    public bool facePlayer = true;

    [Header("Spawn (no gun)")]
    public Transform spawnPoint;
    public Vector2 spawnOffset = new Vector2(0.5f, 0.0f);

    [Header("Burst / Shoot")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 8f;
    public float fireRate = 0.12f;
    public float burstDurationMin = 1f;
    public float burstDurationMax = 2f;
    public float bulletHomingDuration = 2f;

    [Header("Vertical movement (optional)")]
    public bool allowMoveY = false;
    public float verticalSpeed = 2.5f;
    public float yThreshold = 0.12f;

    [Header("Animation")]
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
        var pgo = GameObject.FindGameObjectWithTag("Player");
        if (pgo != null) player = pgo.transform;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (player == null) return;
        float dist = Vector2.Distance(transform.position, player.position);
        playerInRange = dist <= detectionRadius;

        if (playerInRange && facePlayer) FlipToPlayer();
    }

    void FixedUpdate()
    {
        if (player == null) return;

        if (!playerInRange)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            animator.SetBool("IsMoving", false);
            return;
        }

        float dx = player.position.x - transform.position.x;
        float absDx = Mathf.Abs(dx);

        // MOVE X
        if (absDx > desiredDistance + 0.1f)
        {
            float dir = Mathf.Sign(dx);
            velocity.x = dir * speed;
            animator.SetBool("IsMoving", true);
        }
        else if (absDx < desiredDistance - 0.1f)
        {
            float dir = -Mathf.Sign(dx);
            velocity.x = dir * speed;
            animator.SetBool("IsMoving", true);
        }
        else
        {
            velocity.x = 0f;
            animator.SetBool("IsMoving", false);

            if (!isBursting && burstRoutine == null)
                burstRoutine = StartCoroutine(BurstLoop());
        }

        // MOVE Y (optional)
        if (allowMoveY)
        {
            float dy = player.position.y - transform.position.y;
            velocity.y = Mathf.Abs(dy) > yThreshold ? Mathf.Sign(dy) * verticalSpeed : 0f;
        }
        else velocity.y = rb.velocity.y;

        rb.velocity = velocity;
    }

    IEnumerator BurstLoop()
    {
        isBursting = true;

        float duration = Random.Range(burstDurationMin, burstDurationMax);
        float endTime = Time.time + duration;

        while (Time.time < endTime && playerInRange)
        {
            SpawnBullet();
            yield return new WaitForSeconds(fireRate);
        }

        isBursting = false;
        burstRoutine = null;
    }

    void SpawnBullet()
    {
        if (player == null || bulletPrefab == null) return;

        Vector3 spawnPos =
            (spawnPoint != null)
                ? spawnPoint.position
                : transform.position + (Vector3)spawnOffset;

        GameObject b = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        PushHomingBullet bullet = b.GetComponent<PushHomingBullet>();
        bullet.Init(player, bulletSpeed, bulletHomingDuration);
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
