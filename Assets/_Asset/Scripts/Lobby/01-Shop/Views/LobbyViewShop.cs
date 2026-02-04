using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyViewShop : LobbyView
{
    public GameObject boxlist;
    public BoxPackageShop boxPrefabs;
    public List<BoxRemoveAds> boxRemoveAds = new List<BoxRemoveAds>();
    public List<BoxPackageShop> boxs = new List<BoxPackageShop>();
    public List<CellShopGold> cells = new List<CellShopGold>();

    protected override void OnEnable()
    {
        Reload();
    }

    public void Reload()
    {
        LoadBoxPackage();
        LoadCellPackage();
        LoadRemoveAds();
    }

    private void LoadBoxPackage()
    {
        ShopData data = GameData.staticData.shops.GetData(ShopId.SHOP);

        if (boxs.Count < data.packages.Length)
        {
            int package = data.packages.Length - boxs.Count;
            for (int i = 0; i < package; i++)
            {
                BoxPackageShop boxs = Instantiate(boxPrefabs, boxlist.gameObject.transform);
                boxs.Initialize(i);
            }
        }
    }

    private void LoadCellPackage()
    {
        for (int i = 0; i < cells.Count; i++)
        {
            var cell = cells[i];
            cell.gameObject.SetActive(true);
            cell.Initialize(i);
        }
    }

    private void LoadRemoveAds()
    {
        if (GameData.userData.shops.isPurchaseRemoveAds)
        {
            for (int i = 0; i < boxRemoveAds.Count; i++)
            {
                boxRemoveAds[i].gameObject.SetActive(false);
            }
            return;
        }

        for (int i = 0; i < boxRemoveAds.Count; i++)
        {
            var box = boxRemoveAds[i];
            box.gameObject.SetActive(true);
            box.Initialize(i, this);
        }
    }

}
