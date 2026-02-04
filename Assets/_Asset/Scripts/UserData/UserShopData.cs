
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UserShopData : BaseUserData
{
    public Dictionary<int, bool> purchasedBundles = new Dictionary<int, bool>();

    public bool isPurchaseRemoveAds = false;
    protected override string GetDataKey()
    {
        return UserData.DATA_KEY_SHOP;
    }

    public override void ValidateData()
    {
        base.ValidateData();
    }

    public override void RefreshNewDay()
    {
        base.RefreshNewDay();
    }

    public void PurchaseRemoveAds()
    {
        isPurchaseRemoveAds = true;
        isDataChanged = true;
    }

    public void PurchaseBundle(int id)
    {
        if (purchasedBundles.ContainsKey(id))
        {
            purchasedBundles[id] = true;
        }
        else
        {
            purchasedBundles.Add(id, true);
        }
        isDataChanged = true;
    }
    public bool IsPurchased(int id)
    {
        return purchasedBundles.ContainsKey(id) && purchasedBundles[id];
    }
}
