using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static MaxSdkCallbacks;

public static class Keys
{
    public const string realtimeAdRevenueGoogle = "ad_impression";
    public const string realtimeAdRevenue = "ad_impression_sdk";
    public const string adSubRevenue = "sub_impression";
}

public enum ShowResult
{
    Failed = 0,
    Skipped = 1,
    Finished = 2,
}

public class MediationAds : Singleton<MediationAds>
{
    public bool isHackAds = false;

    [Space(20)]
    public AdMobController adMobController;
    public bool isUseOpenAdsByAdmob = false;

    [Space(20)]
    public float lastFullscreenAdTime = -9999f;
    public float lastAppOpenAdTime = -99999;
    public const float INTER_COOLDOWN = 15f;

    // open ads
    [HideInInspector]
    public bool isAppOpenAdLoaded = false;
    [HideInInspector]
    public bool isShowingAppOpenAd = false;
    [HideInInspector]
    public bool isDontShowOpenAds = false;

    protected const float APP_OPEN_COOLDOWN = 180f; // 3 phút

    protected bool isShowingInter = false;
    protected bool isShowBannerDoing = false;
    protected bool isShowingReward = false;
    

    protected bool isInterstitialLoading;
    protected UnityAction<ShowResult> interstitialAdCallback;

    protected bool isRewaded;
    protected UnityAction<ShowResult> rewardAdCallback;

    protected bool isBannerAdLoading;
    protected UnityAction<ShowResult> bannerAdLoadCallback;

    private void Start()
    {
        this.isHackAds = GameConfig.isHackAds;
        DontDestroyOnLoad(this.gameObject);
    }

    public virtual void Initialize()
    {

    }

    public virtual void Preload()
    {

    }

    public virtual void ShowInterstitialAd(string placement, UnityAction<ShowResult> callback,bool isBackupReward = false)
    {
        callback(ShowResult.Finished);
    }

    public virtual void ShowRewardedVideoAd(string placement, UnityAction<ShowResult> callback = null)
    {
        callback(ShowResult.Finished);
    }

    public virtual void LoadBannerAd(UnityAction<ShowResult> callback = null)
    {
        callback(ShowResult.Finished);
    }

    public virtual void ShowBannerAd()
    {
        isShowBannerDoing = true;
    }

    public virtual void HideBannerAd()
    {
        isShowBannerDoing = false;
    }

    public virtual void DestroyBannerAd()
    {
        isShowBannerDoing = false;
    }

    public virtual void ShowMRec()
    {

    }

    public virtual void HideMRec()
    {

    }

    public virtual void DestroyMRec()
    {

    }

    public bool IsAbleShowInter()
    {
        if (isHackAds)
        {
            return false;
        }

        bool isAbleShowInter = false;
        try
        {
            if (GameData.userData.shops.isPurchaseRemoveAds)
            {
                return false;
            }

            if(GameData.userData.profile.currentStageId < GameConfig.levelMinShowInter)
            {
                return false;
            }
        }
        catch
        {

        }

        float elapsed = Time.realtimeSinceStartup - lastFullscreenAdTime;

        isAbleShowInter = elapsed >= GameConfig.INTER_COOLDOWN;

        return isAbleShowInter;

    }

    public void UpdateTimeShowFullAds()
    {
        Debug.Log("_______ Full ads");
        lastFullscreenAdTime = Time.realtimeSinceStartup;
        lastAppOpenAdTime = Time.realtimeSinceStartup;
    }

    public virtual void RequestShowInter(string placement, Action actionNeXtShowInter)
    {
        if (IsAbleShowInter())
        {
            ShowInterstitialAd(placement, (result) => {

                if (actionNeXtShowInter != null)
                {
                    actionNeXtShowInter.Invoke();
                }

            });
        }
        else
        {
            if(actionNeXtShowInter != null)
            {
                actionNeXtShowInter.Invoke();
            }
        }
    }

    public virtual void TryShowAppOpenAd()
    {

    }
}
