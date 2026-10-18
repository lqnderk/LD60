using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private enum Channel
    {
        Clicks,
        Messages,
        Shop
    }

    private static AudioManager instance;

    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<AudioManager>();

                if (instance == null)
                {
                    var host = new GameObject(nameof(AudioManager));
                    instance = host.AddComponent<AudioManager>();
                }
            }

            return instance;
        }
    }

    [Header("Repair Clicks")]
    [SerializeField] private AudioClip[] repairClicks;
    [Range(0f, 1f)] [SerializeField] private float repairVolume = 1f;

    [Header("Computer")]
    [SerializeField] private GameObject computerScreen;
    [SerializeField] private AudioClip[] computerClicks;
    [SerializeField] private AudioClip computerHum;
    [Range(0f, 1f)] [SerializeField] private float humVolume = 0.35f;
    [Range(0f, 2f)] [SerializeField] private float humFade = 0.4f;
    [Range(0.02f, 1f)] [SerializeField] private float humCrossfade = 0.25f;

    [Header("Messages")]
    [SerializeField] private AudioClip[] messageSounds;
    [Range(0f, 1f)] [SerializeField] private float hiddenMessageVolume = 0.5f;
    [Range(-1f, 0f)] [SerializeField] private float hiddenMessagePan = -1f;

    [Header("Shop")]
    [SerializeField] private AudioClip[] purchaseSounds;
    [Range(0f, 1f)] [SerializeField] private float purchaseVolume = 1f;

    [Header("Music")]
    [SerializeField] private AudioClip[] musicTracks;
    [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.25f;
    [Range(0.5f, 1.5f)] [SerializeField] private float musicPitch = 0.85f;
    [Range(0f, 2f)] [SerializeField] private float musicFade = 1f;
    [Range(0.02f, 1f)] [SerializeField] private float musicCrossfade = 0.5f;

    [Header("Sources")]
    [SerializeField] private AudioSource clicksSource;
    [SerializeField] private AudioSource messagesSource;
    [SerializeField] private AudioSource shopSource;
    [SerializeField] private AudioSource loopSource;
    [SerializeField] private AudioSource musicSource;

    private readonly AudioSource[] sfxSources = new AudioSource[3];
    private readonly float[] sfxVolumes = new float[3];

    private CrossfadeLoop humLoop;
    private CrossfadeLoop musicLoop;
    private float humLevel;
    private float musicLevel;

    public float MusicVolume
    {
        get { return musicVolume; }
        set { musicVolume = Mathf.Clamp01(value); }
    }

    public float RepairVolume
    {
        get { return repairVolume; }
        set { repairVolume = Mathf.Clamp01(value); }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        sfxSources[(int)Channel.Clicks] = EnsureSfxSource(clicksSource);
        sfxSources[(int)Channel.Messages] = EnsureSfxSource(messagesSource);
        sfxSources[(int)Channel.Shop] = EnsureSfxSource(shopSource);

        for (int i = 0; i < sfxSources.Length; i++)
        {
            sfxVolumes[i] = sfxSources[i].volume;
        }

        humLoop = new CrossfadeLoop(EnsureLoopSource(ref loopSource), CreateSource(), humCrossfade);
        musicLoop = new CrossfadeLoop(EnsureLoopSource(ref musicSource), CreateSource(), musicCrossfade, musicPitch);

        LoadClip(computerHum);
        LoadClips(musicTracks);
        LoadClips(repairClicks);
        LoadClips(computerClicks);
        LoadClips(messageSounds);
        LoadClips(purchaseSounds);
    }

    private void Start()
    {
        PlayMusic();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void Update()
    {
        bool inComputer = computerScreen != null && computerScreen.activeInHierarchy;

        if (inComputer && Input.GetMouseButtonDown(0))
        {
            PlayComputerClick();
        }

        UpdateHum(inComputer);
        UpdateMusic(inComputer);
    }

    public void PlayRepairClick()
    {
        PlayRandom(Channel.Clicks, repairClicks, repairVolume);
    }

    public void PlayComputerClick()
    {
        PlayRandom(Channel.Clicks, computerClicks);
    }

    public void PlayMessage(bool onScreen)
    {
        if (onScreen)
        {
            PlayRandom(Channel.Messages, messageSounds);
            return;
        }

        PlayRandom(Channel.Messages, messageSounds, hiddenMessageVolume, hiddenMessagePan);
    }

    public void PlayPurchase()
    {
        PlayRandom(Channel.Shop, purchaseSounds, purchaseVolume);
    }

    private void PlayMusic()
    {
        if (musicTracks == null || musicTracks.Length == 0)
        {
            return;
        }

        musicLoop.Play(musicTracks[Random.Range(0, musicTracks.Length)]);
    }

    private void UpdateHum(bool inComputer)
    {
        if (computerHum == null)
        {
            return;
        }

        humLevel = Mathf.MoveTowards(humLevel, inComputer ? 1f : 0f, FadeStep(humFade));

        if (!inComputer && humLevel <= 0f)
        {
            humLoop.Stop();
            return;
        }

        humLoop.Play(computerHum);
        humLoop.Tick(humVolume * humLevel);
    }

    private void UpdateMusic(bool inComputer)
    {
        musicLevel = Mathf.MoveTowards(musicLevel, inComputer ? 0f : 1f, FadeStep(musicFade));
        musicLoop.Tick(musicVolume * musicLevel);
    }

    private AudioSource EnsureSfxSource(AudioSource source)
    {
        if (source == null)
        {
            source = CreateSource();
        }

        return source;
    }

    private AudioSource EnsureLoopSource(ref AudioSource source)
    {
        if (source == null)
        {
            source = CreateSource();
        }

        return source;
    }

    private AudioSource CreateSource()
    {
        var source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;

        return source;
    }

    private static float FadeStep(float duration)
    {
        return duration > 0.01f ? Time.unscaledDeltaTime / duration : 1f;
    }

    private static void LoadClip(AudioClip clip)
    {
        if (clip != null && clip.loadState != AudioDataLoadState.Loaded)
        {
            clip.LoadAudioData();
        }
    }

    private static void LoadClips(AudioClip[] clips)
    {
        if (clips == null)
        {
            return;
        }

        for (int i = 0; i < clips.Length; i++)
        {
            LoadClip(clips[i]);
        }
    }

    private void PlayRandom(Channel channel, AudioClip[] clips, float volume = 1f, float pan = 0f)
    {
        int index = (int)channel;
        AudioSource source = sfxSources[index];

        if (clips == null || clips.Length == 0 || source == null)
        {
            return;
        }

        AudioClip clip = clips[Random.Range(0, clips.Length)];

        if (clip == null)
        {
            return;
        }

        source.Stop();
        source.clip = clip;
        source.volume = sfxVolumes[index] * volume;
        source.panStereo = pan;
        source.Play();
    }
}
