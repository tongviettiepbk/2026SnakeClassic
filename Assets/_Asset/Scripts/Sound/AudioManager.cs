using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager>
{
    public AudioSource audioSourceMusic;
    public AudioSource audioSourceSfx;

    [Space(20)]
    [Header("AudioClip")]
    public AudioClip bgAudio;
    public AudioClip sfxButtonClick;

    [Space(10)]
    public AudioClip sfxClickGecko;

    public AudioClip sfxMoveFail;
    public AudioClip sfxMoveSucess;
    public AudioClip sfxMove;
    public AudioClip sfxWin;
    public AudioClip sfxLose;
    public AudioClip sfxFireWork;

    public AudioClip sfxFreezeStart;
    public AudioClip sfxFreezeExploder;

    public AudioClip sfxHummerRotation;
    public AudioClip sfxHammerCollider;

    public AudioClip sfxOpenReward;
    public AudioClip sfxBreakBrick;
    public AudioClip sfxBreakCrack;
    public AudioClip sfxExitHole;
    public AudioClip sfxCoinCount;

    [Space(20)]
    [Header("Popup")]

    public AudioClip sfxOpenPopup;
    public AudioClip sfxClosePoup;
    public AudioClip sfxPopupIcon;
    public AudioClip sfxCoinAppear;
    public AudioClip sfxOutOfTime;
    public AudioClip sfxWarningTime;

    private float musicPauseTime = 0f;
    private bool isMusicPaused = false;
    [Header("MIX SOUNDS")]
    public AudioClip[] takeCritClips;

    [Header("SFX Pool")]
    public int poolSize = 10;
    private List<AudioSource> sfxPool = new List<AudioSource>();
    private Dictionary<string, AudioSource> loopSfxDict = new Dictionary<string, AudioSource>();

    // Giới hạn số lượng SFX đang chơi theo từng clip
    private Dictionary<AudioClip, int> playingLimitCounter = new Dictionary<AudioClip, int>();

    private Dictionary<string, AudioClip> musicCollection = new Dictionary<string, AudioClip>();

    private List<string> mixBuffer = new List<string>();
    private float mixBufferClearDelay = 0.05f;

    private float MAX_VOLUME = 1f;
    private string keyDataMuteSfx = "is_sfx_on";
    private string keyDataMuteMusic = "is_music_on";
    private string keyDataVolumeSfx = "volume_sfx";
    private string keyDataVolumeMusic = "volume_music";
    private string pathMusic = "Audio/Music/";

    private string keyWarningTime = "sfxWarningTime";

    public AudioClip currentTrack { get; set; }
    public bool isMuteMusic { get; set; }
    public bool isMuteSfx { get; set; }
    public float volumeMusic
    {
        get
        {
            return PlayerPrefs.GetFloat(keyDataVolumeMusic, 1f);
        }
        set
        {
            PlayerPrefs.SetFloat(keyDataVolumeMusic, value);
        }
    }
    public float volumeSfx
    {
        get
        {
            return PlayerPrefs.GetFloat(keyDataVolumeSfx, 1f);
        }
        set
        {
            PlayerPrefs.SetFloat(keyDataVolumeSfx, value);
        }
    }

    protected void Awake()
    {
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        Initialize();
        CreateSfxPool();
        PlayMusicBg();
    }

    private void Initialize()
    {
        isMuteMusic = PlayerPrefs.GetInt(keyDataMuteMusic, 0) == 1;
        isMuteSfx = PlayerPrefs.GetInt(keyDataMuteSfx, 0) == 1;

        volumeSfx = volumeSfx;
        volumeMusic = volumeMusic;

        ChangeSfxVolume(volumeSfx);
        ChangeMusicVolume(volumeMusic);

        StartCoroutine(MixBufferRoutine());
    }

    public void Mute(bool isMute)
    {
#if !UNITY_EDITOR
        audioSourceSfx.mute = isMute;
        audioSourceMusic.mute = isMute;
#endif
    }

    public void SetOffSoundFx(bool isOff)
    {
        DebugCustom.ShowDebug("isMuteSfx", isOff);
        isMuteSfx = isOff;
        if (isOff)
            StopSfx();

        int tempOff = isOff ? 1 : 0;
        PlayerPrefs.SetInt(keyDataMuteSfx, tempOff);

    }
    public void SetOffMucsic(bool isOff)
    {
        DebugCustom.ShowDebug("IsMuteMusic", isOff);

        isMuteMusic = isOff;

        if (isMuteMusic)
            StopMusic();
        else
            PlayMusicBg();

        int tempOff = isOff ? 1 : 0;
        PlayerPrefs.SetInt(keyDataMuteMusic, tempOff);
    }


    #region SOUND

    public void StopSfx()
    {
        audioSourceSfx.Stop();
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip == null)
            return;
        if (isMuteSfx)
            return;

        if (clip && !mixBuffer.Contains(clip.name))
        {
            mixBuffer.Add(clip.name);
            audioSourceSfx.PlayOneShot(clip);
        }
    }

    public void ChangeSfxVolume(float v)
    {
        volumeSfx = v;
        audioSourceSfx.volume = volumeSfx;
    }

    private IEnumerator MixBufferRoutine()
    {
        float time = 0;

        while (true)
        {
            time += Time.unscaledDeltaTime;
            yield return 0;

            if (time >= mixBufferClearDelay)
            {
                mixBuffer.Clear();
                time = 0;
            }
        }
    }

    #endregion

    #region  Play sound, music
    public void PlaySfxClick()
    {
        if (!isMuteSfx)
        {
            DebugCustom.ShowDebugColorRed("Play sound1");
            audioSourceSfx.PlayOneShot(sfxButtonClick);
        }

    }

    //sound
    public void PlayMusicBg()
    {
        PlayMusic(bgAudio);
    }

    #endregion

    #region MUSIC

    private AudioClip GetMusicClip(string name)
    {
        if (musicCollection.ContainsKey(name))
        {
            return musicCollection[name];
        }
        else
        {
            AudioClip clip = Resources.Load<AudioClip>(pathMusic + name);

            if (clip != null)
            {
                musicCollection.Add(name, clip);
                return clip;
            }
            else
            {
                DebugCustom.LogWarning("Music clip not found=" + name);
                return null;
            }
        }
    }

    public void PlayMusic(string musicClipName, bool isLoop = true, float volume = 1f)
    {
        AudioClip musicBG = GetMusicClip(musicClipName);
        CrossFadePlayMusic(musicBG, isLoop, volume);
    }

    public void PlayMusic(AudioClip clip, bool isLoop = true, float volume = 1f)
    {
        CrossFadePlayMusic(clip, isLoop, volume);
    }

    public void StopMusic()
    {
        PlayMusic(clip: null);
    }

    public void PauseMusic()
    {
        if (audioSourceMusic.clip == null)
            return;
        if (!audioSourceMusic.isPlaying)
            return;
        musicPauseTime = audioSourceMusic.time;
        isMusicPaused = true;

        audioSourceMusic.Pause();
    }

    public void ResumeMusic()
    {
        if (audioSourceMusic.clip == null)
            return;
        if (!isMusicPaused)
            return;
        if (isMuteMusic)
            return;

        audioSourceMusic.time = musicPauseTime;
        audioSourceMusic.Play();

        isMusicPaused = false;
    }


    private void CrossFadePlayMusic(AudioClip clip, bool isLoop, float volume)
    {
        audioSourceMusic.loop = isLoop;
        volumeMusic = volume;
        StartCoroutine(Instance.CrossFade(clip, volume));
    }

    public void ChangeMusicVolume(float v)
    {
        volumeMusic = v;
        audioSourceMusic.volume = volumeMusic * MAX_VOLUME;
    }

    private IEnumerator CrossFade(AudioClip to, float volume, float delay = 0.3f)
    {
        if (to != null)
        {
            currentTrack = to;
        }

        if (audioSourceMusic.clip != null)
        {
            while (delay > 0)
            {
                audioSourceMusic.volume = delay * volumeMusic * MAX_VOLUME;
                delay -= Time.unscaledDeltaTime;
                yield return 0;
            }
        }

        audioSourceMusic.clip = to;

        if (to == null || isMuteMusic)
        {
            audioSourceMusic.Stop();
            yield break;
        }

        delay = 0;

        if (!audioSourceMusic.isPlaying)
        {
            audioSourceMusic.Play();
        }

        while (delay < 0.3f)
        {
            audioSourceMusic.volume = delay * volumeMusic * MAX_VOLUME;
            delay += Time.unscaledDeltaTime;
            yield return 0;
        }

        audioSourceMusic.volume = volumeMusic * MAX_VOLUME;
    }

    public void PlayMusic()
    {

    }

    public void PlayMoveFail()
    {
        PlaySfx(sfxMoveFail);
    }

    public void PlayGeckoMove()
    {
        //PlaySfx(sfxMoveSucess);
       PlaySfxMultiple(sfxMove);
    }

    public void PlayWin()
    {
        PlaySfx(sfxWin);
    }

    public void PlayLose()
    {
        PlaySfx(sfxLose);
    }

    public void PlayFirework()
    {
        PlaySfx(sfxFireWork);
    }

    public void PlayFreezeStart()
    {
        PlaySfx(sfxFireWork);
    }

    public void PlayFreezeExploder()
    {
        PlaySfx(sfxFreezeExploder);
    }

    public void PlayHummerRotation()
    {
        PlaySfx(sfxHummerRotation);
    }

    public void PlayHummerCollision()
    {
        PlaySfx(sfxHammerCollider);
    }

    public void PlayOpenReward()
    {
        PlaySfx(sfxOpenReward);
    }

    public void PlayBreakBrick()
    {
        PlaySfx(sfxBreakBrick);
    }

    public void PlayOpenPopup()
    {
        PlaySfx(sfxOpenPopup);
    }

    public void PlayClosePopup()
    {
        PlaySfx(sfxClosePoup);
    }

    public void PlayWarningTime()
    {
        PlayLoopSfx(keyWarningTime, sfxWarningTime);
    }

    public void StopWarningTime()
    {
        StopLoopSfx(keyWarningTime);
    }

    public void PlayCoinAppear()
    {
        //PlaySfx(sfxCoinAppear);
        PlaySfxMultiple(sfxCoinAppear);
    }

    public void PlayExitHole()
    {
        PlaySfxMultiple(sfxExitHole);
    }

    public void PlayOutOfTime()
    {
        PlaySfx(sfxOutOfTime);
    }

    public void PlayCoinCount()
    {
        PlaySfxMultiple(sfxCoinCount);
    }

    #endregion

    #region Update Task

    public void ClickGecko()
    {
        //DebugCustom.ShowDebug("ClickGecko sound");
        PlaySfxMultiple(sfxClickGecko);
    }

    private void CreateSfxPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            AudioSource s = gameObject.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.loop = false;
            s.volume = volumeSfx;
            sfxPool.Add(s);
        }
    }

    private AudioSource GetFreeSfxSource()
    {
        foreach (var s in sfxPool)
        {
            if (!s.isPlaying)
                return s;
        }

        // Nếu hết source -> tạo thêm (auto expand)
        AudioSource extra = gameObject.AddComponent<AudioSource>();
        extra.playOnAwake = false;
        extra.loop = false;
        extra.volume = volumeSfx;
        sfxPool.Add(extra);
        return extra;
    }

    public void PlaySfxMultiple(AudioClip clip)
    {
        if (clip == null) return;
        if (isMuteSfx) return;

        AudioSource free = GetFreeSfxSource();
        free.volume = volumeSfx;
        free.loop = false;
        free.PlayOneShot(clip);
    }

    public void PlayLoopSfx(string key, AudioClip clip)
    {
        if (clip == null) return;
        if (loopSfxDict.ContainsKey(key)) return;   // đã chạy rồi

        AudioSource s = gameObject.AddComponent<AudioSource>();
        s.clip = clip;
        s.loop = true;
        s.volume = volumeSfx;
        s.Play();

        loopSfxDict.Add(key, s);
    }

    public void StopLoopSfx(string key)
    {
        if (!loopSfxDict.ContainsKey(key)) return;

        AudioSource s = loopSfxDict[key];
        s.Stop();
        Destroy(s);

        loopSfxDict.Remove(key);
    }

    #endregion

    #region Play Limited 

    public void PlayLimitedSfx(AudioClip clip, int maxCount)
    {
        if (clip == null) return;
        if (isMuteSfx) return;

        // Lấy số lượng hiện tại
        int current = 0;
        playingLimitCounter.TryGetValue(clip, out current);

        // Nếu vượt giới hạn → không play
        if (current >= maxCount)
            return;

        // Tăng counter
        playingLimitCounter[clip] = current + 1;

        AudioSource free = GetFreeSfxSource();
        free.volume = volumeSfx;
        free.loop = false;
        free.clip = clip;

        // Khi SFX chơi xong → giảm counter
        StartCoroutine(ReduceCounterWhenDone(free, clip));

        free.Play();
    }

    private IEnumerator ReduceCounterWhenDone(AudioSource source, AudioClip clip)
    {
        yield return new WaitWhile(() => source.isPlaying);

        if (playingLimitCounter.ContainsKey(clip))
        {
            playingLimitCounter[clip]--;
            if (playingLimitCounter[clip] <= 0)
                playingLimitCounter.Remove(clip);
        }
    }


    #endregion
}
