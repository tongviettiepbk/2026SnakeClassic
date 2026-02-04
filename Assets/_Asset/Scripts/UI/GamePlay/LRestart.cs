using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LRestart : BaseUI
{
    public Button btClose;
    public Button btRefresh;
    public Button btBackgroud;

    private bool reloadGame = false;
    public Image bgEndGame;
    protected override void Awake()
    {
        base.Awake();
        btClose.onClick.AddListener(Back);
        btBackgroud.onClick.AddListener(Back);
        btRefresh.onClick.AddListener(ClickBtRefresh);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SimpleEventManager.Instance.PostEvent(EventIDSimple.pauseGameUI);
    }

    protected override void OnDisable()
    {
        SimpleEventManager.Instance.PostEvent(EventIDSimple.unPauseGameUI);
    }

    private void ClickBtRefresh()
    {
        if (MediationAds.Instance.IsAbleShowInter())
        {
            MediationAds.Instance.ShowInterstitialAd(GameConfig.ADS_PLACEMENT_REFRESH, (result) =>
            {
                UIManager.Instance.FadeToLoadScene(GameConfig.SCENE_GAME, () =>
                {
                    UIManager.Instance.ClearAllUI();
                });
            });
        }
        else
        {
            UIManager.Instance.FadeToLoadScene(GameConfig.SCENE_GAME, () =>
            {
                UIManager.Instance.ClearAllUI();
            });
        }

    }

    public override void EndClose()
    {
        base.EndClose();
    }

}