using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class LItemChoice : BaseUI
{
    public Button btBackground;
    public Button btClose;
    public CellReward cellReward;
    public TMP_Text txName;
    public TMP_Text txQuantity;
    public CellItemChoice prefabCell;
    public ScrollRect scrollRect;
    public Button btUse;

    private int maxUse;
    private RewardData rewardData;
    private List<CellItemChoice> cells = new List<CellItemChoice>();

    protected override void Awake()
    {
        base.Awake();
        btBackground.onClick.AddListener(Back);
        btClose.onClick.AddListener(Back);
        btUse.onClick.AddListener(ClickBtUse);
    }

    public void Open(RewardData rewardData, bool isPreview)
    {
        this.rewardData = rewardData;

        if (rewardData == null)
        {
            Close();
            return;
        }

        cellReward.Load(rewardData, isClickable: false);
        cellReward.txQuantity.gameObject.SetActive(false);
        txName.text = rewardData.GetInfo().Item1;
        maxUse = (int)rewardData.quantity;

        List<RewardData> rewards = rewardData.GetChoiceRewards();
        if (rewards.Count > cells.Count)
        {
            int needMore = rewards.Count - cells.Count;
            for (int i = 0; i < needMore; i++)
            {
                CellItemChoice element = Instantiate(prefabCell, scrollRect.content);
                cells.Add(element);
            }
        }

        for (int i = 0; i < cells.Count; i++)
        {
            CellItemChoice element = cells[i];
            element.gameObject.SetActive(false);
            if (i < rewards.Count)
            {
                element.Load(this, rewards[i], isPreview);
            }
        }

        Reload();
        btUse.gameObject.SetActive(!isPreview);

        scrollRect.enabled = rewards.Count > 8;
        Vector2 v = scrollRect.content.anchoredPosition;
        v.y = 0f;
        scrollRect.content.anchoredPosition = v;

        gameObject.SetActive(true);
    }

    public void Reload()
    {
        int curUse = GetCurUse();
        txQuantity.text = string.Format("{0}/{1}", curUse, maxUse);
    }

    private int GetCurUse()
    {
        int curUse = 0;

        for (int i = 0; i < cells.Count; i++)
        {
            CellItemChoice element = cells[i];
            curUse += element.quantity;
        }

        return curUse;
    }

    public bool Add()
    {
        int curUse = GetCurUse();
        if (curUse < maxUse)
        {
            return true;
        }

        return false;
    }

    public bool Minus()
    {
        int curUse = GetCurUse();
        if (curUse > 0)
        {
            return true;
        }

        return false;
    }

    private void ClickBtUse()
    {
        int curUse = 0;
        List<RewardData> rewards = new List<RewardData>();

        for (int i = 0; i < cells.Count; i++)
        {
            CellItemChoice element = cells[i];
            if (element.quantity > 0)
            {
                curUse += element.quantity;
                rewards.Add(element.GetRewards());
            }
        }

        if (curUse > 0)
        {
            ItemUtils.Receive(rewards);

            switch (rewardData.type)
            {
            }


            Close();
        }
    }
}
