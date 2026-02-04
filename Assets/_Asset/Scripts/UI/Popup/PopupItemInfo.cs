using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class PopupItemInfo : MonoBehaviour
{
    public CellReward cellReward;
    public TMP_Text txName;
    public TMP_Text txDescription;
    public Button btBackground;
    public Button btUse;

    private RewardData data;
    private bool showButtonUse;

    private void Awake()
    {
        btBackground.onClick.AddListener(Close);
        btUse.onClick.AddListener(ClickBtUse);
    }

    public void Open(RewardData data, bool showButtonUse)
    {
        this.data = data;
        this.showButtonUse = showButtonUse;

        try
        {
            cellReward.Load(data, isClickable: false);
            cellReward.txQuantity.text = string.Empty;

            Tuple<string, string> info = data.GetInfo();

            txName.text = info.Item1;
            txDescription.text = info.Item2;

            bool isUsable = false;

            if (data.isNormalItem)
            {
                ItemData itemData = GameData.staticData.items.GetData(data.type);
                isUsable = itemData != null && itemData.isUsable;
            }
            else
            {
            }

            btUse.gameObject.SetActive(showButtonUse && isUsable);
            gameObject.SetActive(true);
        }
        catch
        {
            gameObject.SetActive(false);
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void ClickBtUse()
    {
        Close();

        LItemChoice ui = UIManager.Instance.LoadUI(UIKey.ITEM_CHOICE) as LItemChoice;
        ui.Open(data, !showButtonUse);
    }
}
