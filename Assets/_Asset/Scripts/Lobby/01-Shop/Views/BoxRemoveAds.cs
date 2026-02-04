using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class BoxRemoveAds : MonoBehaviour
{
    public Button btPurchase;
    public TMP_Text txPrice;
    public TMP_Text txGoldReceive;
    public ShopPackageData shopPackageData;
    public List<CellReward> cells = new List<CellReward>();
    public UserShopData uShop;
    public List<RewardData> rewards = new List<RewardData>();

    private int index;
    private string productId;
    private LobbyViewShop view;
    private void Awake()
    {
        btPurchase.onClick.AddListener(ClickBtPurchase);
    }

    public void Initialize(int index, LobbyViewShop view)
    {
        this.index = index;
        this.view = view;
        txPrice.text = string.Format("${0}", shopPackageData.priceByUsd);
        switch (index)
        {
            case 0: productId = ProductDefine.REMOVE_ADS.productId; break;
            case 1: productId = ProductDefine.REMOVE_ADS_2.productId; break;
        }

        if (string.IsNullOrEmpty(productId) == false)
        {
            try
            {
                ProductMetadata metaData = InAppPurchaseController.Instance.GetProduct(productId).metadata;
                txPrice.text = metaData.localizedPriceString;
            }
            catch { }
        }

        Reload();
    }
    public void Reload()
    {
        uShop = GameData.userData.shops;
        rewards.Clear();

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

            if (txGoldReceive != null && reward.type == ItemType.GOLD)
            {
                txGoldReceive.text = reward.quantity.ToString();
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
        if (GameConfig.isHackInapp)
        {
            PurchaseDone();
            return;
        }

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
        if (uShop != null && !uShop.isPurchaseRemoveAds && shopPackageData.id == ShopPackageId.REMOVE_ADS)
        {
            uShop.PurchaseRemoveAds();
            ItemUtils.Receive(rewards);
            view.Reload();
        }
        else if (uShop != null && !uShop.isPurchaseRemoveAds && shopPackageData.id == ShopPackageId.REMOVE_ADS_2)
        {
            uShop.PurchaseRemoveAds();
            ItemUtils.Receive(rewards);
            view.Reload();
        }

        SimpleEventManager.Instance.PostEvent(EventIDSimple.showBtRemoveAds);
    }
}
