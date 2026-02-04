using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class StaticGameData
{
    public StaticItemData items;
    public StaticShopData shops;
    public void Load()
    {
        if (items == null) items = new StaticItemData();
        if (shops == null) shops = new StaticShopData();
    }
}
