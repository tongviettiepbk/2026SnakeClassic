using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;


public class LLose : BaseUI
{
    public GameObject objLose;
    public Button btReplay;
    public Button btClose;
    public TMP_Text txLevel;
    public TMP_Text txProgress;

    public Image fore;
    private List<RewardData> rewards;

    private bool loadLobby = false;
    private bool loadGame = false;
    protected override void Awake()
    {
        base.Awake();
        btReplay.onClick.AddListener(ClickBtReplay);
        btClose.onClick.AddListener(ClickBtClose);
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

    public void Open()
    {
        CheckProgress();
        string format = LocalizeManager.Instance.GetLocalizeText("Level ");
        int level = MapController.Instance.levelCurrent;
        txLevel.text = string.Format(format + level);
        objLose.gameObject.SetActive(true);
    }

    private void ClickBtReplay()
    {
        MediationAds.Instance.RequestShowInter(GameConfig.placementInterEndGame, () =>
        {
            UIManager.Instance.FadeToLoadScene(GameConfig.SCENE_GAME, () =>
            {
                UIManager.Instance.ClearAllUI();
            });
        });

    }

    private void ClickBtClose()
    {
        AudioManager.Instance.ResumeMusic();

        MediationAds.Instance.RequestShowInter(GameConfig.placementInterEndGame, () =>
        {
            UIManager.Instance.FadeToLoadScene(GameConfig.SCENE_LOBBY, () =>
            {
                UIManager.Instance.ClearAllUI();
            });
        });
    }

    private void CheckProgress()
    {
        float progress = MapController.Instance.GetProgressPercent(); // 0 → 1
        int targetPercent = Mathf.RoundToInt(progress * 100);

        string format = LocalizeManager.Instance.GetLocalizeText("Game Progress: ");

        int current = 0;

        txProgress.text = string.Format(format + " <color=#FFD700>{0}%</color>", 0);
        fore.fillAmount = 0f;

        DOTween.Kill(txProgress);
        DOTween.Kill(fore);

        DOTween.To(
            () => current,
             x =>
                {
                    current = x;
                    txProgress.text = string.Format(format + " <color=#FFD700>{0}%</color>", current);
                }, targetPercent, 1f)
            .SetTarget(txProgress)
            .SetEase(Ease.Linear);

        // FILL IMAGE
        fore
            .DOFillAmount(progress, 1f)
            .SetEase(Ease.Linear)
            .SetTarget(fore);
    }


}
