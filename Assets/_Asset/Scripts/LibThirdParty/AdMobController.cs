using GoogleMobileAds.Api;
using System;
using UnityEngine;
using static MaxSdkCallbacks;

public class AdMobController : MonoBehaviour
{
    public MediationAds mediationAdsController;
    public string idOpenAds = "ca-app-pub-8467610367562059/1115280271";

    [Space(20)]
    public bool isTest = false;
    public string idOpenAdsTest = "ca-app-pub-3940256099942544/9257395921";

    [Header("Banner Ads")]
    public string idBannerCollabps = "ca-app-pub-3046520253844875/5301777531";
    public string idBannerTest = "ca-app-pub-3940256099942544/6300978111";

    //    "collapsible: ca-app-pub-3046520253844875/5301777531
    //adaptive: ca-app-pub-3046520253844875/4649356636"

    private BannerView bannerView;

    private AppOpenAd appOpenAd;

    private bool isShowingAd = false;
    private bool isLoadingAd = false;

    public bool IsAdAvailable => appOpenAd != null && !isShowingAd;

    public void Init()
    {
        Debug.Log("Init admob open ads");
        // Initialize Google Mobile Ads Unity Plugin.
        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            // This callback is called once the MobileAds SDK is initialized.
            LoadAppOpenAd();

            //LoadCollapsibleBanner(); 
        });
    }

    public void LoadAppOpenAd()
    {
        if (isTest)
        {
            idOpenAds = idOpenAdsTest;
        }

        if (appOpenAd != null)
        {
            appOpenAd.Destroy();
            appOpenAd = null;
        }

        var request = new AdRequest();
        AppOpenAd.Load(idOpenAds, request, (ad, error) =>
        {
            if (error != null || ad == null)
            {
                Debug.Log("AppOpen Ad Failed to Load: " + error);
                return;
            }

            Debug.Log("isAppOpenAdLoaded " + true);
            mediationAdsController.isAppOpenAdLoaded = true;

            appOpenAd = ad;
            RegisterAdEvents(ad);
        });
    }

    private void RegisterAdEvents(AppOpenAd ad)
    {
        ad.OnAdFullScreenContentClosed += () =>
        {
            mediationAdsController.isShowingAppOpenAd = false;

            isShowingAd = false;
            LoadAppOpenAd(); // Tải lại sau khi đóng
        };

        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("App open ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };

        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("App open ad recorded an impression.");
        };

        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("App open ad was clicked.");
        };

        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("App open ad full screen content opened.");
            isShowingAd = true;
            mediationAdsController.isShowingAppOpenAd = true;
            mediationAdsController.lastAppOpenAdTime = Time.realtimeSinceStartup;
        };

        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("App open ad failed to open full screen content " +
                           "with error : " + error);

            isShowingAd = false;
            LoadAppOpenAd();
        };
    }

    public void ShowAppOpenAd()
    {
        Debug.Log("Request open ads");
        if (IsAdAvailable && appOpenAd.CanShowAd())
        {
            Debug.Log("show open ads");
            appOpenAd.Show();
        }
    }

    #region COLLAPSIBLE BANNER (BOTTOM)
    public void LoadCollapsibleBanner()
    {
        string adUnitId = isTest ? idBannerTest : idBannerCollabps;

        // ❗ Destroy banner cũ nếu có
        if (bannerView != null)
        {
            bannerView.Destroy();
            bannerView = null;
        }

        // Adaptive Banner
        AdSize adSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(
            AdSize.FullWidth
        );

        bannerView = new BannerView(
            adUnitId,
            adSize,
            AdPosition.Bottom
        );

        // AdRequest + collapsible
        AdRequest request = new AdRequest();
        request.Extras.Add("collapsible", "bottom"); // 🔥 QUAN TRỌNG

        bannerView.LoadAd(request);
    }

    public void DestroyBannerCollap()
    {
        if (bannerView != null)
        {
            bannerView.Destroy();
            bannerView = null;
        }
    }
    #endregion

}
