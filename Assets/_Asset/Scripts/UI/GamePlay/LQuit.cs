using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class LQuit : BaseUI
{
    public Button btNo;
    public Button btClose;
    public Button btBackground;
    public Button btGiveUp;


    protected override void Awake()
    {
        btNo.onClick.AddListener(Back);
        btClose.onClick.AddListener(Back);
        btBackground.onClick.AddListener(Back);
        btGiveUp.onClick.AddListener(ClickBtGiveUp);
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

    private void ClickBtGiveUp()
    {
        GameData.userData.profile.EndStage(false);

        MediationAds.Instance.RequestShowInter(GameConfig.placementInterQuitGame, () =>
        {
            UIManager.Instance.FadeToLoadScene(GameConfig.SCENE_LOBBY, () =>
            {
                UIManager.Instance.ClearAllUI();
            });
        });


    }
}
