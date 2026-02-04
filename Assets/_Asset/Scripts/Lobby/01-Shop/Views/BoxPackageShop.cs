using NSubstitute.Core;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;


public class BoxPackageShop : MonoBehaviour
{
    public TMP_Text txTitle;
    public TMP_Text txPrice;
    public ScrollRect scollRect;
    public CellReward cellPrefabs;
    public CellReward cellRewardLeft;
    public Button btBuy;

    public List<CellReward> cellRewardRight;

    private int index = 0;
    private string productId;
    private ShopPackageData package = null;
    private UserShopData uShop = null;

    public List<RewardData> rewards = new List<RewardData>();

    private void Awake()
    {
        btBuy.onClick.AddListener(ClickBtPurchase);
    }

    public void Initialize(int index)
    {
        this.index = index;

        //Load Reward ben trai

        ShopData data = GameData.staticData.shops.GetPackage(ShopId.SHOP);
        package = data.packages[index];

        if (package.isLocked)
        {
            gameObject.SetActive(false);
            return;
        }
        if (data == null)
        {
            return;
        }
        txTitle.text = package.title.ToString();
        cellRewardLeft.Load(package.rewards[0]);

        txPrice.text = string.Format("${0}", package.priceByUsd);

        switch (index)
        {
            case 0: productId = ProductDefine.MINI_PACK.productId; break;
            case 1: productId = ProductDefine.MEDIUM_PACK.productId; break;
            case 2: productId = ProductDefine.SUPER_PACK.productId; break;
            case 3: productId = ProductDefine.CHAMPION_PACK.productId; break;
            case 4: productId = ProductDefine.GOD_PACK.productId; break;
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

        // Load Reward ben trai 
        for (int i = 1; i < package.rewards.Count; i++)
        {
            if (cellRewardRight.Count < package.rewards.Count - 1)
            {
                CellReward cell = Instantiate(cellPrefabs, scollRect.content);
                cell.Load(package.rewards[i], false, false);
                cellRewardRight.Add(cell);
            }
        }
        Reload();
    }

    private void Reload()
    {
        uShop = GameData.userData.shops;

        if (uShop.isPurchaseRemoveAds && package.id == ShopPackageId.REMOVE_ADS)
        {
            gameObject.SetActive(false);
            return;
        }

    }

    private void ClickBtPurchase()
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

    private void PurchaseDone()
    {
        List<RewardData> rewards = new List<RewardData>();
        ShopData data = GameData.staticData.shops.GetPackage(ShopId.SHOP);
        if (uShop != null && uShop.isPurchaseRemoveAds && package.id == ShopPackageId.REMOVE_ADS)
        {
            uShop.PurchaseRemoveAds();
        }
        for (int i = 0; i < package.rewards.Count; i++)
        {
            var reward = package.rewards[i];
            rewards.Add(reward);
        }

        ItemUtils.Receive(rewards);
        Reload();
    }
}
