using System;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class UserItemData : BaseUserData
{
    public Dictionary<string, long> consumables { get; set; } = new Dictionary<string, long>(); //key=Item Type

    public long totalGemSpent;
    public long heart { get; set; } = 4;
    public double lastUpdatedTimeStamina { get; set; } = ConstantValues.defaultDateMiliseconds;

    protected override string GetDataKey()
    {
        return UserData.DATA_KEY_ITEMS;
    }

    public void Initialize()
    {
#if UNITY_EDITOR
        List<RewardData> rewards = new List<RewardData>();

        ItemUtils.Receive(rewards, showPopup: false, playSfx: false);
#endif
    }

    public void SetQuantity(ItemType type, long quantity)
    {
        if (type == ItemType.HEART)
        {
            long staminaBefore = heart;
            heart = quantity;
            if (heart >= GameConfig.HEART_MAX_RECOVERY || staminaBefore >= GameConfig.HEART_MAX_RECOVERY)
            {
                SaveTimeUpdateStamina();
                DebugCustom.Log("SetQuantity | time=" + lastUpdatedTimeStamina.ToDateTime());
            }
        }

        string id = ((int)type).ToString();
        consumables[id] = quantity;

        if (quantity <= 0)
        {
            consumables.Remove(id);
        }

        isDataChanged = true;
    }

    public long GetQuantityHave(ItemType type)
    {
        if (type == ItemType.HEART)
        {
            UpdateStamina();
            return heart;
        }

        string id = ((int)type).ToString();

        if (consumables.ContainsKey(id))
        {
            return consumables[id];
        }

        return 0;
    }

    public void Receive(ItemType type, long quantity)
    {
        if (quantity > 0)
        {
            Adjust(type, quantity);
        }
    }

    public void Consume(ItemType type, long quantity)
    {
        if (quantity > 0)
        {
            Adjust(type, -quantity);
        }
        if (type == ItemType.GOLD)
        {
            totalGemSpent += quantity;
            isDataChanged = true;
        }
    }

    private void Adjust(ItemType type, long quantity)
    {
        long curQuantity = GetQuantityHave(type);
        curQuantity += quantity;
        SetQuantity(type, curQuantity);
        isDataChanged = true;
        //todo:
        GameData.userData.Save(true);
    }
    #region Stamina
    public void UpdateStamina()
    {
        if (heart < GameConfig.HEART_MAX_RECOVERY)
        {
            //DateTime now = MasterInfo.Instance.time.GetTimeNow();
            //DateTime lastUpdatedTime = lastUpdatedTimeStamina.ToDateTime();
            //TimeSpan timeDifference = now - lastUpdatedTime;
            //int heartToRecover = (int)(timeDifference.TotalSeconds / GameConfig.HEART_MAX_RECOVERY);
            //DebugCustom.LogFormat("[UpdateStamina] last={0}, seconds={1}, recover={2}", lastUpdatedTime, timeDifference.TotalSeconds, staminaToRecover);
            //if (heartToRecover > 0)
            //{
            //    heart += heartToRecover;
            //    if (heart > GameConfig.HEART_MAX_RECOVERY)
            //        heart = GameConfig.HEART_MAX_RECOVERY;

            //    SaveTimeUpdateStamina();
            //DebugCustom.Log("UpdateStamina | time=" + lastUpdatedTimeStamina.ToDateTime());
        }
    }


    public void SaveTimeUpdateStamina()
    {
        //lastUpdatedTimeStamina = MasterInfo.Instance.time.GetTimeNow().ToMiliseconds();
        isDataChanged = true;
    }
    #endregion
}
