using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class LRevival : BaseUI
{
    public GameObject objContentAll;

    [Space(20)]
    public Button btClose;
    public Button btRefill;
    public Button btAds;
    public TMP_Text txPrice;

    protected override void Awake()
    {
        btClose.onClick.AddListener(ClickBtCLose);
        btAds.onClick.AddListener(ClickBtAds);
        btRefill.onClick.AddListener(ClickBtRefill);
        txPrice.text = GameConfig.priceRevival.ToString();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SimpleEventManager.Instance.PostEvent(EventIDSimple.pauseGameUI);

        this.StartDelayAction(1f, () => {
            objContentAll.SetActive(true);
        });
    }

    protected override void OnDisable()
    {
        SimpleEventManager.Instance.PostEvent(EventIDSimple.unPauseGameUI);

        objContentAll.SetActive(false);
    }

    private void ClickBtCLose()
    {
        Close();
        GameData.userData.profile.EndStage(false);
        LLose ui = UIManager.Instance.LoadUI(UIKey.LOSE) as LLose;
        AudioManager.Instance.PauseMusic();
        ui.Open();
        MapController.Instance.SetEndGame(false);
    }

    private void ClickBtRefill()
    {
        float goldHave = ItemUtils.GetQuantityHave(ItemType.GOLD);
        if (goldHave >= GameConfig.priceRevival)
        {
            ItemUtils.Consume(ItemType.GOLD, (long)GameConfig.priceRevival);
            RefillDone();
        }
        else
        {
            UIManager.Instance.ShowToastMessage(ToastMessageType.INSUFFICIENT_RESOURCES);
            UIManager.Instance.LoadUI(UIKey.SHOP);
        }
    }

    private void ClickBtAds()
    {

        MediationAds.Instance.ShowRewardedVideoAd("free_refill", (result) =>
        {
            if (result == ShowResult.Finished)
            {
                RefillDone();
            }
            else if (result == ShowResult.Skipped)
            {
                UIManager.Instance.ShowToastMessage(GameConfig.WATCH_VIDEOADS_NEED_FULL);
            }
            else
            {
                UIManager.Instance.ShowToastMessage(GameConfig.WATCH_VIDEOADS_UNSUCCESS);
            }
        });

    }

    private void RefillDone()
    {
        LGamePlay.Instance.Revival(60, 3);
        Close();
    }

}
