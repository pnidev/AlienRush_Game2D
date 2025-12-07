using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FakeRockMine : MonoBehaviour
{
    [Header("Trigger & One-shot")]
    public string playerTag = "Player";
    public bool triggerOnce = true;
    bool triggered = false;

    [Header("Beep (tít)")]
    public AudioClip beepClip;
    public int beepCount = 3;
    public float beepInterval = 1f;
    public float beepVolume = 0.8f;

    [Header("Explosion")]
    public AudioClip explosionClip;
    public float explosionVolume = 2f;
    public GameObject explosionParticlePrefab;
    public float explosionRadius = 1.2f;
    public int damage = 1;

    [Header("Physics / Collider")]
    public bool useTriggerCollider = true;
    public LayerMask damageLayerMask;
    public string damageableTag = "Player";

    [Header("AudioSources (auto create if null)")]
    public AudioSource audioSourceBeep;
    public AudioSource audioSourceExplode;

    [Header("Debug / Visual")]
    public bool drawGizmos = true;

    Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        if (col == null)
            Debug.LogError("[FakeRockMine] Missing Collider2D");

        if (col != null)
            col.isTrigger = useTriggerCollider;

        if (audioSourceBeep == null)
        {
            audioSourceBeep = gameObject.AddComponent<AudioSource>();
            audioSourceBeep.playOnAwake = false;
            audioSourceBeep.loop = false;
            audioSourceBeep.spatialBlend = 0f;
        }

        if (audioSourceExplode == null)
        {
            audioSourceExplode = gameObject.AddComponent<AudioSource>();
            audioSourceExplode.playOnAwake = false;
            audioSourceExplode.loop = false;
            audioSourceExplode.spatialBlend = 0f;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered && triggerOnce) return;
        if (!other.CompareTag(playerTag)) return;

        StartCoroutine(PlayBeepsAndExplode());
        triggered = true;
    }

    IEnumerator PlayBeepsAndExplode()
    {
        for (int i = 0; i < beepCount; i++)
        {
            if (beepClip != null)
            {
                audioSourceBeep.PlayOneShot(beepClip, beepVolume);
            }
            yield return new WaitForSeconds(beepInterval);
        }

        Explode();
    }

    void Explode()
    {
        Debug.Log($"[FakeRockMine] Explode() called on {gameObject.name}");

        // Play explosion sound
        if (explosionClip != null)
        {
            AudioSource.PlayClipAtPoint(explosionClip, transform.position, explosionVolume);
        }

        // Spawn particle effect
        if (explosionParticlePrefab != null)
        {
            GameObject p = Instantiate(explosionParticlePrefab, transform.position, Quaternion.identity);
            ParticleSystem ps = p.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
                Destroy(p, ps.main.duration + ps.main.startLifetime.constantMax);
            else
                Destroy(p, 5f);
        }

        // DAMAGE PLAYER
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            explosionRadius,
            damageLayerMask);

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag(damageableTag)) continue;

            var ph = hit.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

}
