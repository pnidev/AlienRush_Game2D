using UnityEngine;

public static class AudioHelper
{
    public static void Play2D(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        GameObject go = new GameObject("OneShot2DAudio");
        AudioSource a = go.AddComponent<AudioSource>();

        a.spatialBlend = 0f; // ép 2D
        a.playOnAwake = false;
        a.loop = false;
        a.volume = Mathf.Clamp01(volume);

        a.PlayOneShot(clip);
        Object.Destroy(go, clip.length + 0.05f);
    }
}
