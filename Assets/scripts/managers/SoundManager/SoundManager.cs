using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Library")]
    public SoundLibrary library;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public AudioSource pitchSource;

    [Header("3D Pool")]
    public int poolSize = 10;
    private List<AudioSource> sfx3DSources;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Create3DPool();
    }

    private void Create3DPool()
    {
        sfx3DSources = new List<AudioSource>();

        for (int i = 0; i < poolSize; i++)
        {
            var go = new GameObject($"SFX3D_{i}");
            go.transform.parent = transform;

            AudioSource a = go.AddComponent<AudioSource>();
            a.spatialBlend = 1f;
            a.playOnAwake = false;

            sfx3DSources.Add(a);
        }
    }

    // -------------------- BGM --------------------
    public void PlayBGM(string id)
    {
        var clip = library.GetClip(id);
        if (clip == null) return;

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    // -------------------- SFX 2D --------------------
    public void PlaySFX(string id)
    {
        var clip = library.GetClip(id);
        if (clip == null) return;

        sfxSource.PlayOneShot(clip);
    }

    // -------------------- SFX 3D --------------------
    public void PlaySFX3D(string id, Vector3 position)
    {
        var clip = library.GetClip(id);
        if (clip == null) return;

        AudioSource source = GetAvailable3DSource();

        source.transform.position = position;
        source.clip = clip;
        source.Play();
    }

    private AudioSource GetAvailable3DSource()
    {
        foreach (var s in sfx3DSources)
        {
            if (!s.isPlaying) return s;
        }

        return sfx3DSources[0];
    }

    // -------------------- SFX Pitch --------------------
    public void PlaySFXPitch(string id, float minPitch = 0.9f, float maxPitch = 1.1f)
    {
        var clip = library.GetClip(id);
        if (clip == null) return;

        pitchSource.pitch = Random.Range(minPitch, maxPitch);
        pitchSource.PlayOneShot(clip);
    }
}
