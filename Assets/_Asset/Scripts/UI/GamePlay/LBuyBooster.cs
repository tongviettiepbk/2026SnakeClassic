using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class LBuyBooster : BaseUI
{
    public Button btBackground;
    public Button btClose;
    public Button btBuy;
    public Button btAds;
    public TMP_Text txTitle;
    public TMP_Text txDescription;
    public TMP_Text txPrice;
    public Image icBooster;
    private ItemType type;
    private int valueReceive = 1;
    private int price = 0;
    protected override void Awake()
    {
        btBackground.onClick.AddListener(Back);
        btClose.onClick.AddListener(Back);
        btBuy.onClick.AddListener(ClickBtBuy);
        btAds.onClick.AddListener(ClickBtAds);

    }

    public void Open(ItemType type)
    {
        this.type = type;
        var itemData = GameData.staticData.items.GetData(type);
        price = itemData.priceByGold * valueReceive;
        double goldHave = ItemUtils.GetQuantityHave(ItemType.GOLD);
        if (itemData != null)
        {
            icBooster.sprite = itemData.icon;
            txTitle.text = itemData.GetName();
            txDescription.text = itemData.shortDescription;
            txPrice.text = price.ToString();
        }
    }

    private void ClickBtBuy()
    {
        float goldHave = ItemUtils.GetQuantityHave(ItemType.GOLD);
        if (goldHave >= price)
        {
            PurchaseDone(valueReceive);
            ItemUtils.Consume(ItemType.GOLD, price);
            Close();
        }
        else
        {
            UIManager.Instance.LoadUI(UIKey.SHOP);
            Close();
        }
    }

    private void ClickBtAds()
    {
        //#if UNITY_EDITOR
        //        RefillDone();
        //#else
        MediationAds.Instance.ShowRewardedVideoAd(GameConfig.ADS_PLACEMENT_BUY_BOOSTER, (result) =>
        {
            if (result == ShowResult.Finished)
            {
                PurchaseDone(1);
                Close();
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
        //#endif
    }

    private void PurchaseDone(int quantity)
    {
        var itemData = GameData.staticData.items.GetData(type);
        ItemUtils.Receive(type, quantity);
        LGamePlay.Instance.ReLoadFooter();
    }


}