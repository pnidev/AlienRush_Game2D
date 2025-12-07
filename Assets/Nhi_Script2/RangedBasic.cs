// RangedBasic.cs
using UnityEngine;
using System.Collections;

public class RangedBasic : BaseEnemyController
{
    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float shootIntervalMin = 4f;
    public float shootIntervalMax = 2f;
    public float stopVelocityThreshold = 0.05f; // coi như "đứng im" khi velocity gần = 0

    private Coroutine shootingCoroutine;
    private bool isShooting = false;

    protected override void OnPlayerDetected(float dist)
    {
        base.OnPlayerDetected(dist);
        MoveToMaintainDistance(desiredDistance);

        // Nếu đã ở trong khoảng desiredDistance (velocity sẽ = 0 do MoveToMaintainDistance), bắt đầu bắn
        if (!isShooting && rb.velocity.magnitude <= stopVelocityThreshold && Vector2.Distance(transform.position, player.position) <= detectionRadius)
        {
            shootingCoroutine = StartCoroutine(ShootingLoop());
        }
    }

    protected override void OnPlayerLost()
    {
        base.OnPlayerLost();
        StopShooting();
    }

    // Nếu enemy đang di chuyển (về hoặc lui), tạm dừng shooting
    void LateUpdate()
    {
        // nếu đang bắn nhưng bắt đầu di chuyển (điều chỉnh khoảng cách) thì dừng
        if (isShooting && rb.velocity.magnitude > stopVelocityThreshold)
        {
            StopShooting();
        }
    }

    IEnumerator ShootingLoop()
    {
        isShooting = true;
        while (player != null && Vector2.Distance(transform.position, player.position) <= detectionRadius && rb.velocity.magnitude <= stopVelocityThreshold)
        {
            SpawnBullet();
            float wait = Random.Range(shootIntervalMin, shootIntervalMax);
            yield return new WaitForSeconds(wait);
        }
        isShooting = false;
    }

    void StopShooting()
    {
        if (shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
            shootingCoroutine = null;
        }
        isShooting = false;
    }

    void SpawnBullet()
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject b = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        Rigidbody2D brb = b.GetComponent<Rigidbody2D>();
        if (brb != null)
        {
            Vector2 dir = firePoint.right;

            // LẤY SPEED TỪ EnemyBullet
            float sp = 8f; // fallback

            var enemyBullet = b.GetComponent<EnemyBullet>();
            if (enemyBullet != null)
                sp = enemyBullet.speed;

            brb.velocity = dir * sp;
        }
    }

}
