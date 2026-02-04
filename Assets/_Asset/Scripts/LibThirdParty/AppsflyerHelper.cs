using AppsFlyerSDK;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.SocialPlatforms.Impl;

public static class AppsflyerHelper
{
    public static void SendEvent(string eventValue)
    {
        AppsFlyer.sendEvent(eventValue, null);
    }

    public static void SendEventInapp(string gCurrency,float gPrice)
    {
        Dictionary<string, string> purchaseEvent = new Dictionary<string, string>();
        purchaseEvent.Add(AFInAppEvents.CURRENCY, gCurrency);
        purchaseEvent.Add(AFInAppEvents.REVENUE, gPrice.ToString(CultureInfo.InvariantCulture));
        purchaseEvent.Add("af_quantity", "1");
        AppsFlyer.sendEvent("af_purchase", purchaseEvent);
    }

    public static void AppsFlyerPurchaseEvent(Product product)
    {
        Dictionary<string, string> eventValue = new Dictionary<string, string>();
        eventValue.Add("af_revenue", GetAppsflyerRevenue(product.metadata.localizedPrice));
        eventValue.Add("af_content_id", product.definition.id);
        eventValue.Add("af_currency", product.metadata.isoCurrencyCode);
        AppsFlyer.sendEvent("af_purchase", eventValue);
    }

    public static string GetAppsflyerRevenue(decimal amount)
    {
        decimal val = decimal.Multiply(amount, 0.63m);
        return val.ToString();
    }

    public static void SendLevelAchievedEvent(int level, int score)
    {
        var eventValues = new Dictionary<string, string>()
        {
            { "af_level", level.ToString() },     // compulsory
            { "af_score", score.ToString() }      // compulsory
        };

        AppsFlyer.sendEvent("af_level_achieved", eventValues);
    }

    public static void SendTutComplete()
    {
        var eventValues = new Dictionary<string, string>()
        {
            { "af_success", "True" },     // compulsory
        };

        AppsFlyer.sendEvent("af_tutorial_completion", eventValues);
    }

    public static void SendShowInter(int level)
    {
        var eventValues = new Dictionary<string, string>()
        {
            { "af_level", level.ToString() },     // compulsory
        };

        AppsFlyer.sendEvent("af_inters_show", eventValues);
    }

    public static void SendShowInterSuccess(int level)
    {
        var eventValues = new Dictionary<string, string>()
        {
            { "af_level", level.ToString() },     // compulsory
        };

        AppsFlyer.sendEvent("af_inters_displayed", eventValues);
    }

    public static void SendShowReward(int level)
    {
        var eventValues = new Dictionary<string, string>()
        {
            { "af_level", level.ToString() },     // compulsory
        };

        AppsFlyer.sendEvent("af_rewarded_show", eventValues);
    }

    public static void SendShowRewardSuccess(int level)
    {
        var eventValues = new Dictionary<string, string>()
        {
            { "af_level", level.ToString() },     // compulsory
        };

        AppsFlyer.sendEvent("af_rewarded_displayed", eventValues);
    }


}