using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class FakeTreeSimple : BaseEnemyController
{
    [Header("FakeTree - Simple")]
    public float awakenDelay = 2f;            // đứng im trước khi "thức"
    public float shootIntervalMin = 1.5f;
    public float shootIntervalMax = 2f;
    public float stopVelocityThreshold = 0.05f;

    [Header("Shooting")]
    public GameObject bulletPrefab;          // EnemyBullet prefab
    public Transform firePoint;              // child empty transform để spawn đạn

    bool isAwakened = false;
    Coroutine shootingCoroutine;

    protected override void Start()
    {
        base.Start();
        // phần còn lại giữ nguyên hoặc trống
    }


    protected override void OnPlayerDetected(float dist)
    {
        // nếu chưa awaken -> bắt đầu quá trình awaken (chỉ 1 lần)
        if (!isAwakened)
        {
            isAwakened = true;
            // block movement while waiting: set velocity zero
            velocity = Vector2.zero;
            StartCoroutine(AwakenRoutine());
            return;
        }

        // đã awaken -> hành xử giống ranged: giữ khoảng cách và (nếu đứng) bắn
        FacePlayer();
        MoveToMaintainDistance(desiredDistance);

        // nếu đứng yên và chưa có coroutine bắn -> start
        if (rb.velocity.magnitude <= stopVelocityThreshold && shootingCoroutine == null)
        {
            shootingCoroutine = StartCoroutine(ShootingLoop());
        }
    }

    IEnumerator AwakenRoutine()
    {
        // đợi awakenDelay (đứng im)
        float t = 0f;
        while (t < awakenDelay)
        {
            t += Time.deltaTime;
            yield return null;
        }

        // sau khi awaken, bắt đầu hành động (sẽ gọi OnPlayerDetected tiếp frame sau)
        yield break;
    }

    IEnumerator ShootingLoop()
    {
        while (player != null && Vector2.Distance(transform.position, player.position) <= detectionRadius)
        {
            // bắn chỉ khi đứng im (hoặc gần im)
            if (rb.velocity.magnitude <= stopVelocityThreshold)
            {
                ShootToPlayer();
            }

            float wait = Random.Range(shootIntervalMin, Mathf.Max(shootIntervalMin, shootIntervalMax));
            yield return new WaitForSeconds(wait);

            // nếu enemy bắt đầu di chuyển → tạm dừng shooting loop để preserve logic giữ khoảng cách
            if (rb.velocity.magnitude > stopVelocityThreshold) break;
        }

        shootingCoroutine = null;
    }


    void SpawnBullet()
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject b = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        Rigidbody2D brb = b.GetComponent<Rigidbody2D>();
        if (brb != null)
        {
            var eb = b.GetComponent<EnemyBullet>();
            float sp = (eb != null) ? eb.speed : 8f;
            brb.velocity = firePoint.right * sp;
        }
    }

    protected override void OnPlayerLost()
    {
        base.OnPlayerLost();
        // stop shooting if active
        if (shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
            shootingCoroutine = null;
        }
        // không revert cây về trạng thái chưa awaken (để đơn giản). Muốn revert thì set isAwakened=false ở đây.
    }
    void ShootToPlayer()
    {
        if (bulletPrefab == null || player == null || firePoint == null) return;

        Vector2 dir = (player.position - firePoint.position).normalized;

        GameObject b = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        // Nếu prefab dùng EnemyTreeBullet
        EnemyTreeBullet etb = b.GetComponent<EnemyTreeBullet>();
        if (etb != null)
        {
            etb.Init(dir);
        }
        else
        {
            // fallback: set rigidbody velocity theo dir nếu prefab chỉ có Rigidbody2D
            Rigidbody2D brb = b.GetComponent<Rigidbody2D>();
            if (brb != null)
            {
                brb.velocity = dir * (b.GetComponent<EnemyBullet>()?.speed ?? 8f);
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                b.transform.rotation = Quaternion.Euler(0f, 0f, angle);
                Destroy(b, 4f);
            }
        }
    }

}
