using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LPause : LSettings
{
    public Button btQuit;
    public Button btResume;

    [Space(20)]
    public Button btHack;
    public Button btHideUi;
    public Button btGoLv;
    public TMP_InputField inputLevel;

    protected override void Awake()
    {
        base.Awake();
        btQuit.onClick.AddListener(ClickBtQuit);
        btResume.onClick.AddListener(Back);

        try
        {
            btHack.onClick.AddListener(ClickBtHack);
            btHideUi.onClick.AddListener(ClickHideUi);
            btGoLv.onClick.AddListener(ClickBtGoLv);

            btHack.gameObject.SetActive(false);
            btHideUi.gameObject.SetActive(false);
            btGoLv.gameObject.SetActive(false);
            inputLevel.gameObject.SetActive(false);
        }
        catch
        {

        }
    }

    protected override void OnEnable()
    {
        AudioManager.Instance.PlayOpenPopup();
        SimpleEventManager.Instance.PostEvent(EventIDSimple.pauseGameUI);

        // Hack for video
        btHack.gameObject.SetActive(false);
        if (MapController.Instance != null)
        {
            if (MapController.Instance.isHackInSetting)
            {
                btHack.gameObject.SetActive(true);
                btHideUi.gameObject.SetActive(true);
                btGoLv.gameObject.SetActive(true);
                inputLevel.gameObject.SetActive(true);
            }
        }

        if(GameConfig.lvMktOpenNeedVideo > 0)
        {
            inputLevel.text = GameConfig.lvMktOpenNeedVideo.ToString();
        }
    }

    protected override void OnDisable()
    {
        SimpleEventManager.Instance.PostEvent(EventIDSimple.unPauseGameUI);
    }

    public override void Back()
    {
        base.Back();
        LGamePlay.Instance.PlayAnim();
    }

    private void ClickBtQuit()
    {
        if (MapController.Instance.isStartCountTimeGame)
        {
            Close();
            UIManager.Instance.LoadUI(UIKey.QUIT);
        }
        else
        {
            MediationAds.Instance.RequestShowInter(GameConfig.placementInterQuitGame, () =>
            {
                UIManager.Instance.FadeToLoadScene(GameConfig.SCENE_LOBBY, () =>
                {
                    UIManager.Instance.ClearAllUI();
                });
            });
        }
    }

    private void ClickBtHack()
    {
        if (MapController.Instance != null)
        {
            MapController.Instance.SetEndGame(true);
        }
        Close();
    }

    public void ClickHideUi()
    {
        if (LGamePlay.Instance != null)
        {
            LGamePlay.Instance.HideCanvas();
        }

    }

    public void ClickBtGoLv()
    {
        string temp = inputLevel.text;
        if (string.IsNullOrEmpty(temp))
        {
            UIManager.Instance.ShowToastMessage("Nhap level muon choi!");
            return;
        }

        int tempInt = int.Parse(temp);
        GameConfig.lvMktOpenNeedVideo = tempInt;

        SceneManager.LoadScene(GameConfig.SCENE_GAME);
    }
}
