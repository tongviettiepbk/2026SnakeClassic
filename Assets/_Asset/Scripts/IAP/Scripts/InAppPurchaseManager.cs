using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;

public class InAppPurchaseManager : MonoBehaviour
{
    public static InAppPurchaseManager Instance;

    private StoreController storeController;
    private bool isInitialized;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        InitInapp();
    }

    public async void InitInapp()
    {
        if (isInitialized)
            return;

        // Get StoreController
        storeController = UnityIAPServices.StoreController();

        // Add event listeners
        storeController.OnStoreDisconnected += OnStoreDisconnected;

        storeController.OnProductsFetched += OnProductsFetched;
        storeController.OnProductsFetchFailed += OnProductsFetchFailed;

        storeController.OnPurchasesFetched += OnPurchasesFetched;
        storeController.OnPurchasesFetchFailed += OnPurchasesFetchFailed;

        await storeController.Connect();

        // Define products
        var catalogProvider = new CatalogProvider();

        //catalogProvider.AddProduct("100_gold_coins", ProductType.Consumable,
        //    new StoreSpecificIds()
        //    {
        //        {"100_gold_coins_google", GooglePlay.Name},
        //        {"100_gold_coins_mac", MacAppStore.Name}
        //    });

        for(int i = 0; i < ProductDefine.GetListProducts().Length; i++)
        {
            var dataTemp = ProductDefine.GetListProducts()[i];
            catalogProvider.AddProduct(dataTemp.productId, dataTemp.productType);
        }

        catalogProvider.FetchProducts(
            list => storeController.FetchProducts(list)
        );

        isInitialized = true;
    }

    /// <summary>
    /// Invoked when connection is lost to the current store, or on a Connect() failure.
    /// </summary>
    /// <param name="failure">Information regarding the failure.</param>
    private void OnStoreDisconnected(StoreConnectionFailureDescription failure)
    {
    }

    /// <summary>
    /// Invoked with products that are successfully fetched.
    /// </summary>
    /// <param name="products">Products successfully returned from the app store.</param>
    private void OnProductsFetched(List<Product> products)
    {
        // Fetch purchases for successfully retrieved products
        storeController.FetchPurchases();
    }

    /// <summary>
    /// Invoked when an attempt to fetch products has failed or when a subset of products failed to be fetched.
    /// </summary>
    /// <param name="failure">Information regarding the failure.</param>
    private void OnProductsFetchFailed(ProductFetchFailed failure)
    {
    }

    /// <summary>
    /// Invoked when previous purchases are fetched.
    /// </summary>
    /// <param name="orders">All active pending, completed, and deferred orders for previously fetched products.</param>
    private void OnPurchasesFetched(Orders orders)
    {
    }

    /// <summary>
    /// Invoked when an attempt to fetch previous purchases has failed.
    /// </summary>
    /// <param name="failure">Information regarding the failure.</param>
    private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription failure)
    {
    }

    /// <summary>
    /// Invoked when a purchase needs to be processed and fulfilled.
    /// </summary>
    /// <param name="order">The order awaiting fulfillment.</param>
    private void OnPurchasePending(PendingOrder order)
    {
    }

    public void Buy(string productId)
    {
        if (storeController == null)
        {
            Debug.LogError("IAP not initialized");
            return;
        }

        storeController.PurchaseProduct(productId);
 
    }
}
