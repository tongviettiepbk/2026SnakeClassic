using System;
using Unity.Profiling;
using UnityEngine.Purchasing;

public class ProductDefine
{

    //Bundle ingame 
    public static readonly ProductIAP BUNDLE_01 = new ProductIAP("com.blade.snake.bundle01", ProductType.Consumable);//5.99
    public static readonly ProductIAP BUNDLE_02 = new ProductIAP("com.blade.snake.bundle02", ProductType.Consumable);//5.99
    public static readonly ProductIAP BUNDLE_03 = new ProductIAP("com.blade.snake.bundle03", ProductType.Consumable);//5.99

    // Gems
    public static readonly ProductIAP REMOVE_ADS = new ProductIAP("com.blade.snake.removeads", ProductType.Consumable);//14.99
    public static readonly ProductIAP REMOVE_ADS_2 = new ProductIAP("com.blade.snake.removeads2", ProductType.Consumable);//9.99

    public static readonly ProductIAP MINI_PACK = new ProductIAP("com.blade.snake.minipack", ProductType.Consumable); //9.99
    public static readonly ProductIAP MEDIUM_PACK = new ProductIAP("com.blade.snake.mediumpack", ProductType.Consumable);//14.99
    public static readonly ProductIAP SUPER_PACK = new ProductIAP("com.blade.snake.superpack", ProductType.Consumable);//19.99
    public static readonly ProductIAP CHAMPION_PACK = new ProductIAP("com.blade.snake.chapmionpack", ProductType.Consumable);//49.99
    public static readonly ProductIAP GOD_PACK = new ProductIAP("com.blade.snake.godpack", ProductType.Consumable);//79.99


    public static readonly ProductIAP GOLD_1000 = new ProductIAP("com.blade.snake.gold1000", ProductType.Consumable); // 0.99
    public static readonly ProductIAP GOLD_5000 = new ProductIAP("com.blade.snake.gold5000", ProductType.Consumable); // 4.99
    public static readonly ProductIAP GOLD_10000 = new ProductIAP("com.blade.snake.gold10000", ProductType.Consumable); // 9.99
    public static readonly ProductIAP GOLD_25000 = new ProductIAP("com.blade.snake.gold25000", ProductType.Consumable); // 19.99
    public static readonly ProductIAP GOLD_50000 = new ProductIAP("com.blade.snake.gold50000", ProductType.Consumable); // 39.99
    public static readonly ProductIAP GOLD_100000 = new ProductIAP("com.blade.snake.gold100000", ProductType.Consumable); // 79.99

    private static readonly ProductIAP[] arr =
    {
        GOLD_1000,GOLD_5000,GOLD_10000,GOLD_25000,GOLD_50000,GOLD_100000,REMOVE_ADS,REMOVE_ADS_2,MINI_PACK,
        MEDIUM_PACK,SUPER_PACK,CHAMPION_PACK,GOD_PACK,BUNDLE_01,BUNDLE_02,BUNDLE_03
    };

    public static ProductIAP[] GetListProducts()
    {
        return arr;
    }


}

[Serializable]
public class IAPPayData
{
    public string payLoad;
    public string store;
    public string transactionID;
}

[Serializable]
public class IAPPayload
{
    public string json;
    public string signature;
    public IAPPayloadData payloadData;
}

[Serializable]
public class IAPPayloadData
{
    public string orderId;
    public string packagename;
    public string productId;
    public long purchaseTime;
    public int purchaseState;
    public string purchaseToken;
    public int quantity;
    public bool acknowledged;
}

