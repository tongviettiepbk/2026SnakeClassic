using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class BoxBundle : MonoBehaviour
{

    public Button btPurchase;
    public TMP_Text txPrice;
    public TMP_Text txTitle;
    public TMP_Text txGoldReceive;
    public ShopPackageData shopPackageData;
    public List<CellReward> cells = new List<CellReward>();
    public UserShopData uShop;
    public List<RewardData> rewards = new List<RewardData>();

    private int index;
    private string productId;
    private PopUpBundle popupBundle;
    private RectTransform rect;

    private void Awake()
    {
        popupBundle = GetComponentInParent<PopUpBundle>();
        rect = GetComponent<RectTransform>();
        btPurchase.onClick.AddListener(ClickBtPurchase);

    }

    public void Initialize(int index)
    {
        gameObject.SetActive(true);
        rect.anchoredPosition = Vector2.zero;
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one;

        //Load Reward ben trai

        txPrice.text = string.Format("${0}", shopPackageData.priceByUsd);
        txTitle.text = shopPackageData.title.ToString();

        switch (index)
        {
            case 0: productId = ProductDefine.BUNDLE_01.productId; break;
            case 1: productId = ProductDefine.BUNDLE_02.productId; break;
            case 2: productId = ProductDefine.BUNDLE_03.productId; break;
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

    public void OnSwipeLeft()
    {
        popupBundle.ShowNext();
    }

    public void OnSwipeRight()
    {
        popupBundle.ShowPrev();
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
        if (uShop == null)
        {
            return;
        }
        ItemUtils.Receive(rewards);
        uShop.PurchaseBundle((int)shopPackageData.id);
        if (popupBundle != null)
        {
            popupBundle.Reload();
        }
    }
}
