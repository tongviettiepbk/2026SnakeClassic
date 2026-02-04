using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class CellItemChoice : MonoBehaviour
{
    public CellReward cellReward;
    public Button btMinus;
    public Button btPlus;
    public TMP_Text txQuantity;
    public GameObject objFade;

    private LItemChoice controller;
    private RewardData rewardData;

    public int quantity { get; private set; }

    private void Awake()
    {
        btMinus.onClick.AddListener(ClickBtMinus);
        btPlus.onClick.AddListener(ClickBtAdd);
    }

    public void Load(LItemChoice controller, RewardData rewardData, bool isPreview)
    {
        this.controller = controller;
        this.rewardData = rewardData;

        if (rewardData != null)
        {
            cellReward.Load(rewardData);
            quantity = 0;
            Reload();
            txQuantity.transform.parent.gameObject.SetActive(!isPreview);
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void ClickBtMinus()
    {
        if (quantity > 0 && controller.Minus())
        {
            quantity--;
            Reload();
            controller.Reload();
        }
    }

    private void ClickBtAdd()
    {
        if (controller.Add())
        {
            quantity++;
            Reload();
            controller.Reload();
        }
    }

    public void Reload()
    {
        objFade.SetActive(quantity <= 0);
        txQuantity.text = quantity.ToString();
    }

    public RewardData GetRewards()
    {
        if (quantity > 0)
        {
            RewardData rw = rewardData.Clone();
            rw.quantity *= quantity;
            return rw;
        }

        return null;
    }
}
