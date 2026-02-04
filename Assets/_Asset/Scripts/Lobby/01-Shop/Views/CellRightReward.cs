using System.Collections;
using UnityEngine;


public class CellRightReward : CellReward
{
    public override void Load(RewardData rewardData, bool isClickable = true, bool showBgCell = true)
    {
        base.Load(rewardData, isClickable, showBgCell);
        txQuantity.text = rewardData.quantity > 1 ? "x" + rewardData.quantity.ToStringNumber() : string.Empty;
    }
}
