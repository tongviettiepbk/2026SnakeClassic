using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameData
{
    public static StaticGameData staticData = new StaticGameData();
    public static UserData userData = new UserData();

    public static void Reset()
    {
        staticData = new StaticGameData();
        userData = new UserData();
    }
    public static List<RewardData> GetWinRewards(int level)
    {
        //Todo : TONG-TIEP
        List<RewardData> rewards = new List<RewardData>();
        rewards.Add(new RewardData(ItemType.GOLD, 10));
        return rewards;
    }

    public static List<RewardData> GetLoseReward(int level)
    {
        //Todo : TONG-TIEP
        List<RewardData> rewards = new List<RewardData>();
        rewards.Add(new RewardData(ItemType.GOLD, 0));
        return rewards;
    }

}
