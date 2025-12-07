using UnityEngine;

public class GunEnemy : MonoBehaviour
{
    public Transform player;        // tự động assign từ Enemy
    public float rotateOffset = 0f; // nếu súng bị quay ngược 180 thì đổi số
    public Transform firePoint;
    public GameObject bulletPrefab;
    public float bulletSpeed = 6f;
    public float shootInterval = 1.5f;

    private float shootTimer;

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        RotateToPlayer();
        //HandleShoot();
    }

    void RotateToPlayer()
    {
        // Hướng từ Gun đến Player
        Vector3 dir = player.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, angle + rotateOffset);

        // Xoay lật nếu quay quá 90 độ
        if (angle < -90f || angle > 90f)
        {
            transform.localScale = new Vector3(1, -1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    void HandleShoot()
    {
        shootTimer += Time.deltaTime;

        if (shootTimer >= shootInterval)
        {
            shootTimer = 0f;
            Shoot();
        }
    }

    void Shoot()
    {
        GameObject b = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        Rigidbody2D rb = b.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = firePoint.right * bulletSpeed;
        }
    }
}
