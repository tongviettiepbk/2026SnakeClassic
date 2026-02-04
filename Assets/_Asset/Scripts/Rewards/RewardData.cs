using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
[System.Serializable]
public class RewardData
{
    public ItemType type;
    public double quantity;


    [JsonIgnore] public bool isNormalItem => (int)type < 1000;

    public RewardData() { }

    public RewardData(ItemType type, double quantity)
    {
        this.type = type;
        this.quantity = quantity;
    }

    public RewardData Clone()
    {
        RewardData rw = new RewardData(type, quantity);

        return rw;
    }

    public Tuple<string, string> GetInfo()
    {
        string Name = string.Empty;
        string desc = string.Empty;

        ItemData itemData = GameData.staticData.items.GetData(type);
        if (itemData != null)
        {
            Name = itemData.GetName();
            desc = itemData.GetDescription();
        }
        else
        {
            switch (type)
            {
            }
        }

        Tuple<string, string> info = new Tuple<string, string>(Name, desc);
        return info;
    }


    public List<RewardData> GetChoiceRewards()
    {
        List<RewardData> rewards = new List<RewardData>();

        //    switch (type)
        //    {
        //        case ItemType.HERO_CARD: rewards = heroCard.GetPoolChoiceRewards(); break;
        //        case ItemType.TOWER_CARD: rewards = towerCard.GetPoolChoiceRewards(); break;
        //        case ItemType.GEAR_CHEST: rewards = gearChest.GetPoolChoiceRewards(); break;
        //    }

        return rewards;
    }
}
