// FakeTreeStationary.cs
// Behaviour:
//  - Detect player within detectionRadius
//  - When first detected: wait awakenDelay seconds (stand still)
//  - After awaken: start shooting towards player at random intervals (shootIntervalMin..Max)
//  - Never moves (rb.velocity kept zero). Optional: stop shooting when player leaves range.

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FakeTreeStationary : MonoBehaviour
{
    [Header("Detection & Awaken")]
    public float detectionRadius = 8f;
    public float awakenDelay = 2f;
    public bool requireAwakenEachEntry = false; // if true, will re-awaken each time player re-enters range

    [Header("Shooting")]
    public GameObject bulletPrefab;        // preferably EnemyTreeBullet with Init(Vector2)
    public Transform firePoint;
    public float bulletSpeed = 8f;
    public float shootIntervalMin = 1f;
    public float shootIntervalMax = 2f;
    public bool stopShootingWhenOutOfRange = true;

    [Header("Debug")]
    public bool debugLogs = false;

    Rigidbody2D rb;
    Transform player;
    bool playerInRange = false;
    bool awakened = false;
    Coroutine shootingCoroutine;



    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.velocity = Vector2.zero;
        }
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null && debugLogs) Debug.LogWarning("FakeTreeStationary: Player (tag 'Player') not found.");
    }

    void Update()
    {
        // Ensure tree stays stationary regardless of other scripts
        if (rb != null && rb.velocity != Vector2.zero)
            rb.velocity = Vector2.zero;

        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        bool inRange = dist <= detectionRadius;

        if (inRange && !playerInRange)
        {
            playerInRange = true;
            if (!awakened || requireAwakenEachEntry)
            {
                StartCoroutine(AwakenThenShoot());
            }
            else if (awakened && shootingCoroutine == null)
            {
                shootingCoroutine = StartCoroutine(ShootLoop());
            }
        }
        else if (!inRange && playerInRange)
        {
            playerInRange = false;
            if (stopShootingWhenOutOfRange)
            {
                StopShooting();
            }
        }
    }

    IEnumerator AwakenThenShoot()
    {
        awakened = true;
        if (debugLogs) Debug.Log($"FakeTreeStationary: Player detected. Awaken for {awakenDelay} seconds.");
        yield return new WaitForSeconds(awakenDelay);

        if (debugLogs) Debug.Log("FakeTreeStationary: Awaken complete. Starting shoot loop.");
        shootingCoroutine = StartCoroutine(ShootLoop());
    }

    IEnumerator ShootLoop()
    {
        while (true)
        {
            if (player == null) { yield return null; continue; }

            Vector2 dir = (player.position - transform.position);
            if (dir.sqrMagnitude <= 0.0001f) dir = Vector2.right;

            // flip sprite to face player (optional - depends on art)
            if (dir.x != 0)
            {
                Vector3 s = transform.localScale;
                s.x = Mathf.Sign(dir.x) * Mathf.Abs(s.x);
                transform.localScale = s;
            }

            // spawn bullet
            if (bulletPrefab != null && firePoint != null)
            {
                GameObject b = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

                // try to call Init(Vector2) if exists (EnemyTreeBullet style)
                var treeBullet = b.GetComponent("EnemyTreeBullet");
                if (treeBullet != null)
                {
                    // use SendMessage to avoid compile dependency
                    b.SendMessage("Init", dir.normalized * bulletSpeed, SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    Rigidbody2D brb = b.GetComponent<Rigidbody2D>();
                    if (brb != null)
                    {
                        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                        b.transform.rotation = Quaternion.Euler(0f, 0f, angle);
                        brb.velocity = dir.normalized * bulletSpeed;
                    }
                }
            }
            else
            {
                if (debugLogs) Debug.LogWarning("FakeTreeStationary: bulletPrefab or firePoint not set.");
            }

            float wait = Random.Range(shootIntervalMin, shootIntervalMax);
            yield return new WaitForSeconds(wait);
        }
    }

    void StopShooting()
    {
        if (shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
            shootingCoroutine = null;
            if (debugLogs) Debug.Log("FakeTreeStationary: Stopped shooting (player left range).");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
