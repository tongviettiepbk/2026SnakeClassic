using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;


public class LAds : BaseUI
{
    [Space]
    public Button btClose;
    public ShopPackageData shopPackageData;
    public List<CellReward> cells = new List<CellReward>();
    public UserShopData uShop;
    public List<RewardData> rewards = new List<RewardData>();

    [Space]
    public Button btPurchase;
    public TMP_Text txPrice;
    public TMP_Text txtGoldRevice;

    private string productId;

    protected override void Awake()
    {
        btClose.onClick.AddListener(Back);
        btPurchase.onClick.AddListener(ClickBtPurchase);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        uShop = GameData.userData.shops;
        productId = ProductDefine.REMOVE_ADS.productId;
        SetLayout();

        if(LobbyManager.Instance != null)
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

    private void SetLayout()
    {
        txPrice.text = string.Format("${0}", shopPackageData.priceByUsd);

        if (string.IsNullOrEmpty(productId) == false)
        {
            try
            {
                ProductMetadata metaData = InAppPurchaseController.Instance.GetProduct(productId).metadata;
                txPrice.text = metaData.localizedPriceString;
            }
            catch { }
        }

        if (shopPackageData.rewards == null || shopPackageData.rewards.Count == 0)
        {
            // tắt toàn bộ cell
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].gameObject.SetActive(false);
            }

            return;
        }


        for (int i = 0; i < shopPackageData.rewards.Count; i++)
        {
            var reward = shopPackageData.rewards[i];
            rewards.Add(reward);

            if (txtGoldRevice != null && reward.type == ItemType.GOLD)
            {
                txtGoldRevice.text = reward.quantity.ToString();
            }
        }

        int maxLoad = Mathf.Min(rewards.Count - 1, cells.Count);

        for (int i = 0; i < maxLoad; i++)
        {
            cells[i].Load(rewards[i], showBgCell: false);
        }
    }

    public void ClickBtPurchase()
    {
#if UNITY_EDITOR
        PurchaseDone();
#else
                InAppPurchaseController.Instance.BuyProductID(productId, (product) =>
                       {
                           PurchaseDone();
                       });
#endif

    }

    public void PurchaseDone()
    {
        Back();

        uShop.PurchaseRemoveAds();
        ItemUtils.Receive(rewards);
        SimpleEventManager.Instance.PostEvent(EventIDSimple.showBtRemoveAds);

        if (LGamePlay.Instance != null)
            LGamePlay.Instance.ReLoadFooter();
    }
}
