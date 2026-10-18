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

    [Header("Computer")]
    [SerializeField] private GameObject computerScreen;
    [SerializeField] private AudioClip[] computerClicks;
    [SerializeField] private AudioClip computerHum;
    [Range(0f, 1f)] [SerializeField] private float humVolume = 0.35f;

    [Header("Messages")]
    [SerializeField] private AudioClip[] messageSounds;
    [Range(0f, 1f)] [SerializeField] private float hiddenMessageVolume = 0.5f;
    [Range(-1f, 0f)] [SerializeField] private float hiddenMessagePan = -1f;

    [Header("Shop")]
    [SerializeField] private AudioClip[] purchaseSounds;
    [Range(0f, 1f)] [SerializeField] private float purchaseVolume = 1f;

    [Header("Sources")]
    [SerializeField] private AudioSource clicksSource;
    [SerializeField] private AudioSource messagesSource;
    [SerializeField] private AudioSource shopSource;
    [SerializeField] private AudioSource loopSource;

    private readonly AudioSource[] sfxSources = new AudioSource[3];
    private readonly float[] sfxVolumes = new float[3];

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

        if (loopSource == null)
        {
            loopSource = gameObject.AddComponent<AudioSource>();
            loopSource.playOnAwake = false;
            loopSource.loop = true;
            loopSource.volume = humVolume;
        }
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

        UpdateComputerHum(inComputer);
    }

    public void PlayRepairClick()
    {
        PlayRandom(Channel.Clicks, repairClicks);
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

    private AudioSource EnsureSfxSource(AudioSource source)
    {
        if (source == null)
        {
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
        }

        return source;
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

    private void UpdateComputerHum(bool inComputer)
    {
        if (computerHum == null || loopSource == null)
        {
            return;
        }

        if (!inComputer)
        {
            if (loopSource.isPlaying)
            {
                loopSource.Stop();
            }

            return;
        }

        if (loopSource.isPlaying)
        {
            return;
        }

        loopSource.clip = computerHum;
        loopSource.Play();
    }
}
