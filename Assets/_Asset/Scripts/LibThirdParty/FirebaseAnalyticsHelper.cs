using Firebase.Analytics;
using System;
using System.Collections.Generic;
using UnityEngine;

public class FirebaseAnalyticsHelper
{
    public static void LogEvent(string eventName, params Parameter[] agrs)
    {
#if UNITY_EDITOR
        return;
#endif
        try
        {
            FirebaseAnalytics.LogEvent(eventName, agrs);
        }
        catch (Exception e)
        {
            DebugCustom.LogError(e);
        }
    }

    public static void LogEventCheckPoint(int levelPass)
    {
        LogEvent("checkpoint" + levelPass.ToString("D2"));
    }

    public static void LogLevelStart(int level)
    {
        long currentGold = 0;
        try
        {
            currentGold = GameData.userData.items.GetQuantityHave(ItemType.GOLD);
        }
        catch { }

        Parameter[] parameters = new Parameter[]
        {
        new Parameter("level", level.ToString()),
        new Parameter("current_gold", currentGold.ToString())
        };


        LogEvent("level_start", parameters);
    }

    public static void LogLevelComplete(int level, float timeStart)
    {
        float timeEnd = Time.realtimeSinceStartup;

        var timeTotal = timeEnd - timeStart;

        float timePlayedSeconds = Mathf.RoundToInt(timeTotal);
        Parameter[] parameters = new Parameter[]
        {
        new Parameter("level", level.ToString()),
        new Parameter("timeplayed", Mathf.RoundToInt(timePlayedSeconds).ToString())
        };


        LogEvent("level_complete", parameters);
    }

    public static void LogLevelFail(int levelCurrent)
    {
        string keySaveFail = "keyFail";
        keySaveFail = keySaveFail + levelCurrent;
        int failCount = PlayerPrefs.GetInt(keySaveFail, 0);
        failCount++;

        PlayerPrefs.SetInt(keySaveFail, failCount);
        PlayerPrefs.Save();

        Parameter[] parameters = new Parameter[]
        {
        new Parameter("level", levelCurrent.ToString()),
        new Parameter("failcount", failCount.ToString())
        };

        LogEvent("level_fail", parameters);
    }

    public static void LogEarnVirtualCurrency(string currencyName, long value, string source = null)
    {
        var parameters = new List<Parameter>()
    {
        new Parameter("virtual_currency_name", currencyName),
        new Parameter("value", value)
    };

        if (!string.IsNullOrEmpty(source))
        {
            parameters.Add(new Parameter("source", source));
        }

        LogEvent(
            FirebaseAnalytics.EventEarnVirtualCurrency,
            parameters.ToArray()
        );
    }

    public static void LogSpendVirtualCurrency(string currencyName,long value,string itemName = null)
    {
        var parameters = new List<Parameter>()
                                                {
                                                    new Parameter("virtual_currency_name", currencyName),
                                                    new Parameter("value", value)
                                                };

        if (!string.IsNullOrEmpty(itemName))
        {
            parameters.Add(new Parameter("item_name", itemName));
        }

        LogEvent(
            FirebaseAnalytics.EventSpendVirtualCurrency,
            parameters.ToArray()
        );
    }

    // reward 
    public static void LogAdsRewardLoad()
    {
        LogEvent("ads_reward_load");
    }

    public static void LogAdsRewardClick()
    {
        LogEvent("ads_reward_click");
    }

    public static void LogAdsRewardShowSuccess()
    {
        LogEvent("ads_reward_show_success");
    }

    public static void LogAdsRewardShowFail()
    {
        LogEvent("ads_reward_show_fail");
    }

    public static void LogAdsRewardComplete()
    {
        LogEvent("ads_reward_complete");
    }

    // Inter Event 
    public static void LogAdsInterLoadFail()
    {
        LogEvent("ad_inter_load_fail");
    }

    public static void LogAdsInterLoadSuccess()
    {
        LogEvent("ad_inter_load_success");
    }

    public static void LogAdsInterShow()
    {
        LogEvent("ad_inter_show");
    }

    public static void LogAdsInterClick()
    {
        LogEvent("ad_inter_click");
    }
}