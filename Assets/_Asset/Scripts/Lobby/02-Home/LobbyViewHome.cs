using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyViewHome : LobbyView
{
    public Button btSetting;
    public Button btPlay;
    public Button btPopupRemoveAds;
    public TMP_Text txPlay;
    public List<BaseButtonLobby> buttons = new List<BaseButtonLobby>();
    protected override void Awake()
    {
        base.Awake();
        btSetting.onClick.AddListener(OnClickBtSetting);
        btPlay.onClick.AddListener(ClickBtPlay);
        btPopupRemoveAds.onClick.AddListener(ClickBtPopUpAds);
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.W))
        {
            SimpleEventManager.Instance.PostEvent(EventIDSimple.showBannerSucess);
        }
#endif
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        CheckActiveButtons();
        SimpleEventManager.Instance.Register(EventIDSimple.showBannerSucess, SetBanner);
        SimpleEventManager.Instance.Register(EventIDSimple.showBtRemoveAds, SetButton);
        SetPosYFooter();
    }

    private void OnDisable()
    {
        SimpleEventManager.Instance.Unregister(EventIDSimple.showBannerSucess, SetBanner);
        SimpleEventManager.Instance.Unregister(EventIDSimple.showBtRemoveAds, SetButton);

    }

    private void SetBanner(object param)
    {
        GameConfig.isAdsOn = true;
    }

    private void SetButton(object param)
    {
        btPopupRemoveAds.gameObject.SetActive(!GameData.userData.shops.isPurchaseRemoveAds);
    }

    private void SetPosYFooter()
    {

        if (GameConfig.isAdsOn)
        {
            RectTransform rt = btPlay.GetComponent<RectTransform>();
            Vector2 pos = rt.anchoredPosition;
            pos.y += GameConfig.posYBanner;
            rt.anchoredPosition = pos;
        }
    }

    public void CheckActiveButtons()
    {
        txPlay.text = GameData.userData.profile.currentStageId.ToString();

        for (int i = 0; i < buttons.Count; i++)
        {
            var button = buttons[i];
            button.CheckActive();
        }

        btPopupRemoveAds.gameObject.SetActive(!GameData.userData.shops.isPurchaseRemoveAds);
    }

    public override void CheckNotify()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            var button = buttons[i];
            if (button.gameObject.activeInHierarchy)
            {
                button.CheckNotify();
            }
        }
    }

    private void ClickBtPlay()
    {
        SceneManager.LoadScene(GameConfig.SCENE_GAME);
        //UIManager.Instance.LoadUI(UIKey.STAGE);
    }

    private void OnClickBtSetting()
    {
        UIManager.Instance.LoadUI(UIKey.SETTING);
    }

    private void ClickBtPopUpAds()
    {
        UIManager.Instance.LoadUI(UIKey.ADS);
    }
}
