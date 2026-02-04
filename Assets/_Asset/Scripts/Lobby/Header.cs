using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class Header : MonoBehaviour
{
    public Image avatar;
    //public TMP_Text txName;
    //public TMP_Text txLevel;
    //public Image imgExp;
    public TMP_Text txHeart;
    public TMP_Text txGold;

    public Button btAddHeart;
    public Button btAddGold;

    private void Awake()
    {
        btAddHeart.onClick.AddListener(ClickBtAddHeart);
        btAddGold.onClick.AddListener(ClickBtAddGold);
    }

    private void OnEnable()
    {
        EventDispatcher.Instance.RegisterListener(EventID.ADJUST_ITEM, OnAdjustItem);

        LoadName();
        LoadAvatar();
        LoadLevel();
        LoadResources();
    }

    private void OnDisable()
    {
        EventDispatcher.Instance.RemoveListener(EventID.ADJUST_ITEM, OnAdjustItem);
    }

    private void OnAdjustItem(object obj)
    {
        try
        {
            LoadLevel();
            LoadResources();
        }
        catch { }
    }

    public void LoadName()
    {
        //txName.text = GameData.userData.profile.userName;
#if UNITY_EDITOR
        //txName.text = "LAMDEPTRAI";
#endif
    }

    public void LoadAvatar(object obj = null)
    {

    }

    public void LoadLevel()
    {
        //int level = GameData.userData.profile.GetAccountLevel();
        //txLevel.text = $"Lv. {level}";

        //float percent = GameData.userData.profile.GetAccountLevelProgress();
        //imgExp.fillAmount = percent;
    }

    public void LoadResources()
    {
        long heart = ItemUtils.GetQuantityHave(ItemType.HEART);
        if (heart <= 0)
        {
            heart = 0;
        }
        if (heart == GameConfig.HEART_MAX_RECOVERY)
        {
            string format = LocalizeManager.Instance.GetLocalizeText("FULL LIVE ");
            txHeart.text = string.Format(format);
        }

        txHeart.text = ((double)heart).ToStringNumber();
        long gold = ItemUtils.GetQuantityHave(ItemType.GOLD);
        txGold.text = ((double)gold).ToStringNumber();
    }

    private void ClickBtAddHeart()
    {
        UIManager.Instance.LoadUI(UIKey.MORELIFE);
    }

    private void ClickBtAddGold()
    {
        LobbyManager.Instance.ShowView(LobbyViewType.SHOP);
    }
}
