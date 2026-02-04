using System.Collections;
using UnityEngine;
using Lofelt.NiceVibrations;

public class
    HapticController : MonoBehaviour
{
    private IEnumerator ieVibration;

    private float duration;
    private float delay;

    // private
    private float countdown;

    private string keyHapticOff = "HapticOff";
    private bool isHapticOff = false;

    #region Singleton
    private static HapticController instance;

    public static HapticController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType<HapticController>();
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }

        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        isHapticOff = PlayerPrefs.GetInt(keyHapticOff, 0) == 1;
    }
    #endregion

    #region Set ON Off

    public void SetOffHaptic(bool isOff)
    {
        isHapticOff = isOff;

        int tempOff = isHapticOff ? 1 : 0;
        PlayerPrefs.SetInt(keyHapticOff, tempOff);
    }

    public bool isAvaiableVibration()
    {
        DebugCustom.ShowDebugColorRed("isAvaiableVibration",!isHapticOff);
        return !isHapticOff;
    }

    #endregion

    public void VibrateOne()
    {
        if (!isAvaiableVibration()) return;

#if UNITY_IOS || UNITY_ANDROID
        bool hapticSuppoered = DeviceCapabilities.isVersionSupported;
        if (hapticSuppoered)
        {
            HapticPatterns.PlayConstant(0.08f, 0.0f, 0.03f);
        }
#endif
    }

    #region Gecko Move

    IEnumerator IeMoveGecko;

    public void VibarateGeckoWave(float timeVibarate = 0.7f)
    {
        VibrateOne();

        return;
        if (IeMoveGecko != null)
        {
            StopCoroutine(IeMoveGecko);
        }
        IeMoveGecko = SnakeCrawlHaptic(timeVibarate);
        StartCoroutine(IeMoveGecko);
    }

    IEnumerator SnakeCrawlHaptic(float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            // rung nhẹ
            HapticPatterns.PlayConstant(
                amplitude: 0.08f,
                frequency: 0f,
                duration: 0.03f
            );

            // nghỉ
            yield return new WaitForSeconds(0.05f);

            t += 0.08f;
        }
    }

    #endregion
    public void VibrateMoveGecko(float timeMove = 0.7f)
    {
        if (!isAvaiableVibration()) return;

#if UNITY_IOS || UNITY_ANDROID
        bool hapticSuppoered = DeviceCapabilities.isVersionSupported;
        if (hapticSuppoered)
        {
            HapticPatterns.PlayConstant(0.1f, 0.1f, timeMove);
        }
#endif
    }



    public void VibrateGeckoExit()
    {
        if (!isAvaiableVibration()) return;

#if UNITY_IOS || UNITY_ANDROID
        bool hapticSuppoered = DeviceCapabilities.isVersionSupported;
        if (hapticSuppoered)
        {
            HapticPatterns.PlayConstant(0.02f, 0.0f, 0.03f);
        }
#endif
    }

    public void VibrateGeckoStun()
    {
        if (!isAvaiableVibration()) return;

#if UNITY_IOS || UNITY_ANDROID
        bool hapticSuppoered = DeviceCapabilities.isVersionSupported;
        if (hapticSuppoered)
        {
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);
        }
#endif
    }

    public void VibrateOneHard()
    {
        if (!isAvaiableVibration()) return;

#if UNITY_IOS || UNITY_ANDROID
        bool hapticSuppoered = DeviceCapabilities.isVersionSupported;
        if (hapticSuppoered)
        {
            HapticPatterns.PlayConstant(0.15f, 0.0f, 0.03f);
        }
#endif
    }

    public void StartVibrate(float duration, float delayFirst, float delayTurn)
    {
        if (!isAvaiableVibration()) return;
        this.duration = duration;
        this.delay = delayTurn;

        this.countdown = duration;

        StopVibrate();

        if (ieVibration == null)
        {
            ieVibration = WaitToVibrate(delayFirst);
            StartCoroutine(ieVibration);
        }
    }

    public void StopVibrate()
    {
        if (ieVibration != null) StopCoroutine(ieVibration);
        ieVibration = null;
    }

    IEnumerator WaitToVibrate(float delayFirst)
    {
        yield return new WaitForSeconds(delayFirst);
#if UNITY_IOS || UNITY_ANDROID
        while (countdown > 0)
        {
            VibrateOne();
            yield return new WaitForSeconds(delay);
            countdown -= delay;
        }
#endif
    }
}