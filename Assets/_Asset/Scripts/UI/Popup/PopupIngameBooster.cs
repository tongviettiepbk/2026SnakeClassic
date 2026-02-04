using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class PopupIngameBooster : BaseUI
{
    public Image icBooster;
    public TMP_Text txdescription;
    public Button btClose;

    private ItemType type;

    protected override void Awake()
    {
        btClose.onClick.AddListener(ClickBtClose);
        //btClose.gameObject.SetActive(true);
    }

    public void Open(ItemType type)
    {
        this.gameObject.SetActive(true);
        LGamePlay.Instance.objListBooster.gameObject.SetActive(false);
        this.type = type;

        ItemData itemData = GameData.staticData.items.GetData(type);

        if (itemData != null)
        {
            icBooster.sprite = itemData.icon;
            txdescription.text = itemData.shortDescription;
        }
        btClose.gameObject.SetActive(true);
    }

    private void ClickBtClose()
    {
        MapController.Instance.CancelBoosterUse(type);
        LGamePlay.Instance.ReLoadFooter();
        Close();
    }

    public void DisableBtClose(bool isDisable =false)
    {
        btClose.gameObject.SetActive(!isDisable);
    }
}
