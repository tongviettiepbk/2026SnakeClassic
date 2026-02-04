#if MAX

using AppsFlyerSDK;
using Firebase.Analytics;
using System;

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using static MaxSdkCallbacks;

public class MaxAdController : MediationAds
{
    public string MaxSdkKey = "eQt0q3679KmUyKeNcSzqC01eB-lILmfTnJoufGxpSn__n1NVhHLeMgxZOaICke451El4ZBfuZum9Qw4WxzpW52";

    [Space(20)]
    public string InterstitialAdUnitId = "145a16ac07d162ca";
    public string RewardedAdUnitId = "3f2a24837081356f";
    public string BannerAdUnitId = "f146b2c1186ed353";
    public string MrecId = "1ac2c9fdd16d6ac7";
    public string idOpenAds = "79d1eaf514de7665";

    private int retryAttemptInter;
    private int retryAttemptReward;

    private string placementRewardVideo;

    private bool initialized = false;
    private bool isBannerLoaded = false;
    private bool isMRecLoaded = false;
    private bool isMRecCreated = false;

    private void Awake()
    {
        DontDestroyOnLoad(this);
        Initialize();

    }

    public override void Initialize()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;

        MaxSdkCallbacks.OnSdkInitializedEvent += sdkConfiguration =>
        {
            // AppLovin SDK is initialized, configure and start loading ads.
            InitializeInterstitialAds();
            InitializeRewardedAds();
            InitializeBannerAds();
            InitializeMRecAds();


            if (isUseOpenAdsByAdmob)
            {
                // Init Admob 
                adMobController.Init();
            }
            else
            {
                InitializeAppOpenAds();
            }

            // Show Mediation Debugger
            //MaxSdk.ShowMediationDebugger();
        };

        MaxSdk.InitializeSdk();
    }

    #region Interstitial

    public void InitializeInterstitialAds()
    {
        // Attach callback
        MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
        MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailedEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayedEvent;
        MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
        MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialAdFailedToDisplayEvent;
        MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;

        // Load the first interstitial
        LoadInterstitial();
    }

    private void LoadInterstitial()
    {
        MaxSdk.LoadInterstitial(InterstitialAdUnitId);
    }

    private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad is ready for you to show. MaxSdk.IsInterstitialReady(adUnitId) now returns 'true'

        // Reset retry attempt
        retryAttemptInter = 0;

        FirebaseAnalyticsHelper.LogAdsInterLoadSuccess();
    }

    private void OnInterstitialLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // Interstitial ad failed to load 
        // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds)

        retryAttemptInter++;
        double retryDelay = Math.Pow(2, Math.Min(6, retryAttemptInter));

        Invoke("LoadInterstitial", (float)retryDelay);
    }

    private void OnInterstitialDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        isShowingInter = true;
        UpdateTimeShowFullAds();

        AppsflyerHelper.SendShowInterSuccess(GameData.userData.profile.currentStageId);
    }

    private void OnInterstitialAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad failed to display. AppLovin recommends that you load the next ad.
        LoadInterstitial();

        if (interstitialAdCallback != null)
        {
            interstitialAdCallback(ShowResult.Failed);
            isDontShowOpenAds = false;
        }
            

        interstitialAdCallback = null;

        FirebaseAnalyticsHelper.LogAdsInterLoadFail();
    }

    private void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        LogInterClick();
    }

    private void OnInterstitialHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad is hidden. Pre-load the next ad.
        LoadInterstitial();
        isShowingInter = false;

        if (interstitialAdCallback != null)
        {
            interstitialAdCallback(ShowResult.Finished);
            isDontShowOpenAds = false;
        }
            

        interstitialAdCallback = null;
#if UNITY_IOS
            AudioManager.Instance.Mute(false);
#endif
    }

    #endregion

    #region Rewarded

    public void InitializeRewardedAds()
    {
        // Attach callback
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
        MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
        MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;

        // Load the first rewarded ad
        LoadRewardedAd();
    }

    private void LoadRewardedAd()
    {
        MaxSdk.LoadRewardedAd(RewardedAdUnitId);
    }

    private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad is ready for you to show. MaxSdk.IsRewardedAdReady(adUnitId) now returns 'true'.

        // Reset retry attempt
        retryAttemptReward = 0;

        FirebaseAnalyticsHelper.LogAdsRewardLoad();
    }

    private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // Rewarded ad failed to load 
        // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds).

        retryAttemptReward++;
        double retryDelay = Math.Pow(2, Math.Min(6, retryAttemptReward));

        Invoke("LoadRewardedAd", (float)retryDelay);

        LogVideoRewardLoadFail(this.placementRewardVideo, errorInfo.Message);
    }

    private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        LogVideoRewardShow(this.placementRewardVideo);
        UpdateTimeShowFullAds();
        isShowingReward = true;
    }

    private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad failed to display. AppLovin recommends that you load the next ad.
        LoadRewardedAd();
        if (rewardAdCallback != null)
        {
            rewardAdCallback(ShowResult.Failed);
            rewardAdCallback = null;
            isRewaded = false;
            isDontShowOpenAds = false;
        }

        //AppsFlyer.sendEvent("Monetize_reward_failed", null);
        //FirebaseAnalyticsHelper.LogEvent("Monetize_reward_failed");
    }

    private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        LogClickToVideoReward(this.placementRewardVideo);
    }

    private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad is hidden. Pre-load the next ad
        LoadRewardedAd();
        isShowingReward = false;
        isDontShowOpenAds = false;

        if (rewardAdCallback != null)
            rewardAdCallback(isRewaded ? ShowResult.Finished : ShowResult.Skipped);

        if (isRewaded)
        {

        }

        isRewaded = false;
        rewardAdCallback = null;


#if UNITY_IOS
            AudioManager.Instance.Mute(false);
#endif
    }

    private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
    {
        // The rewarded ad displayed and the user should receive the reward.
        isRewaded = true;

        LogVideoRewardShowDone(this.placementRewardVideo);
    }

    #endregion

    #region Banner

    private int bannerRetryAttempt = 0;
    private const int MAX_BANNER_RETRY = 6;

    public void InitializeBannerAds()
    {
        var adViewConfiguration = new MaxSdk.AdViewConfiguration(MaxSdk.AdViewPosition.BottomCenter);
        MaxSdk.CreateBanner(BannerAdUnitId, adViewConfiguration);

        // Set background color for banners to be fully functional
        MaxSdk.SetBannerBackgroundColor(BannerAdUnitId, Color.white);


        MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
        MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdLoadFailedEvent;
        MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
        MaxSdkCallbacks.Banner.OnAdExpandedEvent += OnBannerAdExpandedEvent;
        MaxSdkCallbacks.Banner.OnAdCollapsedEvent += OnBannerAdCollapsedEvent;

    }

    private void RetryLoadBanner()
    {
        MaxSdk.LoadBanner(BannerAdUnitId);
    }

    private void OnBannerAdLoadedEvent(string adUnitId, MaxSdk.AdInfo adInfo)
    {
        bannerRetryAttempt = 0;
        isBannerLoaded = true;

        HideMRec();
        ShowBannerAd();

    }

    private void OnBannerAdLoadFailedEvent(string adUnitId, MaxSdk.ErrorInfo errorInfo)
    {
        //bannerRetryAttempt++;

        //double retryDelay = Math.Pow(2, Math.Min(bannerRetryAttempt, MAX_BANNER_RETRY));

        //Debug.Log($"Banner load failed. Retry in {retryDelay}s");

        //Invoke(nameof(RetryLoadBanner), (float)retryDelay);

    }

    private void OnBannerAdClickedEvent(string adUnitId, MaxSdk.AdInfo adInfo) { }

    //private void OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdk.AdInfo adInfo) { }

    private void OnBannerAdExpandedEvent(string adUnitId, MaxSdk.AdInfo adInfo) { }

    private void OnBannerAdCollapsedEvent(string adUnitId, MaxSdk.AdInfo adInfo) { }

    public override void ShowBannerAd()
    {
#if UNITY_EDITOR
        return;
#endif
        if (isHackAds)
        {
            return;
        }

        if (!isBannerLoaded) return;

        base.ShowBannerAd();
        MaxSdk.ShowBanner(BannerAdUnitId);
    }

    public override void HideBannerAd()
    {
        base.HideBannerAd();
        MaxSdk.HideBanner(BannerAdUnitId);
    }

    public override void DestroyBannerAd()
    {
        base.DestroyBannerAd();
        MaxSdk.DestroyBanner(BannerAdUnitId);
    }

    public int GetStandardBannerHeightPx()
    {
#if UNITY_ANDROID
        float density = Screen.dpi / 160f;
#else
    float density = 1f;
#endif

        return Mathf.RoundToInt(50 * density);
    }

    #endregion

    #region MREC

    private int mrecRetryAttempt = 0;
    private const int MAX_MREC_RETRY = 6;

    public void InitializeMRecAds()
    {
        if (string.IsNullOrEmpty(MrecId))
        {
            Debug.LogWarning("MREC AdUnitId is empty");
            return;
        }

        if (isMRecCreated)
            return;


        // MRECs are sized to 300x250 on phones and tablets
        var adViewConfiguration = new MaxSdk.AdViewConfiguration(MaxSdk.AdViewPosition.Centered);
        MaxSdk.CreateMRec(MrecId, adViewConfiguration);

        MaxSdkCallbacks.MRec.OnAdLoadedEvent += OnMRecAdLoadedEvent;
        MaxSdkCallbacks.MRec.OnAdLoadFailedEvent += OnMRecAdLoadFailedEvent;
        MaxSdkCallbacks.MRec.OnAdClickedEvent += OnMRecAdClickedEvent;
        MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += OnMRecAdRevenuePaidEvent;
        MaxSdkCallbacks.MRec.OnAdExpandedEvent += OnMRecAdExpandedEvent;
        MaxSdkCallbacks.MRec.OnAdCollapsedEvent += OnMRecAdCollapsedEvent;

        isMRecCreated = true;
    }

    private void RetryLoadMRec()
    {
        if (!isMRecCreated)
            return;

        MaxSdk.LoadMRec(MrecId);
    }


    public void OnMRecAdLoadedEvent(string adUnitId, MaxSdk.AdInfo adInfo)
    {

        mrecRetryAttempt = 0;
        isMRecLoaded = true;
    }

    public void OnMRecAdLoadFailedEvent(string adUnitId, MaxSdk.ErrorInfo error)
    {
        //Debug.Log($"MREC Load Failed: {error.Code} - {error.Message}");
        //mrecRetryAttempt++;

        //double retryDelay = Math.Pow(2, Math.Min(mrecRetryAttempt, MAX_MREC_RETRY));

        //Debug.Log($"MREC Load Failed. Retry in {retryDelay}s - {error.Message}");

        //Invoke(nameof(RetryLoadMRec), (float)retryDelay);
    }

    public void OnMRecAdClickedEvent(string adUnitId, MaxSdk.AdInfo adInfo)
    {
        // Log nếu cần
    }

    public void OnMRecAdRevenuePaidEvent(string adUnitId, MaxSdk.AdInfo adInfo)
    {
        OnAdRevenuePaidEvent(adUnitId, adInfo);
    }

    public void OnMRecAdExpandedEvent(string adUnitId, MaxSdk.AdInfo adInfo)
    {
    }

    public void OnMRecAdCollapsedEvent(string adUnitId, MaxSdk.AdInfo adInfo)
    {
    }

    public override void ShowMRec()
    {
        if (isHackAds)
        {
            return;
        }

        if (!isMRecCreated || !isMRecLoaded)
            return;
        MaxSdk.ShowMRec(MrecId);
    }

    public override void HideMRec()
    {
        if (!isMRecCreated)
            return;

        MaxSdk.HideMRec(MrecId);
    }

    public override void DestroyMRec()
    {
        if (!isMRecCreated)
            return;

        MaxSdk.DestroyMRec(MrecId);
        isMRecCreated = false;
        isMRecLoaded = false;
    }

    #endregion

    #region Open Ads



    public void InitializeAppOpenAds()
    {
        if (string.IsNullOrEmpty(idOpenAds))
            return;

        MaxSdkCallbacks.AppOpen.OnAdLoadedEvent += OnAppOpenAdLoadedEvent;
        MaxSdkCallbacks.AppOpen.OnAdLoadFailedEvent += OnAppOpenAdLoadFailedEvent;
        MaxSdkCallbacks.AppOpen.OnAdDisplayedEvent += OnAppOpenAdDisplayedEvent;
        MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += OnAppOpenAdHiddenEvent;
        MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent += OnAppOpenAdDisplayFailedEvent;
        //MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;----------------

        LoadAppOpenAd();
    }

    private int appOpenRetryAttempt = 0;

    private void LoadAppOpenAd()
    {
        MaxSdk.LoadAppOpenAd(idOpenAds);
    }

    private void OnAppOpenAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        appOpenRetryAttempt++;

        double retryDelay = Math.Pow(2, Math.Min(6, appOpenRetryAttempt));
        Invoke(nameof(LoadAppOpenAd), (float)retryDelay);
    }

    private void OnAppOpenAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        appOpenRetryAttempt = 0;
        isAppOpenAdLoaded = true;
    }

    public override void TryShowAppOpenAd()
    {
        Debug.Log("TryShowAppOpenAd");

#if UNITY_EDITOR
        return;
#endif

        if (!isAppOpenAdLoaded)
            return;

        if (isShowingAppOpenAd)
            return;

        // ❌ Không show nếu vừa xem Inter / Reward
        if (Time.realtimeSinceStartup - lastFullscreenAdTime < APP_OPEN_COOLDOWN)
            return;

        // ❌ Không show nếu đang có ad khác
        if (isShowingInter || isShowingReward)
            return;

        if (isDontShowOpenAds)
        {
            return;
        }

        try
        {
            if (GameData.userData.profile.currentStageId < GameConfig.levelMinShowInter)
            {
                return;
            }
        }
        catch { }


        if (isUseOpenAdsByAdmob)
        {
            adMobController.ShowAppOpenAd();
        }
        else
        {
            try
            {
                MaxSdk.ShowAppOpenAd(idOpenAds);
            }
            catch
            {
            }

        }

    }

    private void OnAppOpenAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        lastAppOpenAdTime = Time.realtimeSinceStartup;
    }

    private void OnAppOpenAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        isShowingAppOpenAd = false;
        isAppOpenAdLoaded = false;

        LoadAppOpenAd();
    }

    private void OnAppOpenAdDisplayFailedEvent(
    string adUnitId,
    MaxSdkBase.ErrorInfo errorInfo,
    MaxSdkBase.AdInfo adInfo)
    {
        isShowingAppOpenAd = false;
        isAppOpenAdLoaded = false;

        LoadAppOpenAd();
    }

    //private void OnApplicationFocus(bool hasFocus)
    //{
    //    if (hasFocus)
    //    {
    //        TryShowAppOpenAd();
    //    }

    //    Debug.Log("hasFocus: " + hasFocus);
    //}

    private void OnApplicationPause(bool pause)
    {
        if (!pause)
        {
            TryShowAppOpenAd();
        }

        Debug.Log("pause: " + pause);
    }
    #endregion


    //Method Show Inter
    public override void ShowInterstitialAd(string placement, UnityAction<ShowResult> callback, bool isBackupReward = false)
    {
        isDontShowOpenAds = true;

#if UNITY_EDITOR
        if (callback != null)
        {
            callback(ShowResult.Finished);
            isDontShowOpenAds = false;
        }
           

        return;
#endif

        if (MaxSdk.IsInterstitialReady(InterstitialAdUnitId))
        {
            UpdateTimeShowFullAds();

            interstitialAdCallback = callback;
            LogInterShow(placement);

            if (!isBackupReward)
            {
                LPrepareAdsInter ui = UIManager.Instance.LoadUI(UIKey.PREPARE_INTER, isPauseMusic: true) as LPrepareAdsInter;
                ui.PrepareShowInter();

                this.StartDelayAction(GameConfig.timeWaitPopupShowInter + 0.2f, () =>
                {
                    if (string.IsNullOrEmpty(placement))
                    {
                        MaxSdk.ShowInterstitial(InterstitialAdUnitId);
                    }
                    else
                    {
                        MaxSdk.ShowInterstitial(InterstitialAdUnitId, placement);
                    }
                });
            }
            else
            {

                if (string.IsNullOrEmpty(placement))
                {
                    MaxSdk.ShowInterstitial(InterstitialAdUnitId);
                }
                else
                {
                    MaxSdk.ShowInterstitial(InterstitialAdUnitId, placement);
                }
            }

#if UNITY_IOS
            AudioManager.Instance.Mute(true);
#endif
        }
        else
        {
            LoadInterstitial();

            if (callback != null)
            {
                callback(ShowResult.Failed);
                isDontShowOpenAds = false;
            }
               

            //FirebaseAnalyticsHelper.LogEvent("Monetize_no_interstitial");
        }
    }

    public override void ShowRewardedVideoAd(string placement, UnityAction<ShowResult> callback = null)
    {
        isDontShowOpenAds = true;

        this.placementRewardVideo = placement;
        LogRequestVideoReward(placement);

        isRewaded = false;

        if (isHackAds)
        {
            callback(ShowResult.Finished);
            return;
        }

#if UNITY_EDITOR
        if (callback != null)
        {
            isDontShowOpenAds = false;
            callback(ShowResult.Finished);
        }
        return;
#endif

        if (MaxSdk.IsRewardedAdReady(RewardedAdUnitId))
        {
            UpdateTimeShowFullAds();

            rewardAdCallback = callback;

            if (string.IsNullOrEmpty(placement))
            {
                MaxSdk.ShowRewardedAd(RewardedAdUnitId);
            }
            else
            {
                MaxSdk.ShowRewardedAd(RewardedAdUnitId, placement);
            }

            LogWatchVideo(placement, true, GameData.userData.profile.currentStageId.ToString());

#if UNITY_IOS
            AudioManager.Instance.Mute(true);
#endif
        }
        else
        {
            isDontShowOpenAds = false;
            LogWatchVideo(placement, false, GameData.userData.profile.currentStageId.ToString());

            ShowInterstitialAd("interstitial_backup_" + placement, callback, true);
            
        }
    }

    #region Login Event

    private void OnAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo impressionData)
    {
        double revenueTemp = 0;

        if (impressionData != null)
        {
            revenueTemp = impressionData.Revenue;
        }

        if(revenueTemp <= 0)
        {
            return;
        }

        try
        {
            var impressionParameters = new[] {
                                            new Firebase.Analytics.Parameter("ad_platform", "AppLovin"),
                                            new Firebase.Analytics.Parameter("ad_source", impressionData.NetworkName),
                                            new Firebase.Analytics.Parameter("ad_unit_name", impressionData.AdUnitIdentifier),
                                            new Firebase.Analytics.Parameter("ad_format", impressionData.AdFormat),
                                            new Firebase.Analytics.Parameter(FirebaseAnalytics.ParameterValue, revenueTemp),
                                            new Firebase.Analytics.Parameter("currency", "USD"), // All AppLovin revenue is sent in USD
                                            };

            FirebaseAnalyticsHelper.LogEvent(Keys.realtimeAdRevenueGoogle, impressionParameters);
            FirebaseAnalyticsHelper.LogEvent(Keys.realtimeAdRevenue, impressionParameters);
        }
        catch
        {

        }


        try
        {
            if (impressionData != null)
            {
                var additionalParams = new Dictionary<string, string>()
                                            {
                                                { AdRevenueScheme.AD_UNIT, adUnitId },
                                                { AdRevenueScheme.AD_TYPE, impressionData.AdFormat },
                                                { "ad_source", impressionData.NetworkName },
                                                { AdRevenueScheme.PLACEMENT, impressionData.Placement },
                                            };

                AppsFlyerAdRevenue.logAdRevenue("AppLovin",
                    AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeApplovinMax,
                    revenueTemp, "USD", additionalParams);
            }

            Debug.Log("Send revenue appflyer____" + revenueTemp);
        }
        catch
        {

        }
    }

    public void LogWatchVideo(string action, bool isHasVideo, string level)
    {
        bool isHasInternet = false;
        var internetTemp = Application.internetReachability;
        if (internetTemp == NetworkReachability.NotReachable)
        {
            isHasInternet = false;
        }
        else
        {
            isHasInternet = true;
        }

        try
        {
            Parameter[] parameters = new Parameter[4]
            {
                new Parameter("actionWatch", action.ToString()),
                new Parameter("has_ads", isHasVideo.ToString()),
                new Parameter("has_internet", isHasInternet.ToString()),
                new Parameter("level",level)
            };
            FirebaseAnalytics.LogEvent("watch_video_ads", parameters);
        }
        catch
        {

        }
    }

    public void LogRequestVideoReward(string placement)
    {
        try
        {
            Parameter[] parameters = new Parameter[1]
            {
                new Parameter("placement", placement.ToString()),
            };

            FirebaseAnalytics.LogEvent("ads_reward_offer", parameters);
        }
        catch
        {
        }

        AppsflyerHelper.SendShowReward(GameData.userData.profile.currentStageId);
    }

    public void LogClickToVideoReward(string placement)
    {
        //try
        //{
        //    Parameter[] parameters = new Parameter[1]
        //    {
        //        new Parameter("placement", placement.ToString()),
        //    };
        //    FirebaseAnalytics.LogEvent("ads_reward_click", parameters);
        //}
        //catch
        //{
        //}

        FirebaseAnalyticsHelper.LogAdsRewardClick();
    }

    public void LogVideoRewardShow(string placement)
    {
        //try
        //{
        //    Parameter[] parameters = new Parameter[1]
        //    {
        //        new Parameter("placement", placement.ToString()),
        //    };
        //    FirebaseAnalytics.LogEvent("ads_reward_show", parameters);
        //}
        //catch
        //{
        //}

        AppsflyerHelper.SendShowRewardSuccess(GameData.userData.profile.currentStageId);
        FirebaseAnalyticsHelper.LogAdsRewardShowSuccess();
    }

    public void LogVideoRewardLoadFail(string placement, string errormsg)
    {
        //try
        //{
        //    Parameter[] parameters = new Parameter[2]
        //    {
        //        new Parameter("placement", placement.ToString()),
        //        new Parameter("errormsg", errormsg.ToString()),
        //    };
        //    FirebaseAnalytics.LogEvent("ads_reward_fail", parameters);
        //}
        //catch
        //{
        //}

        FirebaseAnalyticsHelper.LogAdsRewardShowFail();
    }

    public void LogVideoRewardShowDone(string placement)
    {
        //try
        //{
        //    Parameter[] parameters = new Parameter[1]
        //    {
        //        new Parameter("placement", placement.ToString()),
        //    };
        //    FirebaseAnalytics.LogEvent("ads_reward_complete", parameters);
        //}
        //catch
        //{

        //}

        FirebaseAnalyticsHelper.LogAdsRewardComplete();
    }

    public void LogInterShow(string actionWatchLog)
    {
        //try
        //{
        //    bool isHasInternet = false;
        //    var internetTemp = Application.internetReachability;
        //    if (internetTemp == NetworkReachability.NotReachable)
        //    {
        //        isHasInternet = false;
        //    }
        //    else
        //    {
        //        isHasInternet = true;
        //    }

        //    Parameter[] parameter = new Parameter[]
        //    {
        //        new Parameter("action_watch", actionWatchLog.ToString()),
        //        new Parameter("has_internet", isHasInternet.ToString())
        //    };
        //}
        //catch
        //{

        //}

        AppsflyerHelper.SendShowInter(GameData.userData.profile.currentStageId);
        FirebaseAnalyticsHelper.LogAdsInterShow();
    }

    public void LogInterClick()
    {
        //try
        //{
        //    FirebaseAnalytics.LogEvent("ad_inter_click");
        //}
        //catch
        //{

        //}

        FirebaseAnalyticsHelper.LogAdsInterClick();
    }

    #endregion

}

#endif


