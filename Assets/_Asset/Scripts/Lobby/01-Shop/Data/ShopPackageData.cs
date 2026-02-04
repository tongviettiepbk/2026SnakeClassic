using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Package_", menuName = "Scriptable Objects/Shop/Package")]

public class ShopPackageData : ScriptableObject
{
    public bool isLocked;
    public ShopPackageId id;
    public string title;
    public List<RewardData> rewards;
    public float priceByUsd;
}
