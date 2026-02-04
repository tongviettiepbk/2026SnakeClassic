using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CellReward : MonoBehaviour
{
    public Button buttonComponent;
    public CanvasGroup canvasGroup;
    public Animation anim;
    public Image icItem;
    public TMP_Text txQuantity;
    public GameObject objCompleted;

    public RewardData rewardData { get; private set; }

    private AnimatedButton animatedButton;

    private void Awake()
    {
        animatedButton = GetComponent<AnimatedButton>();

        buttonComponent.onClick.AddListener(OnClick);
    }

    public virtual void Load(RewardData rewardData, bool isClickable = true, bool showBgCell = true)
    {
        this.rewardData = rewardData;
        if (rewardData != null)
        {
            icItem.gameObject.SetActive(rewardData.isNormalItem);
            if (rewardData.isNormalItem)
            {
                ItemData itemData = GameData.staticData.items.GetData(rewardData.type);
                if (itemData != null)
                {
                    icItem.sprite = itemData.icon;
                }
            }

            buttonComponent.image.SetAlpha(showBgCell ? 1f : 0f);

            switch (rewardData.type)
            {
            }

            txQuantity.text = rewardData.quantity == 1
                ? string.Empty
                : rewardData.quantity.ToStringNumber();

            SetCompleted(false);
            buttonComponent.enabled = isClickable;
            buttonComponent.image.raycastTarget = isClickable;

            if (animatedButton != null)
                animatedButton.enabled = isClickable;

            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void SetCompleted(bool isOn)
    {
        objCompleted.SetActive(isOn);
    }

    private void OnClick()
    {
        if (rewardData != null)
        {

            switch (rewardData.type)
            {
                case ItemType.NONE:
                    {
                        //GearBattleData battleData = new GearBattleData(rewardData.gear);
                        //LGearInfo ui = UIManager.Instance.LoadUI(UIKey.GEAR_INFO, isOverlay: true) as LGearInfo;
                        //ui.Open(battleData);
                    }
                    break;

                default:
                    UIManager.Instance.ShowItemInfo(rewardData);
                    break;
            }
        }
    }
}
