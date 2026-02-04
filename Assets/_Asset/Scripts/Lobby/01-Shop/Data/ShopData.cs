using System.Collections;
using UnityEngine;

public enum ShopId
{
    NONE = 0,
    SHOP = 1,
}
[CreateAssetMenu(fileName = "ShopData_", menuName = "Scriptable Objects/Shop/ShopData")]
public class ShopData : ScriptableObject
{
    public ShopId id;
    public ShopPackageData[] packages;
}
