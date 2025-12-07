using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SFXType
{
    Dash,
    Pickup,
    BlindUI,
    Hit,
    Death,
    // thêm nếu cần: EnemyShoot, EnemyDie, UIConfirm...
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    public AudioSource musicSource;     // BGM channel (loop)
    public AudioSource uiSource;        // UI one-shot
    public AudioSource emitterPrefab;   // prefab used for pooled SFX emitters (2D)

    [Header("Pool")]
    public int poolSize = 12;
    private Queue<AudioSource> pool = new Queue<AudioSource>();

    [Header("SFX Mapping (use inspector)")]
    public List<SFXEntry> sfxEntries = new List<SFXEntry>();
    private Dictionary<SFXType, AudioClip> sfxMap = new Dictionary<SFXType, AudioClip>();

    [System.Serializable]
    public class SFXEntry { public SFXType type; public AudioClip clip; }
    private AudioClip currentBGMClip = null;
     private bool isBgmPaused = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitPool();
            BuildMap();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitPool()
    {
        if (emitterPrefab == null)
        {
            // fallback: create an AudioSource programmatically
            GameObject go = new GameObject("EmitterPrefab");
            go.transform.parent = transform;
            var a = go.AddComponent<AudioSource>();
            a.playOnAwake = false;
            emitterPrefab = a;
            go.SetActive(false);
        }

        for (int i = 0; i < poolSize; i++)
        {
            var inst = Instantiate(emitterPrefab, transform);
            inst.gameObject.SetActive(false);
            pool.Enqueue(inst);
        }
    }

    void BuildMap()
    {
        sfxMap.Clear();
        foreach (var e in sfxEntries)
            if (e != null && e.clip != null)
                sfxMap[e.type] = e.clip;
    }

    // === BGM API ===
    public void PlayBGM(AudioClip clip, float fadeTime = 0.3f, bool loop = true, bool force = false)
    {
        if (clip == null) return;

        // nếu nhạc đang play đúng clip và không ép force → không play lại
        if (!force && musicSource != null && musicSource.isPlaying && musicSource.clip == clip)
            return;

        currentBGMClip = clip;
        StartCoroutine(CrossfadeMusic(clip, fadeTime, loop));
    }

    // ADD HERE — Pause nhạc
    public void PauseBGM()
    {
        if (musicSource == null || !musicSource.isPlaying) return;
        musicSource.Pause();
        isBgmPaused = true;
    }

    // ADD HERE — Unpause nhạc
    public void UnpauseBGM()
    {
        if (musicSource == null || !isBgmPaused) return;
        musicSource.UnPause();
        isBgmPaused = false;
    }


    public void StopBGM(float fadeTime = 0.2f)
    {
        currentBGMClip = null;
        StartCoroutine(FadeOutMusic(fadeTime));
    }

    // ADD HERE — Restart nhạc từ đầu (dùng khi revive hoặc restart)
    public void RestartCurrentBGM(float fadeTime = 0.1f)
    {
        if (currentBGMClip == null) return;
        PlayBGM(currentBGMClip, fadeTime, true, true); // force = true
    }


    IEnumerator CrossfadeMusic(AudioClip newClip, float fadeTime, bool loop)
    {
        if (musicSource == null) yield break;

        // fade out current
        float startVol = musicSource.volume;
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVol, 0f, t / fadeTime);
            yield return null;
        }

        musicSource.clip = newClip;
        musicSource.loop = loop;
        musicSource.Play();

        // fade in
        t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, startVol, t / fadeTime);
            yield return null;
        }
        musicSource.volume = startVol;
    }

    IEnumerator FadeOutMusic(float fadeTime)
    {
        if (musicSource == null) yield break;
        float startVol = musicSource.volume;
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVol, 0f, t / fadeTime);
            yield return null;
        }
        musicSource.Stop();
        musicSource.clip = null;
        musicSource.volume = startVol;
    }

    // === SFX API (enum) ===
    public void PlaySFX(SFXType type)
    {
        if (sfxMap.TryGetValue(type, out AudioClip clip) && clip != null)
        {
            PlaySFX(clip);
        }
    }

    // === SFX API (clip) ===
    public void PlaySFX(AudioClip clip, Vector3? position = null)
    {
        if (clip == null) return;

        AudioSource e = GetEmitter();
        e.transform.position = position ?? transform.position;
        e.clip = clip;
        e.loop = false;
        e.gameObject.SetActive(true);
        e.Play();
        StartCoroutine(DisableAfter(e, clip.length + 0.05f));
    }

    // UI one-shots
    public void PlayUI(AudioClip clip)
    {
        if (uiSource == null || clip == null) return;
        uiSource.PlayOneShot(clip);
    }

    private AudioSource GetEmitter()
    {
        if (pool.Count > 0)
        {
            var e = pool.Dequeue();
            if (e == null) return CreateEmitter();
            return e;
        }
        // pool empty -> create transient emitter (should be rare)
        return CreateEmitter();
    }

    private AudioSource CreateEmitter()
    {
        var inst = Instantiate(emitterPrefab, transform);
        inst.gameObject.SetActive(false);
        return inst;
    }

    private IEnumerator DisableAfter(AudioSource e, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (e == null) yield break;
        e.Stop();
        e.clip = null;
        e.gameObject.SetActive(false);
        pool.Enqueue(e);
    }
}
