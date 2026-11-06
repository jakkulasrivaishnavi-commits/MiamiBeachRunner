using System.Collections.Generic;
using UnityEngine;

public enum Sfx { Coin, Key, Crash, Click, Jump, Slide, Lane, Revive }

/// <summary>
/// Generates all audio procedurally (no audio files needed): coin/key pickups, crash, button click,
/// beach-wave ambience and a looping synthwave track. Volumes: Master / Music / SFX (saved).
/// </summary>
public class AudioManager : MonoBehaviour
{
    const int SR = 22050;
    const float TAU = 6.2831853f;

    static AudioManager inst;
    static readonly Dictionary<Sfx, AudioClip> sfxClips = new Dictionary<Sfx, AudioClip>();
    static AudioClip musicClip, ambienceClip;

    AudioSource music, ambience, sfx;
    float master = 1f, musicVol = 0.7f, sfxVol = 0.8f;

    public static AudioManager Ensure()
    {
        if (inst != null) return inst;
        GameObject go = new GameObject("AudioManager");
        DontDestroyOnLoad(go);
        inst = go.AddComponent<AudioManager>();
        return inst;
    }

    public static void Play(Sfx s)
    {
        Ensure().PlayInternal(s);
    }

    /// <summary>which: 0 = master, 1 = music, 2 = sfx.</summary>
    public static float GetVolume(int which)
    {
        AudioManager a = Ensure();
        return which == 0 ? a.master : (which == 1 ? a.musicVol : a.sfxVol);
    }

    public static void SetVolume(int which, float v)
    {
        AudioManager a = Ensure();
        v = Mathf.Clamp01(v);
        if (which == 0) { a.master = v; PlayerPrefs.SetFloat(GameConfig.PrefMaster, v); }
        else if (which == 1) { a.musicVol = v; PlayerPrefs.SetFloat(GameConfig.PrefMusic, v); }
        else { a.sfxVol = v; PlayerPrefs.SetFloat(GameConfig.PrefSfx, v); }
        a.ApplyVolumes();
    }

    void Awake()
    {
        master = PlayerPrefs.GetFloat(GameConfig.PrefMaster, 1.0f);
        musicVol = PlayerPrefs.GetFloat(GameConfig.PrefMusic, 0.7f);
        sfxVol = PlayerPrefs.GetFloat(GameConfig.PrefSfx, 0.8f);

        music = gameObject.AddComponent<AudioSource>();
        music.loop = true; music.playOnAwake = false; music.spatialBlend = 0f;
        ambience = gameObject.AddComponent<AudioSource>();
        ambience.loop = true; ambience.playOnAwake = false; ambience.spatialBlend = 0f;
        sfx = gameObject.AddComponent<AudioSource>();
        sfx.playOnAwake = false; sfx.spatialBlend = 0f;

        if (musicClip == null) musicClip = MakeMusic();
        if (ambienceClip == null) ambienceClip = MakeAmbience();
        music.clip = musicClip;
        ambience.clip = ambienceClip;
        ApplyVolumes();
        music.Play();
        ambience.Play();
    }

    void ApplyVolumes()
    {
        if (music != null) music.volume = master * musicVol * 0.55f;
        if (ambience != null) ambience.volume = master * sfxVol * 0.45f;
        if (sfx != null) sfx.volume = master * sfxVol;
    }

    void PlayInternal(Sfx s)
    {
        AudioClip c;
        if (!sfxClips.TryGetValue(s, out c) || c == null)
        {
            c = MakeSfx(s);
            sfxClips[s] = c;
        }
        float vol = 1f;
        if (s == Sfx.Lane) vol = 0.35f;
        if (s == Sfx.Slide) vol = 0.6f;
        sfx.PlayOneShot(c, vol);
    }

    // ------------------------------------------------------------------ synthesis

    static AudioClip Synth(string name, float seconds, System.Func<float, float> f, bool loop)
    {
        int n = (int)(SR * seconds);
        float[] d = new float[n];
        for (int i = 0; i < n; i++) d[i] = Mathf.Clamp(f(i / (float)SR), -1f, 1f);
        AudioClip c = AudioClip.Create(name, n, 1, SR, false);
        c.SetData(d, 0);
        return c;
    }

    static float Saw(float x) { return 2f * (x - Mathf.Floor(x)) - 1f; }
    static float Freq(int midi) { return 440f * Mathf.Pow(2f, (midi - 69) / 12f); }

    static AudioClip MakeSfx(Sfx s)
    {
        System.Random rnd = new System.Random(3);
        switch (s)
        {
            case Sfx.Coin:
                return Synth("coin", 0.35f, t =>
                {
                    float fr = t < 0.07f ? 988f : 1319f;
                    return 0.45f * Mathf.Exp(-t * 11f) * Mathf.Min(1f, t * 400f) * Mathf.Sin(TAU * fr * t);
                }, false);
            case Sfx.Key:
                return Synth("key", 0.6f, t =>
                {
                    float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f };
                    int idx = Mathf.Min(3, (int)(t / 0.075f));
                    float lt = t - idx * 0.075f;
                    float v = 0.4f * Mathf.Exp(-lt * 12f) * Mathf.Sin(TAU * notes[idx] * t);
                    if (t > 0.3f) v += 0.25f * Mathf.Exp(-(t - 0.3f) * 6f) * Mathf.Sin(TAU * 1568f * t);
                    return v;
                }, false);
            case Sfx.Crash:
                return Synth("crash", 0.9f, t =>
                {
                    float noise = (float)(rnd.NextDouble() * 2.0 - 1.0);
                    return 0.7f * noise * Mathf.Exp(-t * 5f) + 0.8f * Mathf.Sin(TAU * 62f * t) * Mathf.Exp(-t * 6f);
                }, false);
            case Sfx.Click:
                return Synth("click", 0.08f, t => 0.5f * Mathf.Exp(-t * 55f) * Mathf.Sin(TAU * 1250f * t), false);
            case Sfx.Jump:
                return Synth("jump", 0.25f, t =>
                    0.35f * (1f - t / 0.25f) * Mathf.Sin(TAU * (300f * t + 0.5f * 2800f * t * t)), false);
            case Sfx.Slide:
                {
                    float prev = 0f;
                    return Synth("slide", 0.4f, t =>
                    {
                        float noise = (float)(rnd.NextDouble() * 2.0 - 1.0);
                        prev += (noise - prev) * 0.25f;
                        float env = Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / 0.4f));
                        return prev * env * 1.4f;
                    }, false);
                }
            case Sfx.Lane:
                {
                    float prev = 0f;
                    return Synth("lane", 0.12f, t =>
                    {
                        float noise = (float)(rnd.NextDouble() * 2.0 - 1.0);
                        prev += (noise - prev) * 0.35f;
                        return prev * Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / 0.12f)) * 1.2f;
                    }, false);
                }
            default: // Revive
                return Synth("revive", 0.8f, t =>
                {
                    float[] notes = { 392f, 523.25f, 659.25f, 783.99f, 1046.5f };
                    int idx = Mathf.Min(4, (int)(t / 0.1f));
                    float lt = t - idx * 0.1f;
                    return 0.4f * Mathf.Exp(-lt * 7f) * Mathf.Sin(TAU * notes[idx] * t);
                }, false);
        }
    }

    /// <summary>8 seconds of seamless beach waves.</summary>
    static AudioClip MakeAmbience()
    {
        System.Random rnd = new System.Random(21);
        float prev = 0f;
        return Synth("beach_waves", 8f, t =>
        {
            float noise = (float)(rnd.NextDouble() * 2.0 - 1.0);
            prev += (noise - prev) * 0.07f;
            float swell = 0.5f + 0.5f * Mathf.Sin(TAU * t / 8f);
            float env = 0.25f + 0.75f * swell * swell;
            return prev * env * 3.2f;
        }, true);
    }

    /// <summary>Synthwave loop: Am - F - C - G, kick, snare, hats, arpeggio, pad and bass (about 8 s).</summary>
    static AudioClip MakeMusic()
    {
        const float bpm = 118f;
        float beat = 60f / bpm, bar = beat * 4f;
        int bars = 4;
        float len = bar * bars;

        int[] bassMidi = { 33, 29, 36, 31 };
        int[][] chords = { new[] { 57, 60, 64 }, new[] { 53, 57, 60 }, new[] { 60, 64, 67 }, new[] { 55, 59, 62 } };
        float[] bassF = new float[4];
        float[][] chordF = new float[4][];
        float[][] padF = new float[4][];
        for (int i = 0; i < 4; i++)
        {
            bassF[i] = Freq(bassMidi[i]);
            chordF[i] = new float[3];
            padF[i] = new float[3];
            for (int j = 0; j < 3; j++) { chordF[i][j] = Freq(chords[i][j]); padF[i][j] = Freq(chords[i][j] - 12); }
        }
        float[] octaveUp = { 1f, 2f };
        int[] arpOrder = { 0, 1, 2, 1 };
        System.Random rnd = new System.Random(11);

        return Synth("miami_music", len, t =>
        {
            int ci = Mathf.Min(bars - 1, (int)(t / bar));
            float tb = t - ci * bar;

            float eighth = beat * 0.5f;
            float tn = tb % eighth;
            float bass = 0.20f * Mathf.Exp(-tn * 5f) * (Saw(bassF[ci] * t) * 0.6f + Mathf.Sin(TAU * bassF[ci] * t) * 0.6f);

            float sixteenth = beat * 0.25f;
            int step = (int)(tb / sixteenth);
            float tn2 = tb % sixteenth;
            float af = chordF[ci][arpOrder[step % 4]] * octaveUp[(step / 4) % 2];
            float arp = 0.085f * Mathf.Exp(-tn2 * 9f) * Saw(af * t);

            float trem = 0.8f + 0.2f * Mathf.Sin(TAU * 0.25f * t);
            float pad = 0f;
            for (int j = 0; j < 3; j++) pad += Mathf.Sin(TAU * padF[ci][j] * t);
            pad *= 0.028f * trem;

            float tk = tb % beat;
            float kick = 0.42f * Mathf.Exp(-tk * 12f) * Mathf.Sin(TAU * (45f + 90f * Mathf.Exp(-tk * 25f)) * tk);

            float th = (tb + beat * 0.5f) % beat;
            float hat = 0.07f * Mathf.Exp(-th * 70f) * (float)(rnd.NextDouble() * 2.0 - 1.0);

            float ts = (tb - beat) % (beat * 2f);
            if (ts < 0f) ts += beat * 2f;
            float snare = 0.16f * Mathf.Exp(-ts * 18f) * (float)(rnd.NextDouble() * 2.0 - 1.0);

            float x = bass + arp + pad + kick + hat + snare;
            return x / (1f + Mathf.Abs(x));   // soft clip
        }, true);
    }
}
