using NSubstitute.ReceivedExtensions;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LMoreLife : BaseUI
{
    public int costPerRefill = 900;
    public TMP_Text txHeart;
    public TMP_Text txResouces;
    public TMP_Text txGold;

    public Button btRefill;
    public Button bgBackground;
    public Button btClose;

    protected override void Awake()
    {
        base.Awake();
        btRefill.onClick.AddListener(ClickBtRefill);
        bgBackground.onClick.AddListener(Back);
        btClose.onClick.AddListener(Back);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        ReLoad();
    }

    private void ReLoad()
    {
        long heartHave = ItemUtils.GetQuantityHave(ItemType.HEART);
        if (heartHave <= 0)
        {
            heartHave = 0;
        }

        txHeart.text = heartHave.ToString();

        if (heartHave == GameConfig.HEART_MAX_RECOVERY)
        {
            string format = LocalizeManager.Instance.GetLocalizeText("FULL LIVE");
            txResouces.text = string.Format(format);
        }
        else
        {
            GetTimeRecoveryHeart();
        }

    }

    private void ClickBtRefill()
    {
        long heartHave = ItemUtils.GetQuantityHave(ItemType.HEART);
        if (heartHave >= GameConfig.HEART_MAX_RECOVERY)
        {
            UIManager.Instance.ShowToastMessage(ToastMessageType.HEART_FULL);
        }
        else
        {
            long goldHave = ItemUtils.GetQuantityHave(ItemType.GOLD);
            if (goldHave >= costPerRefill)
            {
                long quantity = GameConfig.HEART_MAX_RECOVERY - heartHave;
                ItemUtils.Receive(ItemType.HEART, quantity);
                ItemUtils.Consume(ItemType.GOLD, costPerRefill);
                ReLoad();
            }
            else
            {
                UIManager.Instance.ShowToastMessage(ToastMessageType.INSUFFICIENT_RESOURCES);
            }
        }
    }

    public void GetTimeRecoveryHeart()
    {

    }
}