using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemBooster : MonoBehaviour
{
    public GameObject objLockj;
    public GameObject objUnlock;
    public TMP_Text txLockLevel;

    [Space(20)]
    public Image icBooster;
    public Image bgBooster;
    public Sprite imgBg;
    public CanvasGroup canvasGroup;

    public Booster typeBooster; // loai booster
    public ItemType lastType;
    public ItemType type; // loai item

    public GameObject btAddBooster;
    public GameObject objQuantity;
    public TMP_Text txQuantity;
    public Button btBooster;

    public bool isLock = false;

    private long quantity;

    private void Awake()
    {
        btBooster.onClick.AddListener(ClickBtBooster);
        lastType = ItemType.NONE;
    }

    public void Initiliaze(long quantity)
    {
        this.quantity = quantity;
        ItemData itemData = GameData.staticData.items.GetData(type);
        if (itemData != null)
        {
            icBooster.sprite = itemData.icon;
            icBooster.SetNativeSize();
        }

        Reload(quantity);
    }

    public void ReloadIcon(ItemType type)
    {
        ItemData itemData = GameData.staticData.items.GetData(type);

        if (itemData != null)
        {
            //bgBooster.sprite = imgBg;
            icBooster.sprite = itemData.icon;
            icBooster.SetNativeSize();
        }
    }

    public void Reload(long quantity)
    {
        this.quantity = quantity;
        this.isLock = CheckLockItem();

        objLockj.SetActive(isLock);
        objUnlock.SetActive(!isLock);

        if (quantity == 0)
        {
            objQuantity.gameObject.SetActive(false);
            btAddBooster.gameObject.SetActive(true);
        }
        else
        {
            btAddBooster.gameObject.SetActive(false);
            objQuantity.gameObject.SetActive(true);
            txQuantity.text = quantity.ToString();
        }

    }

    public void ClickBtBooster()
    {
        if(isLock)
        {
            return;
        }

        if (quantity > 0)
        {
            if (lastType != ItemType.NONE)
            {
                MapController.Instance.CancelBoosterUse(lastType);
            }
            MapController.Instance.RequestStartCountTimeGame();

            lastType = type;

            if (type == ItemType.TIME_STOP_ICE)
            {
                btBooster.interactable = false;
                LGamePlay.Instance.StartFreezeTime();
                MapController.Instance.RequestUseBooster(type);
            }
            else if (type == ItemType.FIND_GECKO_MOVE)
            {
                MapController.Instance.RequestUseBooster(type);
            }
            else
            {
                MapController.Instance.RequestUseBooster(type);
                LGamePlay.Instance.popup.Open(type);
            }
        }
        else
        {
            LBuyBooster ui = UIManager.Instance.LoadUI(UIKey.BUYBOOSTER) as LBuyBooster;
            ui.Open(type);
        }
    }

    private bool CheckLockItem()
    {
        int levelLock = 100;

        switch (typeBooster)
        {
            case Booster.ESCAPES:
                levelLock = GameConfig.lvlUnlockEscapseGecko;

                break;

            case Booster.TIMESTOP:
                levelLock = GameConfig.lvlUnlockStopTime;
                break;
            case Booster.BRICK_DESTROY:
                levelLock = GameConfig.lvlUnlockDestroyIceHamer;
                break;
            case Booster.BARRIER_DESTROY:
                levelLock = GameConfig.lvlUnlockDestroyBarrier;
                break;
            case Booster.FIND_GECKO_MOVE:
                levelLock = GameConfig.lvlUnlockFindGeckoCanMove;
                break;
        }

        txLockLevel.text = "Lv." + levelLock.ToString();

        if (MapController.Instance.levelCurrent < levelLock)
        {
            return true;
        }

        return false;
    }

    public void SetLock(bool isLock)
    {
        objLockj.SetActive(isLock);
        objUnlock.SetActive(!isLock);
    }
}
