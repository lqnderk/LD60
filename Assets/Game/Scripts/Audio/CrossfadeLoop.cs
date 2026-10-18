using UnityEngine;

public class CrossfadeLoop
{
    private readonly AudioSource primary;
    private readonly AudioSource secondary;
    private readonly float crossfade;
    private readonly float pitch;

    private AudioSource current;
    private AudioSource incoming;
    private AudioClip clip;
    private double loopStart;
    private double crossStart;
    private bool crossing;
    private bool playing;

    public CrossfadeLoop(AudioSource primary, AudioSource secondary, float crossfade, float pitch = 1f)
    {
        this.primary = primary;
        this.secondary = secondary;
        this.crossfade = crossfade;
        this.pitch = Mathf.Max(0.01f, pitch);

        Prepare(primary, this.pitch);
        Prepare(secondary, this.pitch);
    }

    public bool IsPlaying => playing;

    public void Play(AudioClip newClip)
    {
        if (newClip == null)
        {
            return;
        }

        if (playing && clip == newClip)
        {
            return;
        }

        Stop();

        clip = newClip;
        current = primary;
        loopStart = AudioSettings.dspTime;
        playing = true;

        current.clip = clip;
        current.time = 0f;
        current.volume = 0f;
        current.Play();
    }

    public void Stop()
    {
        primary.Stop();
        secondary.Stop();

        primary.volume = 0f;
        secondary.volume = 0f;

        current = null;
        incoming = null;
        crossing = false;
        playing = false;
    }

    public void Tick(float level)
    {
        if (!playing || current == null)
        {
            return;
        }

        Advance();

        float target = Mathf.Clamp01(level);

        if (!crossing)
        {
            current.volume = target;
            return;
        }

        float progress = Mathf.Clamp01((float)((AudioSettings.dspTime - crossStart) / Fade));

        current.volume = target * (1f - progress);
        incoming.volume = target * progress;

        if (progress < 1f)
        {
            return;
        }

        current.Stop();
        current.volume = 0f;
        current = incoming;
        incoming = null;
        crossing = false;
    }

    private float Fade
    {
        get
        {
            float length = Length;
            return Mathf.Clamp(crossfade, 0.02f, Mathf.Max(0.02f, length * 0.5f));
        }
    }

    private float Length => clip != null ? clip.length / pitch : 0f;

    private void Advance()
    {
        if (crossing || clip == null)
        {
            return;
        }

        float length = Length;

        if (length <= 0.1f)
        {
            return;
        }

        double now = AudioSettings.dspTime;

        if (now < loopStart + length - Fade)
        {
            return;
        }

        AudioSource next = current == primary ? secondary : primary;
        double start = now + 0.02;

        next.clip = clip;
        next.time = 0f;
        next.volume = 0f;
        next.PlayScheduled(start);

        incoming = next;
        crossing = true;
        crossStart = start;
        loopStart = start;
    }

    private static void Prepare(AudioSource source, float pitch)
    {
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        source.dopplerLevel = 0f;
        source.volume = 0f;
        source.pitch = pitch;
    }
}
