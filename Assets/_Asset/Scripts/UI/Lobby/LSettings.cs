using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LSettings : BaseUI
{

    public Button btClose;
    public Button bgBackgroud;
    public Button btRestore;
    public Toggle btMusic;
    public Toggle btSound;
    public Toggle btVibration;



    protected override void Awake()
    {
        btClose.onClick.AddListener(Back);
        bgBackgroud.onClick.AddListener(Back);

        btMusic.onValueChanged.AddListener(OnToggleMusic);
        btSound.onValueChanged.AddListener(OnToggleSound);
        btVibration.onValueChanged.AddListener(OnToggleVibration);


       
    }

    private void Start()
    {
        btMusic.isOn = !AudioManager.Instance.isMuteMusic;
        btSound.isOn = !AudioManager.Instance.isMuteSfx;
        btVibration.isOn = HapticController.Instance.isAvaiableVibration();

    }

    protected override void OnEnable()
    {
        try
        {
#if UNITY_IOS
        btRestore.gameObject.SetActive(true);
#else
            btRestore.gameObject.SetActive(false);
#endif
        }
        catch
        {

        }

        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.isIgnoreSwipe = true;
        }


    }

    protected override void OnDisable()
    {
        base.OnDisable();


        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.isIgnoreSwipe = false;
        }
    }

    private void OnToggleMusic(bool isOn)
    {
        AudioManager.Instance.SetOffMucsic(!isOn);
    }

    private void OnToggleSound(bool isOn)
    {
        AudioManager.Instance.SetOffSoundFx(!isOn);
    }

    private void OnToggleVibration(bool isOn)
    {
        HapticController.Instance.SetOffHaptic(!isOn);
    }


}
