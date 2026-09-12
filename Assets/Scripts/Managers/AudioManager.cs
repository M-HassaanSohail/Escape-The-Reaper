using UnityEngine;

/// <summary>
/// Plays background music and one-shot sound effects. One per scene.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Clips (auto-wired by the builder)")]
    public AudioClip bgmClip;
    public AudioClip buttonClickSFX;

    [Header("Volumes")]
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 0.9f;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    void Awake()
    {
        Instance = this;

        // apply the saved master volume so every scene respects the Settings choice
        AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume", 1f);

        // music source is the required component on this object
        musicSource = GetComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume;

        // a dedicated child source for SFX so music isn't interrupted
        var sfxGo = new GameObject("SFXSource");
        sfxGo.transform.SetParent(transform);
        sfxSource = sfxGo.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.volume = sfxVolume;
    }

    void Start()
    {
        if (bgmClip != null)
        {
            musicSource.clip = bgmClip;
            musicSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void PlayButtonClick()
    {
        PlaySFX(buttonClickSFX);
    }

    // ---- static convenience wrappers (safe even if no AudioManager in scene) ----
    public static void PlaySFXStatic(AudioClip clip)
    {
        if (Instance != null) Instance.PlaySFX(clip);
    }

    public static void PlayButtonClickStatic()
    {
        if (Instance != null) Instance.PlayButtonClick();
    }
}
