using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LOutOfTime : BaseUI
{
    [Space(20)]
    public Button btClose;
    public Button btRefill;
    public Button btAds;
    public TMP_Text txPrice;
    public TMP_Text txtTimeBonus;

    protected override void Awake()
    {
        btClose.onClick.AddListener(Back);
        btRefill.onClick.AddListener(ClickBtRefill);
        btAds.onClick.AddListener(ClickBtAds);
        txPrice.text = GameConfig.priceRefillTime.ToString();
        txtTimeBonus.text = "+" + GameConfig.timeRefillTime.ToString() + "s";
    }

    protected override void OnEnable()
    {
        AudioManager.Instance.PlayOutOfTime();
    }

    public override void Back()
    {
        Close();
        GameData.userData.profile.EndStage(false);
        LLose ui = UIManager.Instance.LoadUI(UIKey.LOSE, isPauseMusic: true) as LLose;
        ui.Open();
        MapController.Instance.SetEndGame(false);
    }

    private void ClickBtRefill()
    {
        float goldHave = ItemUtils.GetQuantityHave(ItemType.GOLD);
        if (goldHave > GameConfig.priceRefillTime)
        {
            ItemUtils.Consume(ItemType.GOLD, (long)GameConfig.priceRefillTime);
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
        //#if UNITY_EDITOR
        //        RefillDone();
        //#else
        MediationAds.Instance.ShowRewardedVideoAd(GameConfig.ADS_PLACEMENT_TIMEOUT, (result) =>
        {
            if (result == ShowResult.Finished)
            {
                RefillDone();
            }
            else
            {

            }
        });
        //#endif

    }

    private void RefillDone()
    {
        LGamePlay.Instance.RefillTime(GameConfig.timeRefillTime);
        Close();
    }
}
