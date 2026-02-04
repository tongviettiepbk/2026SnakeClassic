using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StaticShopData : List<ShopData>
{
    public StaticShopData()
    {
        List<ShopData> data = Resources.LoadAll<ShopData>("Scriptable Objects/Shops").ToList();
        AddRange(data);
    }
    public ShopData GetData(ShopId id)
    {
        for (int i = 0; i < this.Count; i++)
        {
            if (this[i].id == id)
            {
                return this[i];
            }
        }
        return null;
    }

    public ShopData GetPackage(ShopId id)
    {
        for (int i = 0; i < this.Count; i++)
        {
            ShopData data = this[i];
            if (data.id == id && data.packages != null && data.packages.Length > 0)
            {
                return data;
            }
        }
        return null;
    }
}

