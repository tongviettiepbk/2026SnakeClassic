using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class CellShopGold : MonoBehaviour
{
    public int goldReceive;
    public float priceUsd;

    public TMP_Text txGoldReceive;
    public TMP_Text txPrice;

    public Button btPurchase;

    private int index;
    private string productId;

    private void Awake()
    {
        btPurchase.onClick.AddListener(ClickBtPurchase);
    }

    public void Initialize(int index)
    {
        this.index = index;
        txGoldReceive.text = goldReceive.ToString();
        txPrice.text = string.Format("${0}", priceUsd);

        switch (index)
        {
            case 0: productId = ProductDefine.GOLD_1000.productId; break;
            case 1: productId = ProductDefine.GOLD_5000.productId; break;
            case 2: productId = ProductDefine.GOLD_10000.productId; break;
            case 3: productId = ProductDefine.GOLD_25000.productId; break;
            case 4: productId = ProductDefine.GOLD_50000.productId; break;
            case 5: productId = ProductDefine.GOLD_100000.productId; break;
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
        int goldFinal = goldReceive;

        //if (index > 0 && isFirstPurchased == false)
        //{
        //    gemFinal *= 2;
        //}

        RewardData reward = new RewardData(ItemType.GOLD, goldFinal);
        ItemUtils.Receive(reward);
        Reload();
    }
}
