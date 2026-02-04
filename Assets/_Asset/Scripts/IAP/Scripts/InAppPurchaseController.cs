using Firebase.Analytics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

public class InAppPurchaseController : MonoBehaviour, IStoreListener
{
    public float ratePurchase = 0.65f;

    public static InAppPurchaseController Instance { get; private set; }

    private IStoreController m_StoreController;
    private IExtensionProvider m_StoreExtensionProvider;
    private CrossPlatformValidator validator;

    private UnityAction<Product> buyProductCallback;
    private UnityAction<Product> buyProductCallbackDefault;
    private UnityAction initializedCallback;

    private bool isRestoringPurchases;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);

        DontDestroyOnLoad(this);
        InitializePurchasing(ProductDefine.GetListProducts(), BuyIapSuccessCallback, OnInitialized);
    }


    #region PUBLIC METHODS

    public void InitializePurchasing(ProductIAP[] listProduct, UnityAction<Product> buyCallback, UnityAction initCallback = null)
    {
        if (IsInitialized())
        {
            if (initCallback != null)
            {
                initCallback();
            }

            return;
        }

        buyProductCallback = buyCallback;
        buyProductCallbackDefault = buyCallback;
        initializedCallback = initCallback;

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        for (int i = 0; i < listProduct.Length; i++)
        {
            builder.AddProduct(listProduct[i].productId, listProduct[i].productType);
        }

#if !UNITY_EDITOR

#if UNITY_ANDROID
        validator = new CrossPlatformValidator(GooglePlayTangle.Data(), null, Application.identifier);
#elif UNITY_IOS
        // validator = new CrossPlatformValidator(null, AppleTangle.Data(), Application.identifier);
#endif

#endif
        UnityPurchasing.Initialize(this, builder);
    }

    /// <summary>
    /// Buy Product
    /// </summary>
    /// <param name="productId">Id of IAP pack</param>
    /// <param name="callback">Buy callback, if callback is NULL, the defaut buy callback from InitializePurchasing will be use</param>
    public void BuyProductID(string productId, UnityAction<Product> callback = null)
    {
        if (IsInitialized())
        {
            Product product = m_StoreController.products.WithID(productId);

            if (product != null && product.availableToPurchase)
            {
                UIManager.Instance.ShowWaiting(true);

                if (callback != null)
                {
                    // Override purchase callback
                    buyProductCallback = callback;
                }
                else
                {
                    buyProductCallback = buyProductCallbackDefault;
                }

                m_StoreController.InitiatePurchase(product);

                DebugCustom.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));

                if(MediationAds.Instance != null)
                {
                    MediationAds.Instance.isDontShowOpenAds = true;
                }
            }
            else
            {
                DebugCustom.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
            }
        }
        else
        {
            DebugCustom.Log("BuyProductID FAIL. Not initialized or Callback is null.");
        }
    }

    public void RestorePurchases(UnityAction<bool> callback = null)
    {
        if (!IsInitialized())
        {
            DebugCustom.Log("RestorePurchases FAIL. Not initialized.");
            return;
        }

        if (isRestoringPurchases)
        {
            DebugCustom.Log("Restoring Purchases");
            return;
        }

        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
        {
            DebugCustom.Log("RestorePurchases started ...");

            //isRestoringPurchases = true;
            //var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();

            //apple.RestoreTransactions((result) =>
            //{
            //    DebugCustom.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");

            //    isRestoringPurchases = false;

            //    if (callback != null)
            //    {
            //        callback(result);
            //    }
            //});
        }
        else
        {
            DebugCustom.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
        }
    }

    public Product GetProduct(string productId)
    {
        Product re = null;

        if (IsInitialized())
        {
            for (int i = 0; i < m_StoreController.products.all.Length; i++)
            {
                if (string.Equals(productId, m_StoreController.products.all[i].definition.id, StringComparison.Ordinal))
                {
                    re = m_StoreController.products.all[i];
                    break;
                }
            }
        }

        return re;
    }

    public Product[] GetAllProducts()
    {
        if (IsInitialized())
        {
            return m_StoreController.products.all;
        }
        else
        {
            DebugCustom.Log("IAP is not Init, return null");
            return null;
        }
    }

    public bool IsInitialized()
    {
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }

    #endregion

    #region IMPLEMENTION IStoreListener
    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        DebugCustom.Log("OnInitializeFailed_1" + message);
        this.StartDelayAction(5f, () =>
        {
            InitializePurchasing(ProductDefine.GetListProducts(), BuyIapSuccessCallback, OnInitialized);
        });
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        DebugCustom.Log("OnInitializeFailed_2" + error);
        this.StartDelayAction(5f, () =>
        {
            InitializePurchasing(ProductDefine.GetListProducts(), BuyIapSuccessCallback, OnInitialized);
        });
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        m_StoreController = controller;
        m_StoreExtensionProvider = extensions;

        if (initializedCallback != null)
            initializedCallback();

        DebugCustom.Log("On Initialized Success");
    }

    public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
    {
        UIManager.Instance.ShowWaiting(false);
        DebugCustom.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", i.definition.storeSpecificId, p));

        if (MediationAds.Instance != null)
        {
            MediationAds.Instance.isDontShowOpenAds = false;
        }
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {

        Debug.Log("On PurchaseProcessingResult");

        bool validPurchase = true;

#if UNITY_ANDROID || UNITY_STANDALONE_OSX
        try
        {
            var result = validator.Validate(args.purchasedProduct.receipt);

            //Debug.Log("********** Receipt is valid **********\n");
            //foreach (IPurchaseReceipt productReceipt in result)
            //{
            //    Debug.Log(string.Format("Product ID: {0}\nPurchased date: {1}\nReceipt: {2}", productReceipt.productID, productReceipt.purchaseDate, productReceipt));
            //}

            //Debug.Log("**************************************\n");
        }
        catch (IAPSecurityException e)
        {
            validPurchase = false;
            Debug.Log(string.Format("Invalid receipt: {0}", e.Message));
        }
#endif

        Debug.Log("On PurchaseProcessingResult2 " + validPurchase.ToString());

        if (validPurchase)
        {
            if (buyProductCallback != null)
            {
                buyProductCallback.Invoke(args.purchasedProduct);
            }
                
#if UNITY_ANDROID

            try
            {
                //Todo Verify Inapp on server Google Play

                float gPrice = Mathf.RoundToInt((float)args.purchasedProduct.metadata.localizedPrice);
                string gCurrency = args.purchasedProduct.metadata.isoCurrencyCode;
                Debug.Log("Info : ======= " + gPrice + "============" + gCurrency);

                //AppsflyerHelper.SendEventInapp(gCurrency, gPrice);
                AppsflyerHelper.AppsFlyerPurchaseEvent(args.purchasedProduct);
            }
            catch
            {
                Debug.Log("______bug send event appsfly iap");
            }
#endif


#if UNITY_IOS && !UNITY_EDITOR
            try
            {
                AppsflyerHelper.AppsFlyerPurchaseEvent(args.purchasedProduct);
            }
            catch
            {
                Debug.Log("______bug send event appsfly iap");
            }
#endif
            //GameData.userData.purchases.SetEverPurchase();

            List<Parameter> fParamters = new List<Parameter>();
            fParamters.Add(new Parameter("package", args.purchasedProduct.definition.id));
            fParamters.Add(new Parameter("level", GameData.userData.profile.GetAccountLevel()));
            FirebaseAnalyticsHelper.LogEvent("in_app_purchase", fParamters.ToArray());
            //AppsFlyer.sendEvent("af_purchase", null);
        }
        else
        {

        }

        if (MediationAds.Instance != null)
        {
            MediationAds.Instance.isDontShowOpenAds = false;
        }
        UIManager.Instance.ShowWaiting(false);

        // Return a flag indicating whether this product has completely been received, or if the application needs 
        // to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still 
        // saving purchased products to the cloud, and when that save is delayed. 
        return PurchaseProcessingResult.Complete;
    }

    #endregion

    #region LISTENERS

    private void BuyIapSuccessCallback(Product purchasedProduct)
    {
        UIManager.Instance.ShowWaiting(false);
        string productId = purchasedProduct.definition.id;

        string data = JsonConvert.SerializeObject(purchasedProduct.metadata);

        DebugCustom.ShowDebug(data);
        //Debug.Log("LogData inapp___");
        //Debug.Log("Log:__" + data);

        //UniClipboard.SetText(data);

        if (string.IsNullOrEmpty(productId))
        {
            return;
        }
    }

    private void OnInitialized()
    {
        DebugCustom.Log("IAP INIT DONE");
    }

    #endregion


    private void GetVeriryInappGGOnServer(string dataSend, UnityAction<bool, string> callback = null)
    {
        //string url = DBService.Instance.Domain + APIString.getVerifyInapp;

        //Debug.Log("URL__" + url);

        //DBService.Instance.SendMethodPostString(url, dataSend, (webStatus, data) =>
        //{
        //    if (callback != null)
        //        callback(webStatus == WebStatus.SUCCESS, data);
        //});
    }
}

public class GooglePurchase
{
    public PayloadData PayloadData;

    // Json Fields, ! Case-sensetive
    public string Store;
    public string TransactionID;
    public string Payload;

    public static GooglePurchase FromJson(string json)
    {
        var purchase = JsonUtility.FromJson<GooglePurchase>(json);
        purchase.PayloadData = PayloadData.FromJson(purchase.Payload);
        return purchase;
    }
}

public class ApplePurchase
{

    // Json Fields, ! Case-sensetive
    public string Store;
    public string TransactionID;
    public string Payload;

    public static ApplePurchase FromJson(string json)
    {
        var purchase = JsonUtility.FromJson<ApplePurchase>(json);
        return purchase;
    }
}

public class PayloadData
{
    public JsonData JsonData;

    // Json Fields, ! Case-sensetive
    public string signature;
    public string json;

    public static PayloadData FromJson(string json)
    {
        var payload = JsonUtility.FromJson<PayloadData>(json);
        payload.JsonData = JsonUtility.FromJson<JsonData>(payload.json);
        return payload;
    }
}

public class JsonData
{
    // Json Fields, ! Case-sensetive

    public string orderId;
    public string packageName;
    public string productId;
    public long purchaseTime;
    public int purchaseState;
    public string purchaseToken;
}