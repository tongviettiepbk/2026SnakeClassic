using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ItemUtils
{
    public const int STONE_FRAGMENT_COMBINE_QUANTITY = 100;
    public static long GetQuantityHave(ItemType type)
    {
        switch (type)
        {
            case ItemType.EXP_ACCOUNT: return GameData.userData.profile.exp;

            default:
                {
                    ItemData itemData = GameData.staticData.items.GetData(type);
                    if (itemData != null)
                    {
                        return GameData.userData.items.GetQuantityHave(type);
                    }
                    else
                    {
                        DebugCustom.LogError("ItemDataNull=" + type);
                    }
                }
                break;
        }

        return 0;
    }

    public static void Consume(ItemType type, long quantity)
    {
        if (quantity > 0)
        {
            GameData.userData.items.Consume(type, quantity);
            EventDispatcher.Instance.PostEvent(EventID.ADJUST_ITEM);
        }
    }

    public static List<RewardData> Receive(ItemType type, long quantity, bool showPopup = true, bool playSfx = true)
    {
       
        return Receive(new RewardData(type, quantity), showPopup, playSfx);
    }

    public static List<RewardData> Receive(RewardData reward, bool showPopup = true, bool playSfx = true, bool shortenRewards = true)
    {
        return Receive(new List<RewardData>() { reward }, showPopup, playSfx, shortenRewards);
    }

    public static List<RewardData> Receive(List<RewardData> rewards, bool showPopup = true, bool playSfx = true, bool shortenRewards = true)
    {
        DebugCustom.Log("[ItemUtils] Receive=" + JsonConvert.SerializeObject(rewards));
        if (rewards != null && rewards.Count > 0)
        {
            List<RewardData> finalItems = rewards.Identify();

            for (int i = 0; i < finalItems.Count; i++)
            {
                ReceiveItem(finalItems[i]);
            }

            List<RewardData> result = shortenRewards ? finalItems.Shorten() : finalItems;

            if (showPopup)
            {
                UIManager.Instance.ShowRewards(result, playSfx);
            }

            EventDispatcher.Instance.PostEvent(EventID.ADJUST_ITEM);
            return result;
        }

        return null;
    }

    private static void ReceiveItem(RewardData item)
    {
        if (item == null)
        {
            return;
        }

        int quantity = (int)item.quantity;

        try
        {
            switch (item.type)
            {
                case ItemType.EXP_ACCOUNT:
                    {
                        GameData.userData.profile.ReceiveExpAccount(quantity);
                    }
                    break;

                default:
                    {
                        GameData.userData.items.Receive(item.type, (long)item.quantity);
                    }
                    break;
            }
        }
        catch { }
    }

    #region Extensions
    public static List<RewardData> Identify(this List<RewardData> items)
    {
        List<RewardData> result = new List<RewardData>();

        for (int i = 0; i < items.Count; i++)
        {
            RewardData item = items[i];

            if (item != null)
            {
                List<RewardData> tmp = item.Identify();
                for (int j = 0; j < tmp.Count; j++)
                {
                    result.Add(tmp[j]);
                }
            }
        }

        return result;
    }

    public static List<RewardData> Identify(this RewardData _item)
    {
        if (_item == null)
        {
            return null;
        }

        RewardData item = _item.Clone();

        try
        {
            switch (item.type)
            {

            }
        }
        catch { }

        return new List<RewardData>() { item };
    }

    public static List<RewardData> Shorten(this List<RewardData> items)
    {
        List<RewardData> finalItems = new List<RewardData>();

        for (int i = 0; i < items.Count; i++)
        {
            RewardData item = items[i];

            if (item == null)
            {
                continue;
            }

            bool isDuplicate = false;

            for (int j = 0; j < finalItems.Count; j++)
            {
                RewardData rewardData = finalItems[j];

                if (item.type == rewardData.type)
                {
                    switch (item.type)
                    {
                        default:
                            {
                                isDuplicate = true;
                                rewardData.quantity += item.quantity;
                            }
                            break;

                    }
                }
            }

            if (isDuplicate == false)
            {
                finalItems.Add(item);
            }
        }

        return finalItems;
    }

    public static List<RewardData> Clone(this List<RewardData> rewards)
    {
        List<RewardData> newRewards = new List<RewardData>();

        for (int i = 0; i < rewards.Count; i++)
        {
            newRewards.Add(rewards[i].Clone());
        }

        return newRewards;
    }
    #endregion
}
