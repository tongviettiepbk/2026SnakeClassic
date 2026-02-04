using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceGold : MonoBehaviour
{
    public TMP_Text txGold;
    public Button btShop;

    public bool isGnoreAutoLoad = false;

    private void Awake()
    {
        btShop.onClick.AddListener(ClickBtShop);
    }
    private void OnEnable()
    {
        LoadGold();
        EventDispatcher.Instance.RegisterListener(EventID.ADJUST_ITEM, OnAdjustItem);
    }
    private void OnDisable()
    {
        EventDispatcher.Instance.RemoveListener(EventID.ADJUST_ITEM, OnAdjustItem);
    }

    private void OnAdjustItem(object obj)
    {
        if (isGnoreAutoLoad)
        {
            return;
        }
        try
        {
            LoadGold();
        }
        catch { }
    }

    public void LoadGold()
    {
        double goldHave = ItemUtils.GetQuantityHave(ItemType.GOLD);
        txGold.text = goldHave.ToStringNumber();
    }

    private void ClickBtShop()
    {
        UIManager.Instance.LoadUI(UIKey.SHOP);
    }


}
